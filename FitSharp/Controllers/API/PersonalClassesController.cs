using FitSharp.Data;
using FitSharp.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FitSharp.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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

        [HttpGet]
        [AllowAnonymous]
        [Route("Available")]
        public IActionResult GetAvailablePersonalClasses()
        {
            var personalClasses = _personalClassRepository
                .GetAllPersonalClassesWithRelatedData()
                .Where(pc => pc.StartTime > DateTime.Now && pc.CustomerId == null)
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

        [HttpPost]
        [Route("Enroll")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Enroll([FromBody] int personalClassId)
        {
            // Obter o ID do usuário autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { success = false, message = "User not authenticated." });
            }

            // Buscar o Customer associado ao User autenticado
            var entity = await _userRepository.GetEntityByUserIdAsync(userId);
            Customer customer = entity as Customer; // Faz a conversão explícita para Customer
            if (customer == null)
            {
                return NotFound(new { success = false, message = "Customer not found." });
            }

            // Buscar a PersonalClass pelo ID
            var personalClass = await _personalClassRepository.GetByIdAsync(personalClassId);
            if (personalClass == null)
            {
                return NotFound(new { success = false, message = "Personal class not found." });
            }

            // Verificar se a PersonalClass já está atribuída a um Customer
            if (personalClass.CustomerId != null)
            {
                return BadRequest(new { success = false, message = "This class is already booked." });
            }

            // Verificar se o Customer tem classes restantes
            if (customer.ClassesRemaining <= 0)
            {
                return BadRequest(new { success = false, message = "You have no classes remaining." });
            }

            customer.ClassesRemaining--;

            // Associar o Customer à PersonalClass e atualizar na base de dados
            personalClass.CustomerId = customer.Id;
            await _personalClassRepository.UpdateAsync(personalClass);

            return Ok(new { success = true, message = "Successfully enrolled in the class!" });
        }
    }
}