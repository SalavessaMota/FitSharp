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
using System.Security.Claims;
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

        [Authorize(Roles = "Instructor, Employee")]
        public IActionResult Index(string filter)
        {
            var classes = _groupClassRepository.GetAllGroupClassesWithRelatedData();

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
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Create(CreateGroupClassViewModel model)
        {
            if (ModelState.IsValid)
            {
                var instructor = await _userRepository.GetInstructorByUserName(User.Identity.Name);

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
                    EndTime = model.StartTime.AddHours(1),
                    Informations = model.Informations
                };

                await _groupClassRepository.CreateAsync(groupClass);
                return RedirectToAction(nameof(Index));
            }

            model.Rooms = _gymRepository.GetComboRoomsByInstructorName(User.Identity.Name);
            model.ClassTypes = _classTypeRepository.GetComboClassTypes();

            return View(model);
        }

        [Authorize(Roles = "Instructor, Customer, Employee")]
        public IActionResult Details(int id)
        {
            var groupClass = _groupClassRepository.GetGroupClassWithAllRelatedDataAsync(id).Result;

            if (groupClass == null)
            {
                return new NotFoundViewResult("GroupClassNotFound");
            }

            return View(groupClass);
        }

        [Authorize(Roles = "Instructor")]
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
                Informations = groupClass.Informations,
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Instructor")]
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
                groupClass.EndTime = model.StartTime.AddHours(1);
                groupClass.Informations = model.Informations;

                if (groupClass.EndTime < DateTime.Now)
                {
                    _flashMessage.Danger("You can't edit a class that has already happened.");
                    return View(model);
                }

                if (groupClass.StartTime < DateTime.Now)
                {
                    _flashMessage.Danger("You can't edit a class that has already started.");
                    return View(model);
                }

                await _groupClassRepository.UpdateAsync(groupClass);
                return RedirectToAction(nameof(Index));
            }

            model.Instructors = _userRepository.GetComboInstructors();
            model.Rooms = _gymRepository.GetComboRoomsByInstructorName(User.Identity.Name);
            model.ClassTypes = _classTypeRepository.GetComboClassTypes();

            return View(model);
        }

        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("GroupClassNotFound");
            }

            var groupClass = await _groupClassRepository.GetByIdAsync(id.Value);

            if (groupClass == null)
            {
                return new NotFoundViewResult("GroupClassNotFound");
            }

            if (groupClass.EndTime < DateTime.Now)
            {
                _flashMessage.Danger("You can't delete a class that has already happened.");
                return RedirectToAction(nameof(Index));
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

        [Authorize(Roles = "Customer")]
        public IActionResult CustomerGroupClasses(string filter, string username)
        {
            var classes = _groupClassRepository.GetAllGroupClassesWithRelatedDataByUserName(User.Identity.Name);

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

            return View(classes.ToList());
        }

        [Authorize]
        public IActionResult UpcomingGroupClasses()
        {
            var classes = _groupClassRepository.GetAllGroupClassesWithRelatedData();

            var upcomingGroupClasses = classes.Where(c => c.EndTime > DateTime.Now);

            return View(upcomingGroupClasses);
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> SignUp(int id, string returnUrl)
        {
            // Obter a classe em grupo com todos os dados relacionados
            var groupClass = await _groupClassRepository.GetGroupClassWithAllRelatedDataAsync(id);

            // Verificar se a classe existe
            if (groupClass == null)
            {
                _flashMessage.Danger("Class not found.");
                return RedirectToAction(returnUrl ?? nameof(UpcomingGroupClasses));
            }

            // Obter o cliente autenticado
            var customer = await _userRepository.GetCustomerByUserName(User.Identity.Name);

            // Verificar se o cliente tem aulas restantes ou se a assinatura está ativa
            if (customer.ClassesRemaining <= 0 || !customer.MembershipIsActive)
            {
                _flashMessage.Danger("You don't have any available classes remaining in your membership.");
                return RedirectToAction(returnUrl ?? nameof(UpcomingGroupClasses));
            }

            // Verificar se há vagas disponíveis na classe
            if (groupClass.AvailableSpots <= 0)
            {
                _flashMessage.Danger("There are no available spots for this class.");
                return RedirectToAction(returnUrl ?? nameof(UpcomingGroupClasses));
            }

            // Verificar se o cliente já está inscrito na classe
            if (groupClass.Customers.Any(c => c.User.UserName == customer.User.UserName))
            {
                _flashMessage.Danger("You are already signed up for this class.");
                return RedirectToAction(returnUrl ?? nameof(UpcomingGroupClasses));
            }

            // Adicionar o cliente à classe e atualizar os dados
            groupClass.Customers.Add(customer);
            await _groupClassRepository.UpdateAsync(groupClass);

            // Atualizar o cliente e reduzir o número de aulas restantes
            customer.GroupClasses.Add(groupClass);
            customer.ClassesRemaining--;
            await _userRepository.UpdateCustomerAsync(customer);

            // Adicionar mensagem de sucesso
            _flashMessage.Confirmation("You have successfully signed up for the class.");

            // Redirecionar para a página de retorno ou para a página padrão
            return RedirectToAction(returnUrl ?? nameof(UpcomingGroupClasses));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> SignUpCalendar(int id)
        {
            var groupClass = await _groupClassRepository.GetGroupClassWithAllRelatedDataAsync(id);

            if (groupClass == null)
            {
                return Json(new { success = false, message = "Class not found." });
            }

            var customer = await _userRepository.GetCustomerByUserName(User.Identity.Name);

            if (customer.ClassesRemaining <= 0 || !customer.MembershipIsActive)
            {
                return Json(new { success = false, message = "You don't have any available classes remaining in your membership." });
            }

            if (groupClass.AvailableSpots <= 0)
            {
                return Json(new { success = false, message = "There are no available spots for this class." });
            }

            if (groupClass.Customers.Any(c => c.User.UserName == customer.User.UserName))
            {
                return Json(new { success = false, message = "You are already signed up for this class." });
            }

            groupClass.Customers.Add(customer);
            await _groupClassRepository.UpdateAsync(groupClass);

            customer.GroupClasses.Add(groupClass);
            customer.ClassesRemaining--;
            await _userRepository.UpdateCustomerAsync(customer);

            return Json(new { success = true, message = "You have successfully signed up for the class." });
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CancelSignUp(int id, string returnUrl)
        {
            var groupClass = await _groupClassRepository.GetGroupClassWithAllRelatedDataAsync(id);

            if (groupClass == null)
            {
                return new NotFoundViewResult("GroupClassNotFound");
            }

            var customer = await _userRepository.GetCustomerByUserName(User.Identity.Name);

            if (!groupClass.Customers.Any(c => c.User.UserName == customer.User.UserName))
            {
                _flashMessage.Danger("You are not signed up for this class.");
                return RedirectToAction(returnUrl ?? nameof(CustomerGroupClasses));
            }

            groupClass.Customers.Remove(customer);
            await _groupClassRepository.UpdateAsync(groupClass);

            customer.ClassesRemaining++;
            await _userRepository.UpdateCustomerAsync(customer);

            _flashMessage.Confirmation("You have successfully canceled your sign up for the class.");

            // Redireciona para o retorno especificado ou para a default view
            return RedirectToAction(returnUrl ?? nameof(UpcomingGroupClasses));
        }

        [HttpGet]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> GetAvailableGroupClasses()
        {
            // Obter o ID do usuário autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "User not authenticated." });
            }

            // Buscar o Customer associado ao usuário autenticado
            var entity = await _userRepository.GetEntityByUserIdAsync(userId);
            Customer customer = entity as Customer;
            if (customer == null)
            {
                return NotFound(new { success = false, message = "Customer not found." });
            }
            // Buscar aulas disponíveis, excluindo aquelas em que o Customer já está inscrito
            var groupClasses = _groupClassRepository
                .GetAllGroupClassesWithRelatedData()
                .Where(gc => gc.EndTime > DateTime.Now && !gc.Customers.Any(c => c.Id == customer.Id))
                .Select(gc => new
                {
                    id = gc.Id,
                    title = gc.Instructor.Speciality,
                    gym = gc.Room.Gym.Name,
                    classtype = gc.Instructor.Speciality,
                    start = gc.StartTime.ToString("yyyy-MM-ddTHH:mm"),
                    end = gc.EndTime.ToString("yyyy-MM-ddTHH:mm"),
                    instructor = gc.Instructor.User.FullName,
                    instructorscore = gc.Instructor.Rating
                })
                .ToList();

            return Ok(groupClasses);
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Enroll([FromBody] int groupClassId)
        {
            // Obter o ID do usuário autenticado
            var userEmail = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userRepository.GetUserByEmailAsync(userEmail);

            if (user == null)
            {
                return Unauthorized(new { Message = "User not found." });
            }

            var entity = await _userRepository.GetEntityByUserIdAsync(user.Id);
            Customer customer = entity as Customer; // Faz a conversão explícita para Customer
            if (customer == null)
            {
                return BadRequest("Customer not found");
            }

            if (customer.ClassesRemaining <= 0)
            {
                return BadRequest(new { success = false, message = "You have no classes remaining." });
            }

            // Obter a aula em questão
            var groupClass = await _groupClassRepository.GetByIdAsync(groupClassId);
            if (groupClass == null)
            {
                return BadRequest("Group class not found");
            }

            // Verificar se o usuário já está inscrito na aula
            if (groupClass.Customers.Any(c => c.Id == customer.Id))
            {
                return BadRequest("User already enrolled in this group class");
            }

            customer.ClassesRemaining--;
            await _userRepository.UpdateCustomerAsync(customer);

            // Inscrever o usuário na aula
            groupClass.Customers.Add(customer);

            await _groupClassRepository.UpdateAsync(groupClass);

            return Ok(new { success = true, message = "Successfully enrolled in the class!" });
        }
    }
}