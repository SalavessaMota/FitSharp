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
    public class PersonalClassesController : Controller
    {
        private readonly IPersonalClassRepository _personalClassRepository;
        private readonly IUserRepository _userRepository;

        public PersonalClassesController(
            IPersonalClassRepository personalClassRepository,
            IUserRepository userRepository)
        {
            _personalClassRepository = personalClassRepository;
            _userRepository = userRepository;
        }

        [HttpGet("Available")]
        [AllowAnonymous]
        public async Task<JsonResult> GetAvailablePersonalClasses()
        {
            var personalClasses = _personalClassRepository
                .GetAllPersonalClassesWithRelatedData()
                .Where(pc => pc.EndTime > DateTime.Now)
                .Select(pc => new PersonalClassDto
                {
                    Id = pc.Id,
                    Title = pc.Name,
                    Gym = pc.Room.Gym.Name,
                    ClassType = pc.Instructor.Speciality,
                    Start = pc.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    End = pc.EndTime.ToString("yyyy-MM-dd HH:mm"),
                    Instructor = pc.Instructor.User.FullName,
                    InstructorScore = pc.Instructor.Rating
                })
                .ToList();

            return new JsonResult(personalClasses);
        }

        [HttpGet("Upcoming")]
        [Authorize(Roles = "Customer")]
        public async Task<JsonResult> GetNextAvailablePersonalClassesForCustomer()
        {
            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userName == null)
            {
                return new JsonResult(new { success = false, message = "User not found." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            var user = await _userRepository.GetUserByEmailAsync(userName);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var entity = await _userRepository.GetEntityByUserIdAsync(user.Id);
            Customer customer = entity as Customer;
            if (customer == null)
            {
                return new JsonResult(new { success = false, message = "Customer not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var availablePersonalClasses = _personalClassRepository
                .GetAllPersonalClassesWithRelatedData()
                .Where(pc => pc.EndTime > DateTime.Now && pc.CustomerId != customer.Id && pc.CustomerId == null)
                .Select(pc => new PersonalClassDto
                {
                    Id = pc.Id,
                    Title = pc.Name,
                    Gym = pc.Room.Gym.Name,
                    ClassType = pc.Instructor.Speciality,
                    Start = pc.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    End = pc.EndTime.ToString("yyyy-MM-dd HH:mm"),
                    Instructor = pc.Instructor.User.FullName,
                    InstructorScore = pc.Instructor.Rating
                })
                .ToList();

            return new JsonResult(availablePersonalClasses);
        }

        [HttpGet("Enrolled")]
        [Authorize(Roles = "Customer")]
        public async Task<JsonResult> GetEnrolledPersonalClassesForCustomer()
        {
            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userName == null)
            {
                return new JsonResult(new { success = false, message = "User not found." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            var user = await _userRepository.GetUserByEmailAsync(userName);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var entity = await _userRepository.GetEntityByUserIdAsync(user.Id);
            Customer customer = entity as Customer;
            if (customer == null)
            {
                return new JsonResult(new { success = false, message = "Customer not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var enrolledPersonalClasses = _personalClassRepository
                .GetAllPersonalClassesWithRelatedData()
                .Where(pc => pc.CustomerId == customer.Id)
                .Select(pc => new PersonalClassDto
                {
                    Id = pc.Id,
                    Title = pc.Name,
                    Gym = pc.Room.Gym.Name,
                    ClassType = pc.Instructor.Speciality,
                    Start = pc.StartTime.ToString("yyyy-MM-dd HH:mm"),
                    End = pc.EndTime.ToString("yyyy-MM-dd HH:mm"),
                    Instructor = pc.Instructor.User.FullName,
                    InstructorScore = pc.Instructor.Rating
                })
                .ToList();

            return new JsonResult(enrolledPersonalClasses);
        }

        [HttpPost]
        [Route("Enroll")]
        [Authorize(Roles = "Customer")]
        public async Task<JsonResult> Enroll([FromBody] int personalClassId)
        {
            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userName == null)
            {
                return new JsonResult(new { success = false, message = "User not found." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            var user = await _userRepository.GetUserByEmailAsync(userName);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var entity = await _userRepository.GetEntityByUserIdAsync(user.Id);
            Customer customer = entity as Customer;
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

            var personalClass = await _personalClassRepository.GetByIdAsync(personalClassId);
            if (personalClass == null)
            {
                return new JsonResult(new { success = false, message = "Personal class not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (personalClass.CustomerId != null)
            {
                return new JsonResult(new { success = false, message = "Personal class is already full." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            if (personalClass.CustomerId == customer.Id)
            {
                return new JsonResult(new { success = false, message = "You are already enrolled in this personal class." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            customer.ClassesRemaining--;
            await _userRepository.UpdateCustomerAsync(customer);

            personalClass.CustomerId = customer.Id;
            personalClass.Customer = customer;
            await _personalClassRepository.UpdateAsync(personalClass);

            return new JsonResult(new { success = true, message = "Successfully enrolled in the personal class!" });
        }

        [HttpPost("Unenroll")]
        [Authorize(Roles = "Customer")]
        public async Task<JsonResult> UnenrollFromPersonalClass([FromBody] int personalClassId)
        {
            var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userName == null)
            {
                return new JsonResult(new { success = false, message = "User not found." })
                {
                    StatusCode = StatusCodes.Status401Unauthorized
                };
            }

            var user = await _userRepository.GetUserByEmailAsync(userName);
            if (user == null)
            {
                return new JsonResult(new { success = false, message = "User not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var entity = await _userRepository.GetEntityByUserIdAsync(user.Id);
            Customer customer = entity as Customer;
            if (customer == null)
            {
                return new JsonResult(new { success = false, message = "Customer not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            var personalClass = await _personalClassRepository.GetByIdAsync(personalClassId);
            if (personalClass == null)
            {
                return new JsonResult(new { success = false, message = "Personal class not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (personalClass.CustomerId != customer.Id)
            {
                return new JsonResult(new { success = false, message = "You are not enrolled in this personal class." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            customer.ClassesRemaining++;
            await _userRepository.UpdateCustomerAsync(customer);

            personalClass.CustomerId = null;
            personalClass.Customer = null;
            await _personalClassRepository.UpdateAsync(personalClass);

            return new JsonResult(new { success = true, message = "Successfully unenrolled from the personal class!" });
        }
    }
}