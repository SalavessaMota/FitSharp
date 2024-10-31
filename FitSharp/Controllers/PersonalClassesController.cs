using FitSharp.Data;
using FitSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FitSharp.Controllers
{
    public class PersonalClassesController : Controller
    {
        private readonly IPersonalClassesRepository _personalClassesRepository;
        private readonly IGymRepository _gymRepository;
        private readonly IUserRepository _userRepository;
        private readonly IClassTypeRepository _classTypeRepository;

        public PersonalClassesController(
            IPersonalClassesRepository personalClassesRepository,
            IGymRepository gymRepository,
            IUserRepository userRepository,
            IClassTypeRepository classTypeRepository)
        {
            _personalClassesRepository = personalClassesRepository;
            _gymRepository = gymRepository;
            _userRepository = userRepository;
            _classTypeRepository = classTypeRepository;
        }


        public IActionResult Index()
        {
            var instructorName = User.Identity.Name;

            return View(_personalClassesRepository.GetAllPersonalClassesWithRelatedDataByInstructorName(instructorName));
        }


        public IActionResult Create()
        {
            return View(_userRepository.GetAllCustomersWithUser());
        }

        public IActionResult CreatePersonalClass()
        {
            var model = new CreatePersonalClassViewModel();

            model.Rooms = _gymRepository.GetComboRoomsByInstructorName(this.User.Identity.Name);

            model.ClassTypes =  _classTypeRepository.GetComboClassTypes();

            return View(model);
        }


    }
}
