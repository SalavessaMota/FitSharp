﻿using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Helpers;
using FitSharp.Models;
using Microsoft.AspNetCore.Authorization;
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
        private readonly IPersonalClassRepository _personalClassRepository;
        private readonly IGymRepository _gymRepository;
        private readonly IUserRepository _userRepository;
        private readonly IClassTypeRepository _classTypeRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IFlashMessage _flashMessage;

        public PersonalClassesController(
            IPersonalClassRepository personalClassRepository,
            IGymRepository gymRepository,
            IUserRepository userRepository,
            IClassTypeRepository classTypeRepository,
            INotificationRepository notificationRepository,
            IFlashMessage flashMessage)
        {
            _personalClassRepository = personalClassRepository;
            _gymRepository = gymRepository;
            _userRepository = userRepository;
            _classTypeRepository = classTypeRepository;
            _notificationRepository = notificationRepository;
            _flashMessage = flashMessage;
        }

        [Authorize(Roles = "Instructor, Employee")]
        public IActionResult Index(string filter)
        {
            var name = User.Identity.Name;

            var classes = _personalClassRepository.GetAllPersonalClassesWithRelatedData();

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

        [Authorize(Roles = "Instructor")]
        public IActionResult CreateOpenPersonalClass()
        {
            return View();
        }

        [Authorize(Roles = "Instructor")]
        public IActionResult Create()
        {
            var clients = _userRepository.GetAllCustomersWithAllRelatedData();
            var clientsWithMembershipActive = clients.Where(c => c.ClassesRemaining > 0 && c.Membership != null);

            return View(clientsWithMembershipActive);
        }

        [Authorize(Roles = "Instructor")]
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
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreatePersonalClass(CreatePersonalClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var instructor = await _userRepository.GetInstructorByUserName(this.User.Identity.Name);

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

                await _personalClassRepository.CreateAsync(personalClass);

                if (model.CustomerId != 0)
                {
                    var customer = await _userRepository.GetCustomerByIdAsync(model.CustomerId);
                    if (customer != null)
                    {
                        personalClass.CustomerId = model.CustomerId;
                        personalClass.Customer = customer;
                        await _personalClassRepository.UpdateAsync(personalClass);

                        var actionUrl = Url.Action("Details", "PersonalClasses", new { id = personalClass.Id }, protocol: HttpContext.Request.Scheme);
                        var notification = new Notification
                        {
                            Title = "An instructor has scheduled a new personal class.",
                            Message = $"The instructor {instructor.User.FullName} has scheduled a new personal class at the gym {instructor.Gym.Name}, room {personalClass.Room.Name}, at {personalClass.StartTime}.",
                            Action = $"<a href=\"{actionUrl}\" class=\"btn btn-primary\">Go to personal class.</a>",
                            User = customer.User,
                            UserId = customer.User.Id,
                        };

                        await _notificationRepository.CreateAsync(notification);
                        customer.ClassesRemaining--;
                        await _userRepository.UpdateCustomerAsync(customer);
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "The specified customer does not exist.");
                        model.Rooms = _gymRepository.GetComboRoomsByInstructorName(this.User.Identity.Name);
                        model.ClassTypes = _classTypeRepository.GetComboClassTypes();
                        return View(model);
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            model.Rooms = _gymRepository.GetComboRoomsByInstructorName(this.User.Identity.Name);
            model.ClassTypes = _classTypeRepository.GetComboClassTypes();

            return View(model);
        }

        [Authorize(Roles = "Instructor, Customer, Employee")]
        public IActionResult Details(int id)
        {
            var personalClass = _personalClassRepository.GetAllPersonalClassesWithRelatedData()
                .FirstOrDefault(p => p.Id == id);

            if (personalClass == null)
            {
                return new NotFoundViewResult("PersonalClassNotFound");
            }

            return View(personalClass);
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Edit(int id)
        {
            var personalClass = await _personalClassRepository.GetPersonalClassWithAllRelatedData(id);

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
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Edit(EditPersonalClassViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Rooms = await _gymRepository.GetComboRoomsAsync(model.RoomId);
                model.ClassTypes = _classTypeRepository.GetComboClassTypes();
                return View(model);
            }

            var personalClass = await _personalClassRepository.GetPersonalClassWithAllRelatedData(model.Id);
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

            await _personalClassRepository.UpdateAsync(personalClass);

            var actionUrl = Url.Action("Details", "PersonalClasses", new { id = personalClass.Id }, protocol: HttpContext.Request.Scheme);
            var notification = new Notification
            {
                Title = "An instructor has altered a personal class.",
                Message = $"The instructor {personalClass.Instructor.User.FullName} has altered a personal class at the gym {personalClass.Room.Gym.Name}.",
                Action = $"<a href=\"{actionUrl}\" class=\"btn btn-primary\">Go to personal class.</a>",
                User = personalClass.Customer.User,
                UserId = personalClass.Customer.User.Id,
            };

            await _notificationRepository.CreateAsync(notification);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("PersonalClassNotFound");
            }

            var personalClass = await _personalClassRepository.GetPersonalClassWithAllRelatedData(id.Value);
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
                        Title = "An instructor has cancelled a personal class.",
                        Message = $"The instructor {personalClass.Instructor.User.FullName} has cancelled a personal class at the gym {personalClass.Instructor.Gym.Name}.",
                        User = customer.User,
                        UserId = customer.User.Id,
                    };

                    await _notificationRepository.CreateAsync(notification);
                }

                await _personalClassRepository.DeleteAsync(personalClass);
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

        [Authorize(Roles = "Customer")]
        public IActionResult CustomerPersonalClasses(string username, string filter)
        {
            var classes = _personalClassRepository.GetAllPersonalClassesWithRelatedDataByUserName(username);

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

        [Authorize]
        public IActionResult UpcomingPersonalClasses()
        {
            var classes = _personalClassRepository.GetAllPersonalClassesWithRelatedData();
            var futureAvailableclasses = classes.Where(c => c.EndTime > DateTime.Now && c.CustomerId == null);

            return View(futureAvailableclasses);
        }

        public async Task<IActionResult> SignUp(int id, string returnUrl)
        {
            var personalClass = await _personalClassRepository.GetPersonalClassWithAllRelatedData(id);

            if (personalClass == null)
            {
                _flashMessage.Danger("Personal class not found.");
            }

            if (personalClass.CustomerId != null)
            {
                _flashMessage.Danger("This personal class is already booked.");
            }

            var customer = await _userRepository.GetCustomerByUserName(this.User.Identity.Name);

            if (customer.ClassesRemaining <= 0 || !customer.MembershipIsActive)
            {
                _flashMessage.Danger("You don't have any available classes remaining in your membership.");
            }

            personalClass.CustomerId = customer.Id;
            personalClass.Customer = customer;
            await _personalClassRepository.UpdateAsync(personalClass);

            customer.PersonalClasses.Add(personalClass);
            customer.ClassesRemaining--;
            await _userRepository.UpdateCustomerAsync(customer);

            _flashMessage.Confirmation("You have successfully signed up for the personal class.");

            return RedirectToAction(returnUrl ?? nameof(CustomerPersonalClasses), new { username = this.User.Identity.Name, filter = "all" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> SignUpCalendar(int id)
        {
            var personalClass = await _personalClassRepository.GetPersonalClassWithAllRelatedData(id);

            if (personalClass == null)
            {
                return Json(new { success = false, message = "Personal class not found." });
            }

            if (personalClass.CustomerId != null)
            {
                return Json(new { success = false, message = "This personal class is already booked." });
            }

            var customer = await _userRepository.GetCustomerByUserName(this.User.Identity.Name);

            if (customer.ClassesRemaining <= 0 || !customer.MembershipIsActive)
            {
                return Json(new { success = false, message = "You don't have any available classes remaining in your membership." });
            }

            personalClass.CustomerId = customer.Id;
            personalClass.Customer = customer;
            await _personalClassRepository.UpdateAsync(personalClass);

            customer.PersonalClasses.Add(personalClass);
            customer.ClassesRemaining--;
            await _userRepository.UpdateCustomerAsync(customer);

            var actionUrl = Url.Action("Details", "PersonalClasses", new { id = personalClass.Id }, protocol: HttpContext.Request.Scheme);
            var notification = new Notification
            {
                Title = $"A customer has signed up to a personal class on {personalClass.StartTime}.",
                Action = $"<a href=\"{actionUrl}\" class=\"btn btn-primary\">Go to personal class.</a>",
                User = personalClass.Instructor.User,
                UserId = personalClass.Instructor.User.Id,
            };

            await _notificationRepository.CreateAsync(notification);

            return Json(new { success = true, message = "You have successfully signed up for the personal class." });
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CancelSignUp(int id, string returnUrl)
        {
            var personalClass = await _personalClassRepository.GetPersonalClassWithAllRelatedData(id);
            if (personalClass == null)
            {
                return new NotFoundViewResult("PersonalClassNotFound");
            }

            var customer = await _userRepository.GetCustomerByUserName(this.User.Identity.Name);

            if (personalClass.CustomerId != customer.Id)
            {
                _flashMessage.Danger("You are not signed up for this personal class.");
                return RedirectToAction(returnUrl ?? nameof(CustomerPersonalClasses), new { username = this.User.Identity.Name, filter = "all" });
            }

            personalClass.CustomerId = null;
            personalClass.Customer = null;
            await _personalClassRepository.UpdateAsync(personalClass);

            customer.PersonalClasses.Remove(personalClass);
            customer.ClassesRemaining++;
            await _userRepository.UpdateCustomerAsync(customer);

            var actionUrl = Url.Action("Details", "PersonalClasses", new { id = personalClass.Id }, protocol: HttpContext.Request.Scheme);
            var notification = new Notification
            {
                Title = $"A customer has abandoned a personal class on {personalClass.StartTime}.",
                Action = $"<a href=\"{actionUrl}\" class=\"btn btn-primary\">Go to personal class.</a>",
                User = personalClass.Instructor.User,
                UserId = personalClass.Instructor.User.Id,
            };

            await _notificationRepository.CreateAsync(notification);

            _flashMessage.Confirmation("You have successfully cancelled your sign up for the personal class.");

            // Redireciona para o retorno especificado ou uma view padrão
            return RedirectToAction(returnUrl ?? nameof(CustomerPersonalClasses), new { username = this.User.Identity.Name, filter = "all" });
        }

        [HttpGet]
        public IActionResult GetAvailablePersonalClasses()
        {
            var personalClasses = _personalClassRepository
                .GetAllPersonalClassesWithRelatedData()
                .Where(pc => pc.EndTime > DateTime.Now && pc.CustomerId == null)
                .Select(pc => new
                {
                    id = pc.Id,
                    title = pc.Instructor.Speciality,
                    gym = pc.Room.Gym.Name,
                    classtype = pc.Instructor.Speciality,
                    start = pc.StartTime.ToString("yyyy-MM-ddTHH:mm"),
                    end = pc.EndTime.ToString("yyyy-MM-ddTHH:mm"),
                    instructor = pc.Instructor.User.FullName,
                    instructorscore = pc.Instructor.Rating
                })
                .ToList();

            return Ok(personalClasses);
        }
    }
}