using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public async Task<IActionResult> WriteReview(GymReview review)
        {
            if (!ModelState.IsValid)
            {
                return View(review);
            }

            await _gymReviewRepository.AddReviewAsync(review);
            return RedirectToAction("CustomerPersonalClasses", "PersonalClasses", new { username = User.Identity.Name });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ReviewNotFound");
            }

            var review = await _gymReviewRepository.GetByIdAsync(id.Value);
            if (review == null)
            {
                return new NotFoundViewResult("ReviewNotFound");
            }

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(GymReview review)
        {
            if (ModelState.IsValid)
            {
                await _gymReviewRepository.UpdateAsync(review);

                // Obtém o username do utilizador relacionado com a review, caso não esteja no objeto `review`
                var username = review.Customer?.User.UserName ?? User.Identity.Name;

                return RedirectToAction("CustomerPersonalClasses", "PersonalClasses", new { username = username });
            }

            return View(review);
        }

        public async Task<IActionResult> Details(int? id)
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

            return View(review);
        }

        public async Task<IActionResult> GymReviews(int? id)
        {
            if (id == null)
            {
                //TODO: Create reviews not found view
                return new NotFoundViewResult("ReviewNotFound");
            }

            var reviews = await _gymReviewRepository.GetAllReviewsWithRelatedDataByGymId(id.Value);
            return View(reviews);
        }
    }
}