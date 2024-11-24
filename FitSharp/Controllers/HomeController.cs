using FitSharp.Data;
using FitSharp.Data.Entities;
using FitSharp.Helpers;
using FitSharp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;
using Vereyon.Web;

namespace FitSharp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IGymRepository _gymsRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPersonalClassRepository _personalClassRepository;
        private readonly IGroupClassRepository _groupClassRepository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IFlashMessage _flashMessage;
        private readonly IMailHelper _mailHelper;

        public HomeController(
            ILogger<HomeController> logger,
            IGymRepository gymsRepository,
            IUserRepository userRepository,
            IPersonalClassRepository personalClassRepository,
            IGroupClassRepository groupClassRepository,
            INotificationRepository notificationRepository,
            IFlashMessage flashMessage,
            IMailHelper mailHelper)
        {
            _logger = logger;
            _gymsRepository = gymsRepository;
            _userRepository = userRepository;
            _personalClassRepository = personalClassRepository;
            _groupClassRepository = groupClassRepository;
            _notificationRepository = notificationRepository;
            _flashMessage = flashMessage;
            _mailHelper = mailHelper;
        }

        public async Task<IActionResult> Index()
        {
            var gyms = await _gymsRepository.GetGymsWithAllRelatedDataAsync();
            var instructors = _userRepository.GetAllInstructorsWithAllRelatedData();

            if (gyms == null || instructors == null)
            {
                return NotFound();
            }

            if (instructors == null)
            {
                return NotFound();
            }

            var model = new HomeIndexViewModel
            {
                Gyms = gyms,
                Instructors = instructors
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult OurInstructors()
        {
            var instructors = _userRepository.GetAllInstructorsWithAllRelatedData();
            if (instructors == null)
            {
                return NotFound();
            }

            return View(instructors);
        }

        public async Task<IActionResult> OurGyms()
        {
            var gyms = await _gymsRepository.GetGymsWithAllRelatedDataAsync();
            if (gyms == null)
            {
                return NotFound();
            }

            return View(gyms);
        }

        public IActionResult CustomersInformations()
        {
            var customers = _userRepository.GetAllCustomersWithAllRelatedData();

            return View(customers);
        }

        public IActionResult PersonalClassesInfo()
        {
            return View();
        }

        public IActionResult GroupClassesInfo()
        {
            return View();
        }

        [Authorize(Roles = "Customer")]
        public IActionResult CombinedClassesCalendar()
        {
            ViewBag.PersonalClasses = _personalClassRepository.GetAllPersonalClassesWithRelatedData();
            ViewBag.GroupClasses = _groupClassRepository.GetAllGroupClassesWithRelatedData();
            return View();
        }

        public async Task<IActionResult> InstructorDetails(int? id)
        {
            // Busca o instrutor com todos os dados relacionados
            var instructor = await _userRepository.GetInstructorWithAllRelatedDataByInstructorIdAsync(id.Value);

            if (instructor == null)
            {
                return View("InstructorNotFound");
            }


            if (this.User.IsInRole("Customer"))
            {
                var customer = await _userRepository.GetCustomerByUserName(User.Identity.Name);
                if (customer == null)
                {
                    return NotFound();
                }

                // Verificar se o cliente já teve aulas com o instrutor
                ViewBag.HasAttendedClasses = await _personalClassRepository.HasAttendedInstructorAsync(customer.Id, id.Value) ||
                                          await _groupClassRepository.HasAttendedInstructorAsync(customer.Id, id.Value);

                // Verificar se o cliente já fez uma review ao instrutor
                ViewBag.HasReviewed = await _userRepository.HasCustomerReviewedInstructorAsync(customer.Id, id.Value);
            }

            return View(instructor);
        }

        public async Task<IActionResult> GymDetails(int? id)
        {
            var gym = await _gymsRepository.GetGymWithAllRelatedDataAsync(id.Value);

            if (gym == null)
            {
                return View("GymNotFound");
            }
                       

            if (this.User.IsInRole("Customer"))
            {
                var customer = await _userRepository.GetCustomerByUserName(User.Identity.Name);
                if (customer == null)
                {
                    return NotFound();
                }

                // Verificar se o cliente já frequentou aulas no ginásio
                ViewBag.HasAttendedClasses = await _personalClassRepository.HasAttendedGymAsync(customer.Id, id.Value) ||
                                              await _groupClassRepository.HasAttendedGymAsync(customer.Id, id.Value);

                // Verificar se o cliente já fez uma review ao ginásio
                ViewBag.HasReviewed = await _gymsRepository.HasCustomerReviewedGymAsync(customer.Id, id.Value);
            }

            return View(gym);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> RequestDataDeletion()
        {
            // Obter o usuário atual
            var user = await _userRepository.GetUserByEmailAsync(User.Identity.Name);

            if (user == null)
            {
                return NotFound();
            }

            // Obter a entidade associada ao usuário
            var entity = await _userRepository.GetEntityByUserIdAsync(user.Id);

            // Verificar se a entidade é um cliente
            Data.Entities.Customer customer = entity as Data.Entities.Customer;

            if (customer == null)
            {
                return NotFound();
            }

            // Obter todos os administradores com dados relacionados
            var admins = _userRepository.GetAllAdminsWithAllRelatedData();

            // Gerar a URL de ação para o painel de administração
            var actionUrl = Url.Action("Index", "Admin");

            // Criar notificações para cada administrador
            foreach (Admin admin in admins)
            {
                var notification = new Notification
                {
                    Title = $"User {customer.User.FullName} requested data deletion.",
                    Message = $"Please delete all this user's data from our database.",
                    Action = $"<a href=\"{actionUrl}\" class=\"btn btn-primary\">Go to admin panel</a>",
                    User = admin.User,
                    UserId = admin.User.Id,
                };

                await _notificationRepository.CreateAsync(notification);
            }

            // Adicionar mensagem de confirmação para o usuário
            _flashMessage.Confirmation("Data deletion request sent. In a maximum of 48 hours, your data will be deleted.");

            // Redirecionar para a página de privacidade
            return RedirectToAction("Privacy");
        }

        public IActionResult OurTeam()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ContactUs()
        {
            return View(new ContactUsViewModel());
        }

        [HttpPost]
        public IActionResult ContactUs(ContactUsViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _flashMessage.Danger("Message could not be sent. Please try again later.");
                return View(model);
            }

            Response response = _mailHelper.SendEmail("fitsharp.support@yopmail.com", model.Subject,
                                    $"<h3 style=\"color:#B70D00;\">Client:</h3>" +
                                    $"<p>{model.Name}</p><br />" +
                                    $"<h3 style=\"color:#B70D00;\">Email:</h3>" +
                                    $"<p>{model.Email}</p><br />" +
                                    $"<h3 style=\"color:#B70D00;\">Subject:</h3>" +
                                    $"<p>{model.Subject}</p><br />" +
                                    $"<h3 style=\"color:#B70D00;\">Message:</h3>" +
                                    $"<p>{model.Message}</p><br />");

            if (!response.IsSuccess)
            {
                _flashMessage.Danger("Message could not be sent. Please try again later.");
                return View(model);
            }

            _flashMessage.Confirmation("Your message has been successfully sent and we will get in touch in the next 48 hours.");
            ModelState.Clear();
            return View();
        }
    }
}