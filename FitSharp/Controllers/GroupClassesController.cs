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
using System.Security.Policy;
using System.Threading.Tasks;
using Vereyon.Web;

namespace FitSharp.Controllers
{
    public class GroupClassesController : Controller
    {
        private readonly IGroupClassRepository _groupClassRepository;
        private readonly IGymRepository _gymRepository;
        private readonly IUserRepository _userRepository;
        private readonly IClassTypeRepository _classTypeRepository;
        private readonly IFlashMessage _flashMessage;

        public GroupClassesController(
            IGroupClassRepository groupClassRepository,
            IGymRepository gymRepository,
            IUserRepository userRepository,
            IClassTypeRepository classTypeRepository,
            IFlashMessage flashMessage)
        {
            _groupClassRepository = groupClassRepository;
            _gymRepository = gymRepository;
            _userRepository = userRepository;
            _classTypeRepository = classTypeRepository;
            _flashMessage = flashMessage;
        }

        public async Task<IActionResult> Index(string filter)
        {
            var classes = await _groupClassRepository.GetGroupClassesAsyncWithAllRelatedDataAsync();

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

            var model = new CreateGroupClassViewModel
            {
                Rooms = _gymRepository.GetComboRoomsByInstructorName(User.Identity.Name),
                ClassTypes = _classTypeRepository.GetComboClassTypes(),
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGroupClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var instructor = _userRepository.GetInstructorByUserName(User.Identity.Name);


                var groupClass = new GroupClass
                {
                    Name = model.Name,
                    RoomId = model.RoomId,
                    Room = model.Room,
                    ClassTypeId = model.ClassTypeId,
                    ClassType = model.ClassType,
                    InstructorId = instructor.Id,
                    Instructor = instructor,
                    StartTime = model.StartTime,
                    EndTime = model.EndTime,
                    Informations = model.Informations
                };

                await _groupClassRepository.CreateAsync(groupClass);
                return RedirectToAction(nameof(Index));
            }

            model.Rooms = _gymRepository.GetComboRoomsByInstructorName(User.Identity.Name);
            model.ClassTypes = _classTypeRepository.GetComboClassTypes();

            return View(model);
        }

        public IActionResult Details(int id)
        {
            var groupClass = _groupClassRepository.GetGroupClassWithAllRelatedDataAsync(id).Result;

            if(groupClass == null)
            {
                return new NotFoundViewResult("GroupClassNotFound");
            }

            return View(groupClass);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var groupClass = await _groupClassRepository.GetGroupClassWithAllRelatedDataAsync(id);

            if (groupClass == null)
            {
                return new NotFoundViewResult("GroupClassNotFound");
            }


            var model = new EditGroupClassViewModel
            {
                Id = groupClass.Id,
                Name = groupClass.Name,
                InstructorId = groupClass.InstructorId,       
                Instructors = _userRepository.GetComboInstructors(),
                RoomId = groupClass.RoomId,
                Rooms = _gymRepository.GetComboRoomsByInstructorName(User.Identity.Name),
                ClassTypeId = groupClass.ClassTypeId,
                ClassTypes = _classTypeRepository.GetComboClassTypes(),
                StartTime = groupClass.StartTime,
                EndTime = groupClass.EndTime,
                Informations = groupClass.Informations,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditGroupClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var groupClass = await _groupClassRepository.GetByIdAsync(model.Id);

                if (groupClass == null)
                {
                    return new NotFoundViewResult("GroupClassNotFound");
                }

                groupClass.Name = model.Name;
                groupClass.InstructorId = model.InstructorId;
                groupClass.RoomId = model.RoomId;
                groupClass.ClassTypeId = model.ClassTypeId;
                groupClass.StartTime = model.StartTime;
                groupClass.EndTime = model.EndTime;
                groupClass.Informations = model.Informations;

                await _groupClassRepository.UpdateAsync(groupClass);
                return RedirectToAction(nameof(Index));
            }

            model.Instructors = _userRepository.GetComboInstructors();
            model.Rooms = _gymRepository.GetComboRoomsByInstructorName(User.Identity.Name);
            model.ClassTypes = _classTypeRepository.GetComboClassTypes();

            return View(model);
        }





        public async Task<IActionResult> Delete (int? id)
        {
            if(id == null)
            {
                return new NotFoundViewResult("GroupClassNotFound");
            }

            var groupClass = await _groupClassRepository.GetByIdAsync(id.Value);

            if(groupClass == null)
            {
                return new NotFoundViewResult("GroupClassNotFound");
            }

            if(groupClass.EndTime < DateTime.Now)
            {
                _flashMessage.Danger("You can't delete a class that has already happened.");
            }

            try
            {
                await _groupClassRepository.DeleteAsync(groupClass);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
                {
                    ViewBag.ErrorTitle = $"The group class {groupClass.Name} is probably being used!!";
                    ViewBag.ErrorMessage = $"{groupClass.Name}  can't be deleted because it is being used.</br></br>";
                }
            }



            return View("Error");
        }

    }
}
