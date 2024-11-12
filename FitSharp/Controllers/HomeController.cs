using FitSharp.Data;
using FitSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace FitSharp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUserRepository _userRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }

        public IActionResult Index()
        {
            var instructors = _userRepository.GetAllInstructorsWithAllRelatedData();
            if (instructors == null)
            {
                return NotFound();
            }

            return View(instructors);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult OurInstructors()
        {
            var instructors = _userRepository.GetAllInstructorsWithAllRelatedData();
            if(instructors == null)
            {
                return NotFound();
            }

            return View(instructors);
        }
    }
}