using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Entities;
using FitSharp.Helpers;
using FitSharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Vereyon.Web;

namespace FitSharp.Controllers
{
    [Authorize]
    public class GymsController : Controller
    {
        private readonly IGymRepository _gymRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IFlashMessage _flashMessage;

        public GymsController(
            IGymRepository gymRepository,
            ICountryRepository countryRepository,
            IUserRepository userRepository,
            INotificationRepository notificationRepository,
            IFlashMessage flashMessage
        )
        {
            _gymRepository = gymRepository;
            _countryRepository = countryRepository;
            _userRepository = userRepository;
            _notificationRepository = notificationRepository;
            _flashMessage = flashMessage;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var gyms = await _gymRepository.GetGymsWithAllRelatedDataAsync();
            return View(gyms);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Equipments()
        {
            User user = await _userRepository.GetUserByEmailAsync(User.Identity.Name);
            object userRole = await _userRepository.GetEntityByUserIdAsync(user.Id);

            if (userRole is Employee employee)
            {
                var gymEquiments = await _gymRepository.GetGymEquipments(employee.GymId);
                return View(gymEquiments);
            }

            var equiments = await _gymRepository.GetEquipments();
            return View(equiments);
        }

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> ChangeEquipmentStatus(int id)
        {
            var equipment = await _gymRepository.GetEquipmentAsync(id);
            if (equipment == null)
            {
                return RedirectToAction("EquipmentNotFound");
            }

            equipment.RequiresRepair = !equipment.RequiresRepair;
            await _gymRepository.UpdateEquipmentAsync(equipment);

            var admins = _userRepository.GetAllAdminsWithAllRelatedData();
            var actionUrl = Url.Action("Details", "Gyms", new { id = equipment.GymId }, protocol: HttpContext.Request.Scheme);
            foreach (Admin admin in admins)
            {
                if (equipment.RequiresRepair)
                {
                    var notification = new Notification
                    {
                        Title = $"The equipment {equipment.Name} at the gym {equipment.Gym.Name} requires repair.",
                        Message = $"The equipment {equipment.Name} ({equipment.Id}) at the gym {equipment.Gym.Name} ({equipment.Gym.Id}) requires repair.",
                        Action = $"<a href=\"{actionUrl}\" class=\"btn btn-primary\">Go to gym.</a>",
                        User = admin.User,
                        UserId = admin.User.Id,
                    };

                    await _notificationRepository.CreateAsync(notification);
                }
                else
                {
                    var notification = new Notification
                    {
                        Title = $"The equipment {equipment.Name} at the gym {equipment.Gym.Name} has been repaired.",
                        Message = $"The equipment {equipment.Name} ({equipment.Id}) at the gym {equipment.Gym.Name} ({equipment.Gym.Id}) has been repaired.",
                        Action = $"<a href=\"{actionUrl}\" class=\"btn btn-primary\">Go to gym.</a>",
                        User = admin.User,
                        UserId = admin.User.Id,
                    };

                    await _notificationRepository.CreateAsync(notification);
                }
            }

            _flashMessage.Confirmation("Equipment status notification successfully sent.");
            return RedirectToAction("Details", new { id = equipment.Gym.Id });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoom(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("RoomNotFound");
            }

            var room = await _gymRepository.GetRoomAsync(id.Value);
            if (room == null)
            {
                return new NotFoundViewResult("RoomNotFound");
            }

            try
            {
                var gymId = await _gymRepository.DeleteRoomAsync(room);
                return this.RedirectToAction($"Details", new { id = gymId });
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
                {
                    ViewBag.ErrorTitle = $"The room {room.Name} is probably being used!!";
                    ViewBag.ErrorMessage = $"{room.Name}  can't be deleted because it is being used.</br></br>";
                }

                return View("Error");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteEquipment(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("EquipmentNotFound");
            }

            var equipment = await _gymRepository.GetEquipmentAsync(id.Value);
            if (equipment == null)
            {
                return new NotFoundViewResult("EquipmentNotFound");
            }

            try
            {
                var gymId = await _gymRepository.DeleteEquipmentAsync(equipment);
                return this.RedirectToAction($"Details", new { id = gymId });
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
                {
                    ViewBag.ErrorTitle = $"The equipment {equipment.Name} is probably being used!!";
                    ViewBag.ErrorMessage = $"{equipment.Name}  can't be deleted because it is being used.</br></br>";
                }

                return View("Error");
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditRoom(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("RoomNotFound");
            }

            var room = await _gymRepository.GetRoomAsync(id.Value);
            if (room == null)
            {
                return new NotFoundViewResult("RoomNotFound");
            }

            return View(room);
        }

        [HttpPost]
        public async Task<IActionResult> EditRoom(Room room)
        {
            if (room == null)
            {
                return new NotFoundViewResult("RoomNotFound");
            }

            if (this.ModelState.IsValid)
            {
                try
                {
                    var gymId = await _gymRepository.UpdateRoomAsync(room);
                    if (gymId != 0)
                    {
                        return this.RedirectToAction($"Details", new { id = gymId });
                    }
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("UPDATE"))
                    {
                        ViewBag.ErrorTitle = $"The room {room.Name} is probably being used!!";
                        ViewBag.ErrorMessage = $"{room.Name}  can't be edited because it is being used.</br></br>";
                    }

                    return View("Error");
                }
            }

            return this.View(room);
        }

        [Authorize(Roles = "Admin, Employee")]
        public async Task<IActionResult> EditEquipment(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("EquipmentNotFound");
            }

            var equipment = await _gymRepository.GetEquipmentAsync(id.Value);
            if (equipment == null)
            {
                return new NotFoundViewResult("EquipmentNotFound");
            }

            return View(equipment);
        }

        [HttpPost]
        public async Task<IActionResult> EditEquipment(Equipment equipment)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    var gymId = await _gymRepository.UpdateEquipmentAsync(equipment);
                    if (gymId != 0)
                    {
                        return this.RedirectToAction($"Details", new { id = gymId });
                    }
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("UPDATE"))
                    {
                        ViewBag.ErrorTitle = $"The equipment {equipment.Name} is probably being used!!";
                        ViewBag.ErrorMessage = $"{equipment.Name}  can't be edited because it is being used.</br></br>";
                    }

