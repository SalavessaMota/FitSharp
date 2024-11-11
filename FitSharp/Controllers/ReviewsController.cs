using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Vereyon.Web;

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

            return RedirectToAction("CustomerPastPersonalClasses", "PersonalClasses");
        }


        public async Task<IActionResult> Edit(int? id)
        {
           if (id == null)
            {
                return NotFound();
            }

            var review = await _reviewRepository.GetByIdAsync(id.Value);
            if (review == null)
            {
                return NotFound();
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
                return RedirectToAction("CustomerPastPersonalClasses", "PersonalClasses");
            }

            return View(review);
        }




    }
}
