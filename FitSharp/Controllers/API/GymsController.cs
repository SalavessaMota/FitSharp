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
        [AllowAnonymous]
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

        [HttpGet("mymembership")]
        [Authorize(Roles = "Customer")]
        public async Task<JsonResult> GetMyMembership()
        {
            try
            {
                // Obtém o email ou identificador do utilizador a partir do token
                var userName = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userName))
                {
                    return new JsonResult(new { success = false, message = "User not found." })
                    {
                        StatusCode = StatusCodes.Status401Unauthorized
                    };
                }

                // Busca o utilizador no repositório
                var user = await _userRepository.GetUserByEmailAsync(userName);
                if (user == null)
                {
                    return new JsonResult(new { success = false, message = "User not found." })
                    {
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }

                // Obtém a entidade associada ao utilizador (Customer, neste caso)
                var entity = await _userRepository.GetEntityByUserIdAsync(user.Id);
                if (entity is not Customer customer)
                {
                    return new JsonResult(new { success = false, message = "Customer not found." })
                    {
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }

                // Verifica se o Customer tem uma membership associada
                if (customer.Membership == null)
                {
                    return new JsonResult(new { success = false, message = "Membership not found." })
                    {
                        StatusCode = StatusCodes.Status404NotFound
                    };
                }

                // Mapeia os dados da Membership para o DTO
                var membership = customer.Membership;
                var dto = new CustomerMembershipDto
                {
                    Name = membership.Name,
                    ClassesRemaining = customer.ClassesRemaining,
                    Description = membership.Description,
                    MembershipBeginDate = customer.MembershipBeginDate,
                    MembershipEndDate = customer.MembershipEndDate
                };

                // Retorna o DTO encapsulado em JsonResult com status 200 OK
                return new JsonResult(dto)
                {
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception ex)
            {
                // Retorna um erro genérico em caso de exceção
                return new JsonResult(new { success = false, message = $"An error occurred: {ex.Message}" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
        }

    }
}