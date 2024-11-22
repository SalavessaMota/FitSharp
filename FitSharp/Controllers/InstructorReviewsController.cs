using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace FitSharp.Controllers
{
    public class InstructorReviewsController : Controller
    {
        private readonly IInstructorReviewRepository _instructorReviewRepository;
        private readonly IUserRepository _userRepository;

        public InstructorReviewsController(
            IInstructorReviewRepository instructorReviewRepository,
            IUserRepository userRepository)
        {
            _instructorReviewRepository = instructorReviewRepository;
            _userRepository = userRepository;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var reviews = await _instructorReviewRepository.GetReviewsWithAllRelatedDataAsync();
            return View(reviews);
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> WriteReview(int instructorId)
        {
            var customer = await _userRepository.GetCustomerByUserName(this.User.Identity.Name);
            if (customer == null)
            {
                return NotFound();
            }

            var instructor = await _userRepository.GetInstructorWithAllRelatedDataByInstructorIdAsync(instructorId);
            if (instructor == null)
            {
                return NotFound();
            }

            var review = new InstructorReview
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
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> WriteReview(InstructorReview review)
        {
            if (!ModelState.IsValid)
            {
                return View(review);
            }

            await _instructorReviewRepository.AddReviewAsync(review);
            return RedirectToAction("InstructorDetails", "Home", new { Id = review.InstructorId });
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ReviewNotFound");
            }

            var review = await _instructorReviewRepository.GetByIdAsync(id.Value);
            if (review == null)
            {
                return new NotFoundViewResult("ReviewNotFound");
            }

            return View(review);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(InstructorReview review)
        {
            if (ModelState.IsValid)
            {
                await _instructorReviewRepository.UpdateAsync(review);

                // Obtém o username do utilizador relacionado com a review, caso não esteja no objeto `review`
                var username = review.Customer?.User.UserName ?? User.Identity.Name;

                return RedirectToAction("InstructorDetails", "Home", new { Id = review.InstructorId });
            }

            return View(review);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("ReviewNotFound");
            }

            var review = await _instructorReviewRepository.GetReviewWithAllRelatedDataByIdAsync(id.Value);
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
                return new NotFoundViewResult("ReviewNotFound");
            }

            var reviews = await _instructorReviewRepository.GetAllReviewsWithRelatedDataByInstructorId(id.Value);
            return View(reviews);
        }
    }
}