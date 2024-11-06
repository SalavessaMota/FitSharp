﻿using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Entities;
using FitSharp.Helpers;
using FitSharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
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
    private readonly IGymRepository _gymRepository;
    private readonly IMembershipRepository _membershipRepository;
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;

    public AdminController(
        IUserHelper userHelper,
        IBlobHelper blobHelper,
        IMailHelper mailHelper,
        ICountryRepository countryRepository,
        IGymRepository gymRepository,
        IMembershipRepository membershipRepository,
        IUserRepository userRepository,
        IConfiguration configuration)
    {
        _userHelper = userHelper;
        _blobHelper = blobHelper;
        _mailHelper = mailHelper;
        _countryRepository = countryRepository;
        _gymRepository = gymRepository;
        _membershipRepository = membershipRepository;
        _userRepository = userRepository;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userRepository.GetAllUsersWithCityAndCountry().ToListAsync();
        var loggedUser = await _userRepository.GetUserByEmailAsync(User.Identity.Name);
        var model = new List<AdminUserViewModel>();

        foreach (var user in users)
        {
            var roles = await _userHelper.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault();

            model.Add(new AdminUserViewModel
            {
                UserId = user.Id,
                Fullname = user.FullName,
                Email = user.Email,
                Role = userRole,
                Address = user.Address,
                CityName = user.City?.Name,
                CountryName = user.City?.Country.Name,
                ImageFullPath = user.ImageFullPath,
                IsActive = user.IsActive
            });
        }

        return View(model);
    }


    public async Task<IActionResult> RegisterEmployee()
    {
        var model = new AdminRegisterNewEmployeeViewModel
        {
            Countries = _countryRepository.GetComboCountries(),
            Cities = await _countryRepository.GetComboCitiesAsync(1),
            Gyms = _gymRepository.GetComboGyms()
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterEmployee(AdminRegisterNewEmployeeViewModel model)
    {
        //TODO: Add email already used message.

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
                    CityId = model.CityId,
                    IsActive = true
                };

                var result = await _userRepository.AddUserAsync(user, "Fitsharp1!");
                await _userHelper.AddUserToRoleAsync(user, "Employee");

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);

                var employee = new Employee { User = user, GymId = model.GymId };
                await _userRepository.AddEmployeeAsync(employee);

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                string tokenLink = Url.Action("SetPassword", "Account", new
                {
                    token = myToken,
                    email = user.Email
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

                    ModelState.Clear();

                    return View(model = new AdminRegisterNewEmployeeViewModel
                    {
                        Countries = _countryRepository.GetComboCountries(),
                        Cities = await _countryRepository.GetComboCitiesAsync(1),
                        Gyms = _gymRepository.GetComboGyms()
                    });
                }

                return RedirectToAction("Index", "Admin");
            }
        }

        model.Countries = _countryRepository.GetComboCountries();
        model.Cities = await _countryRepository.GetComboCitiesAsync(1);
        model.Gyms = _gymRepository.GetComboGyms();

        return View(model);
    }

    public async Task<IActionResult> RegisterInstructor()
    {
        var model = new AdminRegisterNewInstructorViewModel
        {
            Countries = _countryRepository.GetComboCountries(),
            Cities = await _countryRepository.GetComboCitiesAsync(1),
            Gyms = _gymRepository.GetComboGyms()
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterInstructor(AdminRegisterNewInstructorViewModel model)
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
                    CityId = model.CityId,
                    IsActive = true
                };

                var result = await _userRepository.AddUserAsync(user, "Fitsharp1!");
                await _userHelper.AddUserToRoleAsync(user, "Instructor");

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);

                var instructor = new Instructor
                {
                    User = user,
                    GymId = model.GymId,
                    Speciality = model.Speciality,
                    Description = model.Description
                };
                await _userRepository.AddEmployeeAsync(instructor);

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                string tokenLink = Url.Action("SetPassword", "Account", new
                {
                    token = myToken,
                    email = user.Email
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

                    ModelState.Clear();

                    return View(model = new AdminRegisterNewInstructorViewModel
                    {
                        Countries = _countryRepository.GetComboCountries(),
                        Cities = await _countryRepository.GetComboCitiesAsync(1),
                        Gyms = _gymRepository.GetComboGyms()
                    });
                }

                model.Countries = _countryRepository.GetComboCountries();
                model.Cities = await _countryRepository.GetComboCitiesAsync(1);
                model.Gyms = _gymRepository.GetComboGyms();

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
                    CityId = model.CityId,
                    IsActive = true
                };

                var result = await _userRepository.AddUserAsync(user, "Fitsharp1!");
                await _userHelper.AddUserToRoleAsync(user, "Customer");

                var customer = new Customer { User = user };
                await _userRepository.AddCustomerAsync(customer);

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                string tokenLink = Url.Action("SetPassword", "Account", new
                {
                    token = myToken,
                    email = user.Email
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendEmail(model.Username, "FitSharp - Set your Password",
                                        $"<h1 style=\"color:#1E90FF;\">Welcome to FitSharp!</h1>" +
                                        $"<p>Your admin account has been created by an authorized personnel.</p>" +
                                        $"<p>To complete your registration, please set your password by clicking the link below:</p>" +
                                        $"<p><a href = \"{tokenLink}\" style=\"color:#FFA500; font-weight:bold;\">Set Password</a></p>" +
                                        $"<p>If you didn’t expect this registration or believe it was a mistake, please contact us or disregard this        email.</p>" +
                                        $"<br>" +
                                        $"<p>Best regards,</p>" +
                                        $"<p>The FitSharp Team</p>" +
                                        $"<p><small>This is an automated message. Please do not reply to this email.</small></p>");

                if (response.IsSuccess)
                {
                    ViewBag.Message = "The account has been created, and the user has been sent an email to set their password.";

                    ModelState.Clear();

                    return View( model = new AdminRegisterNewUserViewModel
                    {
                        Countries = _countryRepository.GetComboCountries(),
                        Cities = await _countryRepository.GetComboCitiesAsync(1)
                    });
                }
                return RedirectToAction("Index", "Admin");
            }
        }

        model.Countries = _countryRepository.GetComboCountries();
        model.Cities = await _countryRepository.GetComboCitiesAsync(1);

        return View(model);
    }

    public async Task<IActionResult> RegisterAdmin()
    {
        var model = new AdminRegisterNewAdminViewModel
        {
            Countries = _countryRepository.GetComboCountries(),
            Cities = await _countryRepository.GetComboCitiesAsync(1)
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterAdmin(AdminRegisterNewAdminViewModel model)
    {
        if (ModelState.IsValid)
        {
            var adminPassword = _configuration["AdminPassword"];
            if (model.AdminPassword != adminPassword)
            {
                ModelState.AddModelError("", "Invalid admin registration password.");
                model.Countries = _countryRepository.GetComboCountries();
                model.Cities = await _countryRepository.GetComboCitiesAsync(model.CountryId);
                return View(model);
            }

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
                    CityId = model.CityId,
                    IsActive = true
                };

                var result = await _userRepository.AddUserAsync(user, "Fitsharp1!");
                await _userHelper.AddUserToRoleAsync(user, "Admin");

                var admin = new Admin { User = user };
                await _userRepository.AddAdminAsync(admin);

                var token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                string tokenLink = Url.Action("SetPassword", "Account", new
                {
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendEmail(model.Username, "FitSharp - Set your Password",
                                        $"<h1 style=\"color:#1E90FF;\">Welcome to FitSharp!</h1>" +
                                        $"<p>Your admin account has been created by an authorized personnel.</p>" +
                                        $"<p>To complete your registration, please set your password by clicking the link below:</p>" +
                                        $"<p><a href = \"{tokenLink}\" style=\"color:#FFA500; font-weight:bold;\">Set Password</a></p>" +
                                        $"<p>If you didn’t expect this registration or believe it was a mistake, please contact us or disregard this        email.</p>" +
                                        $"<br>" +
                                        $"<p>Best regards,</p>" +
                                        $"<p>The FitSharp Team</p>" +
                                        $"<p><small>This is an automated message. Please do not reply to this email.</small></p>");

                if (response.IsSuccess)
                {
                    ViewBag.Message = "The admin account has been created, and the user has been sent an email to set their password.";

                    ModelState.Clear();

                    return View(new AdminRegisterNewAdminViewModel
                    {
                        Countries = _countryRepository.GetComboCountries(),
                        Cities = await _countryRepository.GetComboCitiesAsync(1)
                    });
                }                               

                return RedirectToAction("Index", "Admin");
            }
        }

        model.Countries = _countryRepository.GetComboCountries();
        model.Cities = await _countryRepository.GetComboCitiesAsync(model.CountryId);

        return View(model);
    }

    public async Task<IActionResult> EditUser(string id)
    {
        var user = await _userRepository.GetUserWithCountryAndCityByIdAsync(id);
        if (user == null) return new NotFoundViewResult("UserNotFound");

        var model = new AdminEditUserViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Address = user.Address,
            PhoneNumber = user.PhoneNumber,
            UserType = _userRepository.DetermineUserType(user),
            CityId = user.CityId,
            CountryId = user.City.Country.Id
        };

        switch (model.UserType)
        {
            case "Customer":
                var customer = await _userRepository.GetCustomerByUserIdAsync(user.Id);
                if (customer != null)model.MembershipId = customer.MembershipId;
                break;

            case "Employee":
                var employee = await _userRepository.GetEmployeeByUserIdAsync(user.Id);
                if (employee != null)
                {
                    model.GymId = employee.GymId;
                }
                break;

            case "Instructor":
                var instructor = await _userRepository.GetInstructorByUserIdAsync(user.Id);
                if (instructor != null)
                {
                    model.GymId = instructor.GymId;
                    model.Speciality = instructor.Speciality;
                    model.Description = instructor.Description;
                }
                break;

            case "Admin":
                break;
        }

        // Carregar países e cidades
        model.Countries = _countryRepository.GetComboCountries();
        model.Cities = model.CountryId > 0 ? await _countryRepository.GetComboCitiesAsync(model.CountryId) : null;
        model.Gyms = _gymRepository.GetComboGyms();
        model.Memberships = _membershipRepository.GetComboMemberships();

        return View(model);
    }


    [HttpPost]
    public async Task<IActionResult> EditUser(AdminEditUserViewModel model, string id)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null) return new NotFoundViewResult("UserNotFound");

        // Atualizar campos comuns
        user.FirstName = model.FirstName;
        user.LastName = model.LastName;
        user.Address = model.Address;
        user.CityId = model.CityId;        
        user.PhoneNumber = model.PhoneNumber;

        if(model.UserType == "Customer") 
        {
            var customer = await _userRepository.GetCustomerByUserIdAsync(user.Id);
            if (customer != null)
            {
                customer.MembershipId = model.MembershipId;
                await _userRepository.UpdateCustomerAsync(customer);
            }
        }


        // Atualizar campos específicos por tipo
        switch (model.UserType)
        {
            case "Customer":
                var customer = await _userRepository.GetCustomerByUserIdAsync(user.Id);
                if (customer != null)
                {
                    customer.MembershipId = model.MembershipId;
                    await _userRepository.UpdateCustomerAsync(customer);
                }
                break;

            case "Employee":
                var employee = await _userRepository.GetEmployeeByUserIdAsync(user.Id);
                if (employee != null) employee.GymId = model.GymId;
                break;

            case "Instructor":
                var instructor = await _userRepository.GetInstructorByUserIdAsync(user.Id);
                if (instructor != null)
                {
                    instructor.GymId = model.GymId;
                    instructor.Speciality = model.Speciality;
                    instructor.Description = model.Description;
                }
                break;
        }

        await _userRepository.UpdateUserAsync(user);
        TempData["Success"] = "User profile updated successfully.";
        return RedirectToAction("Index");
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

        // Inicializa o modelo com os dados básicos do usuário
        var adminUserViewModel = new AdminEditUserViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Address = user.Address,
            PhoneNumber = user.PhoneNumber,
            CityId = user.CityId,
            City = city,
            Country = city.Country,
            CountryId = city.CountryId,
            UserType = _userRepository.DetermineUserType(user),
            ImageId = user.ImageId
        };

        // Obter a entidade associada ao usuário (Customer, Employee, Instructor, Admin)
        var entity = await _userRepository.GetEntityByUserIdAsync(user.Id);
        adminUserViewModel.Entity = entity;

        // Preenchendo propriedades específicas com base no tipo de usuário
        if (entity is Customer customer)
        {
            adminUserViewModel.MembershipId = customer.MembershipId;
            adminUserViewModel.Membership = customer.Membership;
        }
        else if (entity is Employee employee)
        {
            adminUserViewModel.GymId = employee.GymId;
            adminUserViewModel.GymName = employee.Gym?.Name;

            if (entity is Instructor instructor)
            {
                adminUserViewModel.Speciality = instructor.Speciality;
                adminUserViewModel.Description = instructor.Description;
            }
        }
        
        // Caso para Admin - se houver propriedades específicas, adiciona-as aqui

        return View(adminUserViewModel);
    }


    [HttpPost]
    public async Task<IActionResult> DisableUser(string id, string adminPassword)
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

        var storedAdminPassword = _configuration["AdminPassword"];

        if (adminPassword != storedAdminPassword)
        {
            TempData["Error"] = "Incorrect password. Unable to deactivate the user.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            user.IsActive = false;
            await _userRepository.UpdateUserAsync(user);

            // Remover roles e da tabela específica
            await _userHelper.RemoveRolesAsync(user, await _userHelper.GetRolesAsync(user));

            // Verificar e remover o usuário da tabela específica
            await _userRepository.RemoveFromSpecificTableAsync(user);


            TempData["Success"] = "User has been successfully deactivated.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
            {
                ViewBag.ErrorTitle = "An error occurred while trying to disable the user.";
                ViewBag.ErrorMessage = "The user may still be referenced in the system.";
            }

            return View("Error");
        }
    }

    [HttpPost]
    public async Task<IActionResult> ActivateUser(string id, string adminPassword, string role)
    {
        if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(role))
        {
            TempData["Error"] = "User ID or Role is missing.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userRepository.GetUserByIdAsync(id);
        if (user == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var storedAdminPassword = _configuration["AdminPassword"];
        if (adminPassword != storedAdminPassword)
        {
            TempData["Error"] = "Incorrect administrator password.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            user.IsActive = true;
            await _userRepository.UpdateUserAsync(user);

            // Adicionar role e incluir o usuário na tabela específica
            await _userHelper.RemoveRolesAsync(user, await _userHelper.GetRolesAsync(user));
            await _userHelper.AddUserToRoleAsync(user, role);
            await _userRepository.AddToSpecificTableAsync(user, role);

            TempData["Success"] = $"User successfully activated with role: {role}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"An error occurred while activating the user: {ex.Message}";
            return RedirectToAction(nameof(Index));
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