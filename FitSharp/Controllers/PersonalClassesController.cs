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
using Vereyon.Web;

namespace FitSharp.Controllers
{
    public class PersonalClassesController : Controller
    {
        private readonly IPersonalClassRepository _personalClassesRepository;
        private readonly IGymRepository _gymRepository;
        private readonly IUserRepository _userRepository;
        private readonly IClassTypeRepository _classTypeRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IFlashMessage _flashMessage;

        public PersonalClassesController(
            IPersonalClassRepository personalClassesRepository,
            IGymRepository gymRepository,
            IUserRepository userRepository,
            IClassTypeRepository classTypeRepository,
            INotificationRepository notificationRepository,
            IFlashMessage flashMessage)
        {
            _personalClassesRepository = personalClassesRepository;
            _gymRepository = gymRepository;
            _userRepository = userRepository;
            _classTypeRepository = classTypeRepository;
            _notificationRepository = notificationRepository;
            _flashMessage = flashMessage;
        }

        public IActionResult Index(string filter)
        {
            var name = User.Identity.Name;

            var classes = _personalClassesRepository.GetAllPersonalClassesWithRelatedData();

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

        public IActionResult CreateOpenPersonalClass()
        {
            return View();
        }

        public IActionResult Create()
        {
            var clients = _userRepository.GetAllCustomersWithAllRelatedData();
            var clientsWithMembershipActive = clients.Where(c => c.ClassesRemaining > 0 && c.Membership != null);

            return View(clientsWithMembershipActive);
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
                    ClassTypeId = model.ClassTypeId,
                    ClassType = model.ClassType,
                    RoomId = model.RoomId,
                    Room = await _gymRepository.GetRoomAsync(model.RoomId),
                    StartTime = model.StartTime,
                    EndTime = model.StartTime.AddHours(1),
                    Informations = model.Informations
                };

                await _personalClassesRepository.CreateAsync(personalClass);

                if (model.CustomerId != 0)
                {
                    var customer = await _userRepository.GetCustomerByIdAsync(model.CustomerId);

                    if (customer != null)
                    {
                        personalClass.CustomerId = model.CustomerId;
                        personalClass.Customer = customer;
                        await _personalClassesRepository.UpdateAsync(personalClass);

                        var actionUrl = Url.Action("Details", "PersonalClasses", new { id = personalClass.Id }, protocol: HttpContext.Request.Scheme);
                        var notification = new Notification
                        {
                            Title = "An instructor has scheduled a new personal class and is awaiting confirmation.",
                            Action = $"<a href=\"{actionUrl}\" class=\"btn btn-primary\">Go to personal class.</a>",
                            User = customer.User,
                            UserId = customer.User.Id,
                        };

                        await _notificationRepository.CreateAsync(notification);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "The specified customer does not exist.");
                        model.Rooms = _gymRepository.GetComboRoomsByInstructorName(this.User.Identity.Name);
                        model.ClassTypes = _classTypeRepository.GetComboClassTypes();
                        return View(model);
                    }

                    customer.ClassesRemaining--;
                    await _userRepository.UpdateCustomerAsync(customer);
                }

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
                return new NotFoundViewResult("PersonalClassNotFound");
            }

            return View(personalClass);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var personalClass = await _personalClassesRepository.GetPersonalClassWithAllRelatedData(id);

            if (personalClass == null)
            {
                return new NotFoundViewResult("PersonalClassNotFound");
            }

            var gym = await _gymRepository.GetGymByRoomAsync(personalClass.Room);

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
                    return new NotFoundViewResult("PersonalClassNotFound");
                }

                personalClass.Name = model.Name;
                personalClass.CustomerId = model.CustomerId;
                personalClass.InstructorId = model.InstructorId;
                personalClass.RoomId = model.RoomId;
                personalClass.ClassTypeId = model.ClassTypeId;
                personalClass.StartTime = model.StartTime;
                personalClass.EndTime = model.StartTime.AddHours(1);
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

            var personalClass = await _personalClassesRepository.GetByIdAsync(id.Value);
            if (personalClass == null)
            {
                return new NotFoundViewResult("PersonalClassNotFound");
            }

            try
            {
                if (personalClass.CustomerId != null)
                {
                    var customer = await _userRepository.GetCustomerByIdAsync(personalClass.CustomerId.Value);
                    if (customer == null)
                    {
                        return RedirectToAction("UserNotFound", "Account");
                    }
                    customer.ClassesRemaining++;
                    await _userRepository.UpdateCustomerAsync(customer);

                    var notification = new Notification
                    {
                        Title = $"A personal class has been cancelled on {personalClass.StartTime:dd/MM/yyyy HH:mm}.",
                        Message = $"The instructor {personalClass.Instructor.User.FullName} has cancelled your personal class on {personalClass.StartTime:dd/MM/yyyy HH:mm}.",
                        User = customer.User,
                        UserId = customer.User.Id,
                    };

                    await _notificationRepository.CreateAsync(notification);
                }

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

        public IActionResult CustomerPersonalClasses(string username, string filter)
        {
            var classes = _personalClassesRepository.GetAllPersonalClassesWithRelatedDataByUserName(username);

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

        public IActionResult UpcomingPersonalClasses()
        {
            var classes = _personalClassesRepository.GetAllPersonalClassesWithRelatedData();
            var futureAvailableclasses = classes.Where(c => c.EndTime > DateTime.Now && c.CustomerId == null);

            return View(futureAvailableclasses);
        }

        public async Task<IActionResult> Signup(int id)
        {
            var personalClass = await _personalClassesRepository.GetPersonalClassWithAllRelatedData(id);

            if (personalClass == null)
            {
                return new NotFoundViewResult("PersonalClassNotFound");
            }

            if (personalClass.CustomerId != null)
            {
                _flashMessage.Danger("This personal class is already booked.");
                return RedirectToAction(nameof(UpcomingPersonalClasses));
            }

            var customer = await _userRepository.GetCustomerByUserName(this.User.Identity.Name);

            if (customer.ClassesRemaining <= 0 || !customer.MembershipIsActive)
            {
                _flashMessage.Danger("You don't have any available classes remaining in your membership.");
                return RedirectToAction(nameof(UpcomingPersonalClasses));
            }

            personalClass.CustomerId = customer.Id;
            personalClass.Customer = customer;
            await _personalClassesRepository.UpdateAsync(personalClass);

            customer.PersonalClasses.Add(personalClass);
            customer.ClassesRemaining--;
            await _userRepository.UpdateCustomerAsync(customer);

            _flashMessage.Confirmation("You have successfully signed up for the personal class.");
            return RedirectToAction(nameof(UpcomingPersonalClasses));
        }

        public IActionResult CustomerPersonalClassesCalendar()
        {
            return View(); 
        }
    }
}