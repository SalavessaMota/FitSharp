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

        [HttpGet]
        [Route("Available")]
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
                .Where(gc => gc.StartTime > DateTime.Now && !gc.Customers.Any(c => c.Id == customer.Id))
                .Select(gc => new
                {
                    id = gc.Id,
                    title = gc.Instructor.Speciality,
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
        [Route("Enroll")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Enroll([FromBody] int groupClassId)
        {
            // Obter o ID do usuário autenticado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return BadRequest("User not found");
            }

            // Obter o usuário autenticado
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("User not found");
            }

            var entity = await _userRepository.GetEntityByUserIdAsync(userId);
            Customer customer = entity as Customer; // Faz a conversão explícita para Customer
            if (customer == null)
            {
                return BadRequest("Customer not found");
            }

            if(customer.ClassesRemaining <= 0)
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