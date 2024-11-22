using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FitSharp.Controllers
{
    public class GymReviewsController : Controller
    {
        private readonly IGymReviewRepository _gymReviewRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGymRepository _gymRepository;

        public GymReviewsController(
            IGymReviewRepository gymReviewRepository,
            IUserRepository userRepository,
            IGymRepository gymRepository)
        {
            _gymReviewRepository = gymReviewRepository;
            _userRepository = userRepository;
            _gymRepository = gymRepository;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var reviews = await _gymReviewRepository.GetReviewsWithAllRelatedDataAsync();
            return View(reviews);
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> WriteReview(int gymId)
        {
            var customer = await _userRepository.GetCustomerByUserName(this.User.Identity.Name);
            if (customer == null)
            {
                return NotFound();
            }

            var gym = await _gymRepository.GetGymWithAllRelatedDataAsync(gymId);
            if (gym == null)
            {
                return NotFound();
            }

            var review = new GymReview
            {
                GymId = gymId,
                Gym = gym,
                CustomerId = customer.Id,
                Customer = customer,
                ReviewDate = DateTime.Now
            };

            return View(review);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> WriteReview(GymReview review)
        {
            if (ContainsOffensiveWords(review.Description))
            {
                ModelState.AddModelError("Description", "Your review contains inappropriate language.");
            }

            if (!ModelState.IsValid)
            {
                return View(review);
            }

            await _gymReviewRepository.AddReviewAsync(review);
            return RedirectToAction("GymDetails", "Home", new { Id = review.GymId });
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ReviewNotFound");
            }

            var review = await _gymReviewRepository.GetReviewWithAllRelatedDataByIdAsync(id.Value);
            if (review == null)
            {
                return new NotFoundViewResult("ReviewNotFound");
            }

            if (review.Customer.User.UserName != User.Identity.Name)
            {
                return RedirectToAction("NotAuthorized", "Account");
            }

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Edit(GymReview review)
        {
            if (ContainsOffensiveWords(review.Description))
            {
                ModelState.AddModelError("Description", "Your review contains inappropriate language.");
            }

            if (ModelState.IsValid)
            {
                await _gymReviewRepository.UpdateAsync(review);

                // Obtém o username do utilizador relacionado com a review, caso não esteja no objeto `review`
                var username = review.Customer?.User.UserName ?? User.Identity.Name;

                return RedirectToAction("GymDetails", "Home", new { Id = review.GymId });
            }

            return View(review);
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("GymReviewNotFound");
            }

            var review = await _gymReviewRepository.GetReviewWithAllRelatedDataByIdAsync(id.Value);
            if (review == null)
            {
                return new NotFoundViewResult("GymReviewNotFound");
            }

            return View(review);
        }

        public async Task<IActionResult> GymReviews(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("GymReviewNotFound");
            }

            var reviews = await _gymReviewRepository.GetAllReviewsWithRelatedDataByGymId(id.Value);
            return View(reviews);
        }

        private bool ContainsOffensiveWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var offensiveWords = new List<string>
            {
                "Dumb",
                "Stupid"
            };

            foreach (var word in offensiveWords)
            {
                if (text.IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}