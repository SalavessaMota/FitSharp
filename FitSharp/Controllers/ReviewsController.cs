using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FitSharp.Controllers
{    
    public class ReviewsController : Controller
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IUserRepository _userRepository;

        public ReviewsController(
            IReviewRepository reviewRepository,
            IUserRepository userRepository)
        {
            _reviewRepository = reviewRepository;
            _userRepository = userRepository;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewRepository.GetReviewsWithAllRelatedDataAsync();
            return View(reviews);
        }


        public async Task<IActionResult> WriteReview(int instructorId)
        {
            var customer = await _userRepository.GetCustomerByUserName(this.User.Identity.Name);
            if (customer == null)
            {
                return NotFound();
            }

            var instructor = _userRepository.GetInstructorWithAllRelatedDataByInstructorId(instructorId);
            if (instructor == null)
            {
                return NotFound();
            }

            var review = new Review
            {
                InstructorId = instructorId,
                Instructor = instructor,
                CustomerId = customer.Id,
                Customer = customer,
                ReviewDate = DateTime.Now
            };

            return View(review);
        }

        [HttpPost]
        public async Task<IActionResult> WriteReview(Review review)
        {
            if (!ModelState.IsValid)
            {
                return View(review);
            }

            await _reviewRepository.AddReviewAsync(review);


            return RedirectToAction("CustomerPastPersonalClasses", "PersonalClasses", new { username = User.Identity.Name });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ReviewNotFound");
            }

            var review = await _reviewRepository.GetByIdAsync(id.Value);
            if (review == null)
            {
                return new NotFoundViewResult("ReviewNotFound");
            }

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Review review)
        {
            if (ModelState.IsValid)
            {
                await _reviewRepository.UpdateAsync(review);

                // Obtém o username do utilizador relacionado com a review, caso não esteja no objeto `review`
                var username = review.Customer?.User.UserName ?? User.Identity.Name;

                return RedirectToAction("CustomerPastPersonalClasses", "PersonalClasses", new { username = username });
            }

            return View(review);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return new NotFoundViewResult("ReviewNotFound");
            }

            var review = await _reviewRepository.GetReviewWithAllRelatedDataByIdAsync(id.Value);
            if (review == null)
            {
                return new NotFoundViewResult("ReviewNotFound");
            }

            return View(review);
        }

        public async Task<IActionResult> InstructorReviews(int? id)
        
        {
            if (id == null)
            {
                //TODO: Create reviews not found view
                return new NotFoundViewResult("ReviewNotFound");
            }

            var reviews = await _reviewRepository.GetAllReviewsWithRelatedDataByInstructorId(id.Value);

            return View(reviews);
        }
    }
}
