using FitSharp.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FitSharp.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly IReviewRepository _reviewRepository;

        public ReviewsController(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<IActionResult> Index()
        {
            var reviews = await _reviewRepository.GetReviewsWithAllRelatedDataAsync();
            return View(reviews);
        }
    }
}
