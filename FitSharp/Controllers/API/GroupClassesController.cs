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
        public async Task<JsonResult> GetAvailableGroupClasses()
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

        [HttpGet("Upcoming")]
        [Authorize(Roles = "Customer")]
        public async Task<JsonResult> GetNextAvailableGroupClassesForCustomer()
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

            var availableGroupClasses = _groupClassRepository
                .GetAllGroupClassesWithRelatedData()
                .Where(gc => gc.EndTime > DateTime.Now && gc.Customers.All(c => c.Id != customer.Id))
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

            return new JsonResult(availableGroupClasses);
        }

        [HttpGet("Enrolled")]
        [Authorize(Roles = "Customer")]
        public async Task<JsonResult> GetEnrolledGroupClassesForCustomer()
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

            var enrolledGroupClasses = _groupClassRepository
                .GetAllGroupClassesWithRelatedData()
                .Where(gc => gc.Customers.Any(c => c.Id == customer.Id))
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

            return new JsonResult(enrolledGroupClasses);
        }



        [HttpPost]
        [Route("Enroll")]
        [Authorize(Roles = "Customer")]
        public async Task<JsonResult> Enroll([FromBody] int groupClassId)
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

        [HttpPost]
        [Route("Unenroll")]
        [Authorize(Roles = "Customer")]
        public async Task<JsonResult> Unenroll([FromBody] int groupClassId)
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

            var groupClass = await _groupClassRepository.GetGroupClassWithAllRelatedDataAsync(groupClassId);
            if (groupClass == null)
            {
                return new JsonResult(new { success = false, message = "Group class not found." })
                {
                    StatusCode = StatusCodes.Status404NotFound
                };
            }

            if (!groupClass.Customers.Any(c => c.Id == customer.Id))
            {
                return new JsonResult(new { success = false, message = "User is not enrolled in this group class." })
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
            }

            // Remove o cliente da lista de inscritos na aula
            groupClass.Customers.Remove(customer);

            // Incrementa o número de aulas restantes do cliente
            customer.ClassesRemaining++;
            await _userRepository.UpdateCustomerAsync(customer);

            // Atualiza a aula na base de dados
            await _groupClassRepository.UpdateAsync(groupClass);

            return new JsonResult(new { success = true, message = "Successfully unenrolled from the class!" });
        }

    }
}