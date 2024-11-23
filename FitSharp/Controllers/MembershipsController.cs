using Braintree;
using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Vereyon.Web;

namespace FitSharp.Controllers
{
    public class MembershipsController : Controller
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFlashMessage _flashMessage;
        private readonly IPaymentHelper _paymentHelper;
        private readonly IMailHelper _mailHelper;

        public MembershipsController(
            IMembershipRepository membershipRepository,
            IUserRepository userRepository,
            IFlashMessage flashMessage,
            IPaymentHelper paymentHelper,
            IMailHelper mailHelper)
        {
            _membershipRepository = membershipRepository;
            _userRepository = userRepository;
            _flashMessage = flashMessage;
            _paymentHelper = paymentHelper;
            _mailHelper = mailHelper;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            return View(_membershipRepository.GetAll().OrderBy(m => m.Price));
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Membership membership)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _membershipRepository.CreateAsync(membership);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    _flashMessage.Danger("This membership already exists");
                }

                return View(membership);
            }

            return View(membership);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("MembershipNotFound");
            }

            var membership = await _membershipRepository.GetByIdAsync(id.Value);
            if (membership == null)
            {
                return new NotFoundViewResult("MembershipNotFound");
            }
            return View(membership);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Membership membership)
        {
            if (ModelState.IsValid)
            {
                await _membershipRepository.UpdateAsync(membership);
                return RedirectToAction(nameof(Index));
            }

            return View(membership);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("MembershipNotFound");
            }

            var membership = await _membershipRepository.GetByIdAsync(id.Value);
            if (membership == null)
            {
                return new NotFoundViewResult("MembershipNotFound");
            }

            try
            {
                await _membershipRepository.DeleteAsync(membership);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
                {
                    ViewBag.ErrorTitle = $"The membership {membership.Name} is probably being used!!";
                    ViewBag.ErrorMessage = $"{membership.Name} can't be deleted because it is being used.</br></br>";
                }

                return View("Error");
            }
        }

        public async Task<IActionResult> AvailableMemberships()
        {
            var memberships = await _membershipRepository.GetAll().ToListAsync();
            return View(memberships);
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PurchaseMembership(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("MembershipNotFound");
            }

            var membership = await _membershipRepository.GetByIdAsync(id.Value);
            if (membership == null)
            {
                return RedirectToAction("MembershipNotFound");
            }

            var gateway = _paymentHelper.GetGateway();
            var clientToken = gateway.ClientToken.Generate();
            ViewBag.ClientToken = clientToken;
            return View(membership);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> PurchaseMembership(Membership model)
        {
            var membership = await _membershipRepository.GetByIdAsync(model.Id);
            var gateway = _paymentHelper.GetGateway();
            var request = new TransactionRequest
            {
                Amount = Convert.ToDecimal(model.Price),
                PaymentMethodNonce = model.Nonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            Result<Transaction> result = gateway.Transaction.Sale(request);
            if (!result.IsSuccess())
            {
                _flashMessage.Danger("Could not accept payment.");
                return RedirectToAction("AvailableMemberships");
            }

            var customer = await _userRepository.GetCustomerByUserName(User.Identity.Name);
            if (customer == null)
            {
                RedirectToAction("UserNotFound", "Admin");
            }

            customer.MembershipId = membership.Id;
            customer.MembershipBeginDate = DateTime.Now;
            customer.MembershipEndDate = DateTime.Now.AddMonths(1);
            customer.ClassesRemaining = membership.NumberOfClasses;
            customer.MembershipIsActive = true;
            await _userRepository.UpdateCustomerAsync(customer);

            Response response = _mailHelper.SendEmail(customer.User.Email, "FitSharp - Membership Receipt",
                                    $"<h1 style=\"color:#B70D00;\">Thank you for your purchase!</h1>" +
                                    $"<p>Here are the details of your newly acquired membership.</p><hr />" +
                                    $"<h2 style=\"color:#B70D00;\">Membership:</h2>" +
                                    $"<h3 style=\"color:#B70D00;\">Name:</h3>" +
                                    $"<p>{membership.Name}</p>" +
                                    $"<h3 style=\"color:#B70D00;\">Number of available classes:</h3>" +
                                    $"<p>{(membership.NumberOfClasses > 9999 ? "Unlimited" : membership.NumberOfClasses.ToString())}</p>" +
                                    $"<h3 style=\"color:#B70D00;\">Description:</h3>" +
                                    $"<p>{membership.Description}</p>" +
                                    $"<h3 style=\"color:#B70D00;\">Price:</h3>" +
                                    $"<p>{model.Price}€</p><br />" +
                                    $"<h2 style=\"color:#B70D00;\">Client:</h2>" +
                                    $"<h3 style=\"color:#B70D00;\">Name:</h3>" +
                                    $"<p>{customer.User.FullName}</p>" +
                                    $"<h3 style=\"color:#B70D00;\">Address:</h3>" +
                                    $"<p>{customer.User.Address}</p>" +
                                    $"<h3 style=\"color:#B70D00;\">Tax Number:</h3>" +
                                    $"<p>{customer.User.TaxNumber}</p><br />" +
                                    $"<p>If you didn’t request this purchase, please contact us at our website.</p>" +
                                    $"<br>" +
                                    $"<p>Your fitness journey awaits,</p>" +
                                    $"<p>The FitSharp Team</p>" +
                                    $"<p><small>This is an automated message. Please do not reply to this email.</small></p>");

            if (!response.IsSuccess)
            {
                _flashMessage.Danger("Message could not be sent. Please try again later.");
                return View(model);
            }

            _flashMessage.Confirmation("Payment accepted successfully! You will receive an email with the receipt shortly.");
            return RedirectToAction("AvailableMemberships");
        }
    }
}