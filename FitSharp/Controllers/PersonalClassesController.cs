using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Helpers;
using FitSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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


        public IActionResult Index(string filter)
        {
            var name = User.Identity.Name;

            var classes = _personalClassesRepository.GetAllPersonalClassesWithRelatedDataByUserName(name);

            switch (filter)
            {
                case "past":
                    classes = classes.Where(c => c.EndTime < DateTime.Now);
                    break;
                case "future":
                    classes = classes.Where(c => c.EndTime > DateTime.Now);
                    break;
                default:
                    filter = "all";
                    break;
            }

            ViewBag.CurrentFilter = new List<SelectListItem>
            {
                new SelectListItem { Value = "all", Text = "All Classes", Selected = (filter == "all") },
                new SelectListItem { Value = "past", Text = "Past Classes", Selected = (filter == "past") },
                new SelectListItem { Value = "future", Text = "Future Classes", Selected = (filter == "future") }
            };

            return View(classes);
        }





        public IActionResult Create()
        {
            return View(_userRepository.GetAllCustomersWithUser());
        }

        public IActionResult CreatePersonalClass(int customerId)
        {
            var model = new CreatePersonalClassViewModel
            {
                CustomerId = customerId,
                Rooms = _gymRepository.GetComboRoomsByInstructorName(this.User.Identity.Name),
                ClassTypes = _classTypeRepository.GetComboClassTypes(),
                StartTime = System.DateTime.Now,
                EndTime = System.DateTime.Now.AddHours(1)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePersonalClass(CreatePersonalClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var instructor = _userRepository.GetInstructorByUserName(this.User.Identity.Name);

                

                var personalClass = new PersonalClass
                {
                    Name = model.Name,
                    InstructorId = instructor.Id,
                    Instructor = instructor,
                    CustomerId = model.CustomerId,
                    Customer = await _userRepository.GetCustomerByIdAsync(model.CustomerId),
                    ClassTypeId = model.ClassTypeId,
                    RoomId = model.RoomId,
                    Room = await _gymRepository.GetRoomAsync(model.RoomId),
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Informations = model.Informations
                };

                await _personalClassesRepository.CreateAsync(personalClass);
                return RedirectToAction(nameof(Index));
            }

            model.Rooms = _gymRepository.GetComboRoomsByInstructorName(this.User.Identity.Name);
            model.ClassTypes = _classTypeRepository.GetComboClassTypes();

            return View(model);
        }

        public IActionResult Details(int id)
        {
            var personalClass = _personalClassesRepository.GetAllPersonalClassesWithRelatedData()
                .FirstOrDefault(p => p.Id == id);

            if (personalClass == null)
            {
                return NotFound();
            }

            return View(personalClass);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var personalClass = await _personalClassesRepository.GetPersonalClassWithAllRelatedData(id);

            if (personalClass == null)
            {
                return NotFound();
            }

            var gym = await _gymRepository.GetGymAsync(personalClass.Room);

            var model = new EditPersonalClassViewModel
            {
                Id = personalClass.Id,
                Name = personalClass.Name,
                CustomerId = personalClass.CustomerId,
                InstructorId = personalClass.InstructorId,
                RoomId = personalClass.RoomId,
                Rooms = await _gymRepository.GetComboRoomsAsync(gym.Id),
                ClassTypeId = personalClass.ClassTypeId,
                ClassTypes = _classTypeRepository.GetComboClassTypes(),
                StartTime = personalClass.StartTime,
                EndTime = personalClass.EndTime,
                Informations = personalClass.Informations
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPersonalClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var personalClass = await _personalClassesRepository.GetByIdAsync(model.Id);

                if (personalClass == null)
                {
                    return NotFound();
                }

                personalClass.Name = model.Name;
                personalClass.CustomerId = model.CustomerId;
                personalClass.InstructorId = model.InstructorId;
                personalClass.RoomId = model.RoomId;
                personalClass.ClassTypeId = model.ClassTypeId;
                personalClass.StartTime = model.StartTime;
                personalClass.EndTime = model.EndTime;
                personalClass.Informations = model.Informations;

                await _personalClassesRepository.UpdateAsync(personalClass);
                return RedirectToAction(nameof(Index));
            }

            model.Rooms = await _gymRepository.GetComboRoomsAsync(model.RoomId);
            model.ClassTypes = _classTypeRepository.GetComboClassTypes();

            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("PersonalClassNotFound");
            }

            var personalClass= await _personalClassesRepository.GetByIdAsync(id.Value);
            if (personalClass == null)
            {
                return new NotFoundViewResult("PersonalClassNotFoundNotFound");
            }

            try
            {
                await _personalClassesRepository.DeleteAsync(personalClass);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
                {
                    ViewBag.ErrorTitle = $"The personal class {personalClass.Name} is probably being used!!";
                    ViewBag.ErrorMessage = $"{personalClass.Name}  can't be deleted because it is being used.</br></br>";
                }

                return View("Error");
            }
        }
    }
}
