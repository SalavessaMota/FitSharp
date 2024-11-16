using FitSharp.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace FitSharp.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class GroupClassesController : Controller
    {
        private readonly IGroupClassRepository _groupClassRepository;

        public GroupClassesController(IGroupClassRepository groupClassRepository)
        {
            _groupClassRepository = groupClassRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("Available")]
        public IActionResult GetAvailableGroupClasses()
        {
            var groupClasses = _groupClassRepository.GetAllGroupClassesWithRelatedData()
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
    }
}