                    return View("Error");
                }
            }

            return this.View(equipment);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRoom(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("RoomNotFound");
            }

            var gym = await _gymRepository.GetByIdAsync(id.Value);
            if (gym == null)
            {
                return new NotFoundViewResult("RoomNotFound");
            }

            var model = new RoomViewModel { GymId = gym.Id };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddRoom(RoomViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var room = new Room
                {
                    Name = model.Name,
                    Capacity = model.Capacity,
                    IsVirtual = model.IsVirtual,
                    GymId = model.GymId
                };

                await _gymRepository.AddRoomAsync(room);
                return RedirectToAction("Details", new { id = model.GymId });
            }

            return this.View(model);
        }

        [Authorize(Roles = "Admin, Employee")]
        public async Task<IActionResult> AddEquipment(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("GymNotFound");
            }

            var gym = await _gymRepository.GetByIdAsync(id.Value);
            if (gym == null)
            {
                return new NotFoundViewResult("GymNotFound");
            }

            var model = new EquipmentViewModel { GymId = gym.Id };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddEquipment(EquipmentViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var equipment = new Equipment
                {
                    Name = model.Name,
                    Description = model.Description,
                    GymId = model.GymId
                };

                await _gymRepository.AddEquipmentAsync(equipment);
                return RedirectToAction("Details", new { id = model.GymId });
            }

            return this.View(model);
        }

        [Authorize(Roles = "Admin, Employee")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("GymNotFound");
            }

            var gym = await _gymRepository.GetGymWithRoomsAndEquipmentsAsync(id.Value);
            if (gym == null)
            {
                return new NotFoundViewResult("GymNotFound");
            }

            return View(gym);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            var model = new GymViewModel
            {
                Countries = _countryRepository.GetComboCountries(),
                Cities = await _countryRepository.GetComboCitiesAsync(0)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GymViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var city = await _countryRepository.GetCityAsync(model.CityId);

            try
            {
                var gym = new Gym
                {
                    Name = model.Name,
                    Address = model.Address,
                    CityId = city.Id,
                };

                await _gymRepository.CreateAsync(gym);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                _flashMessage.Danger("This gym already exists");
            }

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("GymNotFound");
            }

            var gym = await _gymRepository.GetByIdAsync(id.Value);
            if (gym == null)
            {
                return new NotFoundViewResult("GymNotFound");
            }
            return View(gym);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Gym gym)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _gymRepository.UpdateAsync(gym);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException != null && ex.InnerException.Message.Contains("UPDATE"))
                    {
                        ViewBag.ErrorTitle = $"The gym {gym.Name} is probably being used!!";
                        ViewBag.ErrorMessage = $"{gym.Name}  can't be edited because it is being used.</br></br>";
                    }

                    return View("Error");
                }
            }

            return View(gym);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("GymNotFound");
            }

            var gym = await _gymRepository.GetByIdAsync(id.Value);
            if (gym == null)
            {
                return new NotFoundViewResult("GymNotFound");
            }

            try
            {
                await _gymRepository.DeleteAsync(gym);
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException != null && ex.InnerException.Message.Contains("DELETE"))
                {
                    ViewBag.ErrorTitle = $"The Gym {gym.Name} is probably being used!!";
                    ViewBag.ErrorMessage = $"{gym.Name}  can't be deleted because it is being used.</br></br>";
                }

                return View("Error");
            }
        }

        public IActionResult GymNotFound()
        {
            return View();
        }
    }
}