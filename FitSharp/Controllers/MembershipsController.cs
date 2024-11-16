using Braintree;
using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Helpers;
using Microsoft.AspNetCore.Mvc;
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

        public MembershipsController(
            IMembershipRepository membershipRepository,
            IUserRepository userRepository,
            IFlashMessage flashMessage,
            IPaymentHelper paymentHelper)
        {
            _membershipRepository = membershipRepository;
            _userRepository = userRepository;
            _flashMessage = flashMessage;
            _paymentHelper = paymentHelper;
        }

        public IActionResult Index()
        {
            return View(_membershipRepository.GetAll().OrderBy(m => m.Price));
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> Edit(Membership membership)
        {
            if (ModelState.IsValid)
            {
                await _membershipRepository.UpdateAsync(membership);
                return RedirectToAction(nameof(Index));
            }

            return View(membership);
        }

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

            _flashMessage.Confirmation("Payment accepted successfully!");
            return RedirectToAction("AvailableMemberships");
        }
    }
}