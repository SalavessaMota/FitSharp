﻿using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Entities;
using FitSharp.Helpers;
using FitSharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QRCoder;
using System;
using System.Drawing.Imaging;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Vereyon.Web;

namespace FitSharp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserHelper _userHelper;
        private readonly IMailHelper _mailHelper;
        private readonly IConfiguration _configuration;
        private readonly ICountryRepository _countryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IBlobHelper _blobHelper;
        private readonly IFlashMessage _flashMessage;

        public AccountController(
            IUserHelper userHelper,
            IMailHelper mailHelper,
            IConfiguration configuration,
            ICountryRepository countryRepository,
            IUserRepository userRepository,
            IBlobHelper blobHelper,
            IFlashMessage flashMessage)
        {
            _userHelper = userHelper;
            _mailHelper = mailHelper;
            _configuration = configuration;
            _countryRepository = countryRepository;
            _userRepository = userRepository;
            _blobHelper = blobHelper;
            _flashMessage = flashMessage;
        }

        public IActionResult Login()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        public async Task CheckMembershipStatus()
        {
            var customer = await _userRepository.GetCustomerByUserName(User.Identity.Name);
            if (customer.MembershipIsActive == true && customer.MembershipEndDate <= DateTime.Now)
            {
                customer.MembershipIsActive = false;
                await _userRepository.UpdateCustomerAsync(customer);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userHelper.LoginAsync(model);

                if (result.Succeeded)
                {
                    var user = await _userRepository.GetUserByEmailAsync(model.Username);

                    // Verificar se o utilizador existe
                    if (user == null)
                    {
                        await _userHelper.LogoutAsync();
                        _flashMessage.Danger("The user does not exist. Please check your credentials.");
                        return View(model);
                    }

                    // Verificar se o utilizador está ativo
                    if (!user.IsActive)
                    {
                        await _userHelper.LogoutAsync();
                        _flashMessage.Danger("The account is not active, please contact administration.");
                        return View(model);
                    }

                    // Redirecionar para a URL de retorno, se disponível
                    if (this.Request.Query.Keys.Contains("ReturnUrl"))
                    {
                        return Redirect(this.Request.Query["ReturnUrl"].First());
                    }

                    // Verificar a role "Customer" e a validade da subscrição
                    if (await _userHelper.IsUserInRoleAsync(user, "Customer"))
                    {
                        var customer = await _userRepository.GetCustomerByUserName(user.UserName);

                        if (customer != null && customer.MembershipIsActive && customer.MembershipEndDate <= DateTime.Now)
                        {
                            customer.MembershipIsActive = false;
                            customer.MembershipId = null;
                            customer.Membership = null;
                            customer.ClassesRemaining = 0;
                            await _userRepository.UpdateCustomerAsync(customer);
                        }
                    }

                    return this.RedirectToAction("Index", "Home");
                }
            }

            // Mensagem de erro para credenciais inválidas
            _flashMessage.Danger("Failed to login. Please check your username and password.");
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _userHelper.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Register()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new RegisterNewUserViewModel
            {
                Countries = _countryRepository.GetComboCountries(),
                Cities = await _countryRepository.GetComboCitiesAsync(0)
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterNewUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepository.GetUserByEmailAsync(model.Username);
                if (user != null)
                {
                    ModelState.AddModelError(string.Empty, "The email is already registered.");
                    return View(model);
                }

                var city = await _countryRepository.GetCityAsync(model.CityId);

                user = new User
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Username,
                    UserName = model.Username,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    TaxNumber = model.TaxNumber,
                    CityId = model.CityId,
                    IsActive = true
                };

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var imageId = await _blobHelper.UploadBlobAsync(model.ImageFile, "users");
                    user.ImageId = imageId;
                }

                var result = await _userRepository.AddUserAsync(user, model.Password);
                if (result != IdentityResult.Success)
                {
                    ModelState.AddModelError(string.Empty, "The user couldn't be created.");
                    return View(model);
                }
                await _userHelper.AddUserToRoleAsync(user, "Customer");

                var customer = new Customer
                {
                    User = user,
                    MembershipIsActive = true,
                    MembershipBeginDate = DateTime.Now,
                    MembershipEndDate = DateTime.Now.AddMonths(1),
                    ClassesRemaining = 2,
                    MembershipId = 1
                };

                await _userRepository.AddCustomerAsync(customer);

                string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                string tokenLink = Url.Action("ConfirmEmail", "Account", new
                {
                    userid = user.Id,
                    token = myToken
                }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendEmail(model.Username, "FitSharp - Welcome to Your Fitness Journey",
                                        $"<h1 style=\"color:#B70D00;\">Welcome to FitSharp!</h1>" +
                                        $"<p>Thank you for choosing FitSharp, your gateway to a healthier and empowered lifestyle.</p>" +
                                        $"<p>We’re thrilled to have you as part of our global community. To complete your registration, please confirm your email address by clicking the link below:</p>" +
                                        $"<p><a href = \"{tokenLink}\" style=\"color:#FFA500; font-weight:bold;\">Confirm Email</a></p>" +
                                        $"<p>If you didn’t create this account, please disregard this email.</p>" +
                                        $"<br>" +
                                        $"<p>Your fitness journey awaits,</p>" +
                                        $"<p>The FitSharp Team</p>" +
                                        $"<p><small>This is an automated message. Please do not reply to this email.</small></p>");

                if (response.IsSuccess)
                {
                    ViewBag.Message = "The instructions to allow your account registration have been sent to your email.";
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "The user couldn't be logged in.");
            }

            return View(model);
        }

        // GET
        [Authorize]
        public async Task<IActionResult> ChangeUser()
        {
            if (!this.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var user = await _userRepository.GetUserByEmailAsync(this.User.Identity.Name);
            var model = new UserViewModel();
            if (user != null)
            {
                model.FirstName = user.FirstName;
                model.LastName = user.LastName;
                model.Address = user.Address;
                model.PhoneNumber = user.PhoneNumber;
                model.TaxNumber = user.TaxNumber;

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

        // POST
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangeUser(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepository.GetUserByEmailAsync(this.User.Identity.Name);
                if (user != null)
                {
                    var city = await _countryRepository.GetCityAsync(model.CityId);

                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.Address = model.Address;
                    user.PhoneNumber = model.PhoneNumber;
                    user.TaxNumber = model.TaxNumber;
                    user.CityId = model.CityId;
                    user.City = city;

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

            model.Countries = _countryRepository.GetComboCountries();
            if (model.CountryId != 0)
            {
                model.Cities = await _countryRepository.GetComboCitiesAsync(model.CountryId);
            }

            return View(model);
        }

        [Authorize]
        public IActionResult ChangePassword()
        {
            if (!this.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userRepository.GetUserByEmailAsync(this.User.Identity.Name);
                if (user != null)
                {
                    var result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        _flashMessage.Confirmation("Password changed successfully.");
                        return View(model);
                    }
                    else
                    {
                        this.ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault().Description);
                        _flashMessage.Danger(result.Errors.FirstOrDefault().Description);
                    }
                }
                else
                {
                    _flashMessage.Danger("User not found.");
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await _userRepository.GetUserByEmailAsync(model.Username);
                if (user != null)
                {
                    var result = await _userHelper.ValidatePasswordAsync(
                        user,
                        model.Password);

                    if (result.Succeeded)
                    {
                        var role = await _userHelper.GetRolesAsync(user);
                        var roleAsString = role.FirstOrDefault();

                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.UserName),
                            new Claim(ClaimTypes.Role, roleAsString)
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        var token = new JwtSecurityToken(
                            _configuration["Tokens:Issuer"],
                            _configuration["Tokens:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddDays(15),
                            signingCredentials: credentials);
                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        };

                        return this.Created(string.Empty, results);
                    }
                }
            }

            return BadRequest();
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return new NotFoundViewResult("UserNotFound");
            }

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new NotFoundViewResult("UserNotFound");
            }

            var result = await _userHelper.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return new NotFoundViewResult("UserNotFound");
            }

            user.IsActive = true;
            await _userRepository.UpdateUserAsync(user);

            return View();
        }

        public IActionResult RecoverPassword()
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = await _userRepository.GetUserByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "The email doesn't correspont to a registered user.");
                    return View(model);
                }

                var myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);

                var link = this.Url.Action(
                    "SetPassword",
                    "Account",
                    new
                    {
                        token = myToken,
                        userid = user.Id
                    }, protocol: HttpContext.Request.Scheme);

                Response response = _mailHelper.SendEmail(model.Email, "FitSharp - Password Reset",
                                        $"<h1 style=\"color:#B70D00;\">FitSharp Password Reset</h1>" +
                                        $"<p>We received a request to reset your password. If you made this request, please click the link below to reset your password:</p>" +
                                        $"<p><a href = \"{link}\" style=\"color:#FFA500; font-weight:bold;\">Reset Password</a></p>" +
                                        $"<p>If you did not request a password reset, please ignore this email. Your account is still secure.</p>" +
                                        $"<br>" +
                                        $"<p>Best regards,</p>" +
                                        $"<p>The FitSharp Team</p>" +
                                        $"<p><small>This is an automated message. Please do not reply to this email.</small></p>");

                if (response.IsSuccess)
                {
                    this.ViewBag.Message = "The instructions to recover your password have been sent to email.";
                }

                return this.View();
            }

            return this.View(model);
        }

        public async Task<IActionResult> SetPassword(string token, string userId)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                await _userHelper.LogoutAsync();
            }

            var model = new SetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
        {
            var user = await _userRepository.GetUserByIdAsync(model.UserId);

            if (user != null)
            {
                var result = await _userHelper.SetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    this.ViewBag.Message = "Password set successfully, you can now login.";
                    user.IsActive = true;
                    await _userRepository.UpdateUserAsync(user);
                    return View();
                }

                this.ViewBag.Message = "Error while resetting the password.";
                return View(model);
            }

            this.ViewBag.Message = "User not found.";

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> GenerateQRCode(string userName)
        {
            //var customer = await _userRepository.GetCustomerByUserName(User.Identity.Name);
            var customer = await _userRepository.GetCustomerByUserName(userName);

            if (customer == null)
            {
                return new NotFoundViewResult("UserNotFound");
            }

            var qrData = $"Name: {customer.User.FullName}\n" +
                         $"Membership ID: {customer.MembershipId}\n" +
                         $"Classes Remaining: {customer.ClassesRemaining}\n" +
                         $"Membership Start: {customer.MembershipBeginDate.ToShortDateString()}\n" +
                         $"Membership End: {customer.MembershipEndDate.ToShortDateString()}\n" +
                         $"Membership Active: {(customer.MembershipIsActive ? "Yes" : "No")}";

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);

                using (var qrCodeImage = qrCode.GetGraphic(20))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        qrCodeImage.Save(memoryStream, ImageFormat.Png);
                        return File(memoryStream.ToArray(), "image/png");
                    }
                }
            }
        }

        [Authorize]
        public async Task<IActionResult> DisplayQRCode(string userName)
        {
            var customer = await _userRepository.GetCustomerByUserName(userName);

            if (customer == null)
            {
                return new NotFoundViewResult("UserNotFound");
            }

            return View(customer);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DownloadQRCodePdf(string userName)
        {
            var customer = await _userRepository.GetCustomerByUserName(userName);

            if (customer == null)
            {
                return new NotFoundViewResult("UserNotFound");
            }

            var qrData = $"Name: {customer.User.FullName}\n" +
                         $"Membership ID: {customer.MembershipId}\n" +
                         $"Classes Remaining: {customer.ClassesRemaining}\n" +
                         $"Membership Start: {customer.MembershipBeginDate.ToShortDateString()}\n" +
                         $"Membership End: {customer.MembershipEndDate.ToShortDateString()}\n" +
                         $"Membership Active: {(customer.MembershipIsActive ? "Yes" : "No")}";

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new QRCode(qrCodeData);

                using (var qrCodeImage = qrCode.GetGraphic(20))
                {
                    using (var document = new Syncfusion.Pdf.PdfDocument())
                    {
                        var page = document.Pages.Add();

                        using (var memoryStream = new MemoryStream())
                        {
                            qrCodeImage.Save(memoryStream, ImageFormat.Png);
                            memoryStream.Position = 0;

                            var image = new Syncfusion.Pdf.Graphics.PdfBitmap(memoryStream);
                            var graphics = page.Graphics;

                            var maxWidth = page.Graphics.ClientSize.Width * 0.5;
                            var maxHeight = page.Graphics.ClientSize.Height * 0.5;

                            var aspectRatio = image.Width / (float)image.Height;
                            var width = Math.Min(maxWidth, image.Width);
                            var height = width / aspectRatio;

                            var x = (float)(page.Graphics.ClientSize.Width - width) / 2;
                            var y = (float)(page.Graphics.ClientSize.Height - height) / 2;

                            graphics.DrawImage(image, new Syncfusion.Drawing.RectangleF(x, y, (float)width, (float)height));
                        }

                        page.Graphics.DrawString(
                            $"QR Code for {customer.User.FullName}",
                            new Syncfusion.Pdf.Graphics.PdfStandardFont(Syncfusion.Pdf.Graphics.PdfFontFamily.Helvetica, 24),
                            Syncfusion.Pdf.Graphics.PdfBrushes.Black,
                            new Syncfusion.Drawing.PointF(50, 50)
                        );

                        using (var pdfStream = new MemoryStream())
                        {
                            document.Save(pdfStream);
                            pdfStream.Position = 0;

                            return File(pdfStream.ToArray(), "application/pdf", "QRCode.pdf");
                        }
                    }
                }
            }
        }

        public IActionResult NotAuthorized()
        {
            return View();
        }

        [HttpPost]
        [Route("Account/GetCitiesAsync")]
        public async Task<JsonResult> GetCitiesAsync(int countryId)
        {
            var country = await _countryRepository.GetCountryWithCitiesAsync(countryId);
            return Json(country.Cities.OrderBy(c => c.Name));
        }
    }
}