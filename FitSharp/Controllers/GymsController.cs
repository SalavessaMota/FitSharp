using FitSharp.Data.Entities;
using FitSharp.Data;
using FitSharp.Helpers;
using FitSharp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using Vereyon.Web;

namespace FitSharp.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class GymsController : Controller
    {
        private readonly IGymRepository _gymRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IFlashMessage _flashMessage;

        public GymsController(
            IGymRepository gymRepository,
            ICountryRepository countryRepository,
            IFlashMessage flashMessage
        )
        {
            _gymRepository = gymRepository;
            _countryRepository = countryRepository;
            _flashMessage = flashMessage;
        }

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
            if (this.ModelState.IsValid)
            {
                var gymId = await _gymRepository.UpdateRoomAsync(room);
                if (gymId != 0)
                {
                    return this.RedirectToAction($"Details", new { id = gymId });
                }
            }

            return this.View(room);
        }

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
                await _gymRepository.AddRoomAsync(model);
                return RedirectToAction("Details", new { id = model.GymId });
            }

            return this.View(model);
        }

        public IActionResult Index()
        {
            return View(_gymRepository.GetGymsWithRooms());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new NotFoundViewResult("GymNotFound");
            }

            var gym = await _gymRepository.GetGymWithRoomsAsync(id.Value);
            if (gym == null)
            {
                return new NotFoundViewResult("GymNotFound");
            }

            return View(gym);
        }

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
                await _gymRepository.UpdateAsync(gym);
                return RedirectToAction(nameof(Index));
            }

            return View(gym);
        }

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
