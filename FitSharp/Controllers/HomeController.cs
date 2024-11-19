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
        private readonly IPersonalClassRepository _personalClassRepository;
        private readonly IGroupClassRepository _groupClassRepository;

        public HomeController(
            ILogger<HomeController> logger,
            IGymRepository gymsRepository,
            IUserRepository userRepository,
            IPersonalClassRepository personalClassRepository,
            IGroupClassRepository groupClassRepository)
        {
            _logger = logger;
            _gymsRepository = gymsRepository;
            _userRepository = userRepository;
            _personalClassRepository = personalClassRepository;
            _groupClassRepository = groupClassRepository;
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
            ViewBag.PersonalClasses = _personalClassRepository.GetAllPersonalClassesWithRelatedData();
            ViewBag.GroupClasses = _groupClassRepository.GetAllGroupClassesWithRelatedData();
            return View();
        }

        public async Task<IActionResult> InstructorDetails(int id)
        {
            // Busca o instrutor com todos os dados relacionados
            var instructor = _userRepository.GetInstructorWithAllRelatedDataByInstructorId(id);

            if (instructor == null)
            {
                return View("InstructorNotFound");
            }

            return View(instructor);
        }
    }
}