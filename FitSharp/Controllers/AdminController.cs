using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Entities;
using FitSharp.Helpers;
using FitSharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserHelper _userHelper;
    private readonly IBlobHelper _blobHelper;
    private readonly IMailHelper _mailHelper;
    private readonly ICountryRepository _countryRepository;
    private readonly IUserRepository _userRepository;

    public AdminController(
        IUserHelper userHelper,
        IBlobHelper blobHelper,
        IMailHelper mailHelper,
        ICountryRepository countryRepository,
        IUserRepository userRepository)
    {
        _userHelper = userHelper;
        _blobHelper = blobHelper;
        _mailHelper = mailHelper;
        _countryRepository = countryRepository;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userRepository.GetAllUsersWithCityAndCountry().ToListAsync();

        var loggedUser = await _userRepository.GetUserByEmailAsync(User.Identity.Name);

        var model = new List<EditUserRolesViewModel>();

        foreach (var user in users)
        {
            var roles = await _userHelper.GetRolesAsync(user);

            model.Add(new EditUserRolesViewModel
            {
                UserId = user.Id,
                Username = user.FullName,
                Email = user.Email,
                Roles = roles.Select(r => new UserRoleViewModel { RoleName = r }).ToList(),
                SelectedRole = roles.FirstOrDefault(),
                Address = user.Address,
                CityName = user.City?.Name,
                CountryName = user.City?.Country.Name,
                ImageFullPath = user.ImageFullPath
            });
        }

        return View(model);
    }

    public async Task<IActionResult> EditUserRoles(string id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            return new NotFoundViewResult("UserNotFound");
        }

        var userRoles = await _userHelper.GetRolesAsync(user);
        var userRole = userRoles.FirstOrDefault();

        var model = new EditUserRolesViewModel
        {
            UserId = user.Id,
            Username = user.UserName,
            Email = user.Email,
            Roles = _userHelper.GetAllRoles().Select(u => new UserRoleViewModel
            {
                RoleName = u.Name
            }).ToList(),
            SelectedRole = userRole
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditUserRoles(EditUserRolesViewModel model)
    {
        var user = await _userRepository.GetUserByIdAsync(model.UserId);
        if (user == null)
        {
            return new NotFoundViewResult("UserNotFound");
        }

        if (string.IsNullOrEmpty(model.SelectedRole))
        {
            ModelState.AddModelError("", "Please select a role.");
            model.Roles = _userHelper.GetAllRoles().Select(u => new UserRoleViewModel
            {
                RoleName = u.Name
            }).ToList();
            return View(model);
        }

        var roles = await _userHelper.GetRolesAsync(user);
        var result = await _userHelper.RemoveRolesAsync(user, roles);
        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Failed to remove existing roles.");
            model.Roles = _userHelper.GetAllRoles().Select(u => new UserRoleViewModel
            {
                RoleName = u.Name
            }).ToList();
            return View(model);
        }

        var addResult = await _userHelper.AddUserToRoleAsync(user, model.SelectedRole);
        if (!addResult.Succeeded)
        {
            ModelState.AddModelError("", "Failed to add the selected role.");
            model.Roles = _userHelper.GetAllRoles().Select(u => new UserRoleViewModel
            {
                RoleName = u.Name
            }).ToList();
            return View(model);
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> RegisterEmployee()
    {
        var model = new AdminRegisterNewUserViewModel
        {
            Countries = _countryRepository.GetComboCountries(),
            Cities = await _countryRepository.GetComboCitiesAsync(1)
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterEmployee(AdminRegisterNewUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userRepository.GetUserByEmailAsync(model.Username);
            if (user == null)
            {
                var city = await _countryRepository.GetCityAsync(model.CityId);

                user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Username,
                    UserName = model.Username,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    CityId = model.CityId
                };

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                    user.ImageId = imageId;
                }

                var result = await _userRepository.AddUserAsync(user, "123123");
                await _userHelper.AddUserToRoleAsync(user, "Employee");

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                string tokenLink = Url.Action("ResetPassword", "Account", new
                {
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendEmail(model.Username, "FitSharp - Set Your Password",
                        $"<h1 style=\"color:#1E90FF;\">Welcome to FitSharp!</h1>" +
                        $"<p>Your account has been created by an administrator on behalf of FitSharp, your trusted platform for fitness and wellness.</p>" +
                        $"<p>To complete your registration, please set your password by clicking the link below:</p>" +
                        $"<p><a href = \"{tokenLink}\" style=\"color:#FFA500; font-weight:bold;\">Set Password</a></p>" +
                        $"<p>If you weren’t expecting this registration or believe it was a mistake, please contact us or disregard this email.</p>" +
                        $"<br>" +
                        $"<p>Best regards,</p>" +
                        $"<p>The FitSharp Team</p>" +
                        $"<p><small>This is an automated message. Please do not reply to this email.</small></p>");


                if (response.IsSuccess)
                {
                    ViewBag.Message = "The account has been created, and the user has been sent an email to set their password.";
                    return View(model);
                }

                return RedirectToAction("Index", "Admin");
            }
        }

        return View(model);
    }

    public async Task<IActionResult> RegisterCustomer()
    {
        var model = new AdminRegisterNewUserViewModel
        {
            Countries = _countryRepository.GetComboCountries(),
            Cities = await _countryRepository.GetComboCitiesAsync(1)
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterCustomer(AdminRegisterNewUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userRepository.GetUserByEmailAsync(model.Username);
            if (user == null)
            {
                var city = await _countryRepository.GetCityAsync(model.CityId);

                user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Username,
                    UserName = model.Username,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    CityId = model.CityId
                };

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                    user.ImageId = imageId;
                }

                var result = await _userRepository.AddUserAsync(user, "123123");
                await _userHelper.AddUserToRoleAsync(user, "Customer");

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                string tokenLink = Url.Action("ResetPassword", "Account", new
                {
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendEmail(model.Username, "AirCinel - Set your Password",
                                        $"<h1 style=\"color:#1E90FF;\">Welcome to AirCinel!</h1>" +
                                        $"<p>Your account has been created by an administrator on behalf of AirCinel, your trusted airline for premium travel experiences.</p>" +
                                        $"<p>To complete your registration, please set your password by clicking the link below:</p>" +
                                        $"<p><a href = \"{tokenLink}\" style=\"color:#FFA500; font-weight:bold;\">Set Password</a></p>" +
                                        $"<p>If you didn’t expect this registration or believe it was a mistake, please contact us or disregard this email.</p>" +
                                        $"<br>" +
                                        $"<p>Safe travels,</p>" +
                                        $"<p>The AirCinel Team</p>" +
                                        $"<p><small>This is an automated message. Please do not reply to this email.</small></p>");

                if (response.IsSuccess)
                {
                    ViewBag.Message = "The account has been created, and the user has been sent an email to set their password.";
                    return View(model);
                }

                return RedirectToAction("Index", "Admin");
            }
        }

        return View(model);
    }

    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userRepository.GetUserByIdAsync(id);

        if (user == null)
        {
            return new NotFoundViewResult("UserNotFound");
        }

        var model = new UserViewModel();
        if (user != null)
        {
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;
            model.Address = user.Address;
            model.PhoneNumber = user.PhoneNumber;

            var city = await _countryRepository.GetCityAsync(user.CityId);
            if (city != null)
            {
                var country = await _countryRepository.GetCountryAsync(city);
                if (country != null)
                {
                    model.CountryId = country.Id;
                    model.CityId = city.Id;

                    // Populando as listas de países e cidades
                    model.Countries = _countryRepository.GetComboCountries();
                    model.Cities = await _countryRepository.GetComboCitiesAsync(country.Id);
                }
            }
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> EditUser(UserViewModel model, string id)
    {
        if (ModelState.IsValid)
        {
            var user = await _userRepository.GetUserByIdAsync(id);
            if (user != null)
            {
                var city = await _countryRepository.GetCityAsync(model.CityId);

                // Atualiza os dados do usuário
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Address = model.Address;
                user.PhoneNumber = model.PhoneNumber;
                user.CityId = model.CityId;
                user.City = city;

                // Verifica se há uma nova imagem para upload
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                    user.ImageId = imageId;
                }

                var response = await _userRepository.UpdateUserAsync(user);
                if (response.Succeeded)
                {
                    ViewBag.UserMessage = "User updated!";
                }
                else
                {
                    ModelState.AddModelError(string.Empty, response.Errors.FirstOrDefault()?.Description);
                }
            }
        }

        // Recarrega as listas de países e cidades
        model.Countries = _countryRepository.GetComboCountries();
        if (model.CountryId != 0)
        {
            model.Cities = await _countryRepository.GetComboCitiesAsync(model.CountryId);
        }

        return View(model);
    }

    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
        {
            return new NotFoundViewResult("UserNotFound");
        }

        var user = await _userRepository.GetUserWithCountryAndCityByIdAsync(id);
        if (user == null)
        {
            return new NotFoundViewResult("UserNotFound");
        }

        var city = await _countryRepository.GetCityAsync(user.CityId);
        if (city == null)
        {
            return new NotFoundViewResult("UserNotFound");
        }
        user.City = city;

        return View(user);
    }

    public async Task<IActionResult> DeleteUser(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return new NotFoundViewResult("UserNotFound");
        }

        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            return new NotFoundViewResult("UserNotFound");
        }

        try
        {
            await _userRepository.DeleteUserAsync(user);
            await _userHelper.RemoveRolesAsync(user, await _userHelper.GetRolesAsync(user));
            await _blobHelper.DeleteBlobAsync("users", user.ImageId.ToString());
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
            {
                ViewBag.ErrorTitle = $"This User is probably being used!";
                ViewBag.ErrorMessage = $"This User can't be deleted because he has tickets bought.</br></br>";
            }

            return View("Error");
        }
    }

    public IActionResult UserNotFound()
    {
        return View();
    }

    [HttpPost]
    [Route("Admin/GetCitiesAsync")]
    public async Task<IActionResult> GetCitiesAsync(int countryId)
    {
        var country = await _countryRepository.GetCountryWithCitiesAsync(countryId);
        return Json(country.Cities.OrderBy(c => c.Name).Select(c => new
        {
            Id = c.Id,
            Name = c.Name
        }));
    }
}