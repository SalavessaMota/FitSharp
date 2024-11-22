using FitSharp.Data;
using FitSharp.Data.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FitSharp.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class GymsController : Controller
    {
        private readonly IUserRepository _userRepository;
        private readonly IGymRepository _gymRepository;

        public GymsController(
            IUserRepository userRepository,
            IGymRepository gymRepository)
        {
            _userRepository = userRepository;
            _gymRepository = gymRepository;
        }

        [HttpGet("All")]
        [AllowAnonymous]
        public async Task<JsonResult> GetAllGyms()
        {
            try
            {
                var gyms = await _gymRepository.GetGymsWithAllRelatedDataAsync();

                var gymDtos = gyms.Select(g => new GymDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    Address = g.Address,
                    City = g.City.Name,
                    Country = g.City.Country.Name,
                    NumberOfRooms = g.NumberOfRooms,
                    NumberOfEquipments = g.NumberOfEquipments,
                    Rating = g.Rating,
                    ImageUrl = g.ImageFullPath
                }).ToList();

                return new JsonResult(gymDtos);
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = $"Internal server error: {ex.Message}" })
                {
                    StatusCode = 500
                };
            }
        }

        [HttpGet("AllInstructors")]
        public JsonResult GetAllInstructors()
        {
            var instructors = _userRepository.GetAllInstructorsWithAllRelatedData();

            var instructorDtos = instructors.Select(i => new InstructorDto
            {
                Id = i.Id,
                FullName = i.User.FullName,
                Speciality = i.Speciality,
                Description = i.Description,
                Rating = i.Rating,
                NumberOfGroupClasses = i.GroupClasses?.Count ?? 0,
                NumberOfPersonalClasses = i.PersonalClasses?.Count ?? 0,
                GymName = i.Gym?.Name,
                ImageFullPath = i.User.ImageFullPath
            }).ToList();

            return new JsonResult(instructorDtos);
        }
    }
}