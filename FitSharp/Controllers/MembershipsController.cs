using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Vereyon.Web;

namespace FitSharp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MembershipsController : Controller
    {
        private readonly IMembershipRepository _membershipRepository;
        private readonly IFlashMessage _flashMessage;

        public MembershipsController(
            IMembershipRepository membershipRepository,
            IFlashMessage flashMessage)
        {
            _membershipRepository = membershipRepository;
            _flashMessage = flashMessage;
        }

        public IActionResult Index()
        {
            return View(_membershipRepository.GetAll());
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
                    ViewBag.ErrorMessage = $"{membership.Name}  can't be deleted because it is being used.</br></br>";
                }

                return View("Error");
            }
        }
    }
}