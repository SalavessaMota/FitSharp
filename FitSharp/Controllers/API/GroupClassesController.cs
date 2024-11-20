using FitSharp.Data;
using FitSharp.Data.Dtos;
using FitSharp.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FitSharp.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GroupClassesController : Controller
    {
        private readonly IGroupClassRepository _groupClassRepository;
        private readonly IUserRepository _userRepository;

        public GroupClassesController(
            IGroupClassRepository groupClassRepository,
            IUserRepository userRepository)
        {
            _groupClassRepository = groupClassRepository;
            _userRepository = userRepository;
        }


        [HttpGet("Available")]
        [AllowAnonymous]
        public async Task<JsonResult> GetAvailableGroupClassesAPI()
        {
            var groupClasses = _groupClassRepository
                .GetAllGroupClassesWithRelatedData()
                .Where(gc => gc.EndTime > DateTime.Now)
                .Select(gc => new GroupClassDto
                {
                    Id = gc.Id,
                    Title = gc.Instructor.Speciality,
                    Gym = gc.Room.Gym.Name,
                    ClassType = gc.Instructor.Speciality,
                    Start = gc.StartTime.ToString("yyyy-MM-ddTHH:mm"),
                    End = gc.EndTime.ToString("yyyy-MM-ddTHH:mm"),
                    Instructor = gc.Instructor.User.FullName,
                    InstructorScore = gc.Instructor.Rating
                })
                .ToList();

            return new JsonResult(groupClasses);
        }

        [HttpPost]
        [Route("Enroll")]
        [Authorize(Roles = "Customer")]
        public async Task<JsonResult> Enroll([FromBody] int groupClassId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return new JsonResult(new { success = false, message = "User not found." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var entity = await _userRepository.GetEntityByUserIdAsync(userId);
            Customer customer = entity as Customer; // Faz a conversão explícita para Customer
            if (customer == null)
            {
                return new JsonResult(new { success = false, message = "Customer not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (customer.ClassesRemaining <= 0)
            {
                return new JsonResult(new { success = false, message = "You have no classes remaining." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            var groupClass = await _groupClassRepository.GetByIdAsync(groupClassId);
            if (groupClass == null)
            {
                return new JsonResult(new { success = false, message = "Group class not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (groupClass.Customers.Any(c => c.Id == customer.Id))
            {
                return new JsonResult(new { success = false, message = "User already enrolled in this group class." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            customer.ClassesRemaining--;
            await _userRepository.UpdateCustomerAsync(customer);

            groupClass.Customers.Add(customer);
            await _groupClassRepository.UpdateAsync(groupClass);

            return new JsonResult(new { success = true, message = "Successfully enrolled in the class!" });
        }
    }
}