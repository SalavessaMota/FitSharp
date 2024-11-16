using FitSharp.Data;
using FitSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace FitSharp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGymRepository _gymsRepository;
        private readonly IUserRepository _userRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IGymRepository gymsRepository,
            IUserRepository userRepository)
        {
            _logger = logger;
            _gymsRepository = gymsRepository;
            _userRepository = userRepository;
        }

        public async Task<IActionResult> Index()
        {
            var gyms = await _gymsRepository.GetGymsWithAllRelatedDataAsync();
            var instructors = _userRepository.GetAllInstructorsWithAllRelatedData();

            if (gyms == null || instructors == null)
            {
                return NotFound();
            }

            if (instructors == null)
            {
                return NotFound();
            }

            var model = new HomeIndexViewModel
            {
                Gyms = gyms,
                Instructors = instructors
            };

            return View(model);
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
            if (instructors == null)
            {
                return NotFound();
            }

            return View(instructors);
        }

        public async Task<IActionResult> OurGyms()
        {
            var gyms = await _gymsRepository.GetGymsWithAllRelatedDataAsync();
            if (gyms == null)
            {
                return NotFound();
            }

            return View(gyms);
        }

        public IActionResult CustomersInformations()
        {
            var customers = _userRepository.GetAllCustomersWithAllRelatedData();

            return View(customers);
        }

        public IActionResult PersonalClassesInfo()
        {
            return View();
        }

        public IActionResult GroupClassesInfo()
        {
            return View();
        }

        public IActionResult CombinedClassesCalendar()
        {
            return View();
        }
    }
}