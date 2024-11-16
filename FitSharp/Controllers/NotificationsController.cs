using FitSharp.Data;
using FitSharp.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[Authorize]
public class NotificationsController : Controller
{
    private readonly IUserHelper _userHelper;
    private readonly IUserRepository _userRepository;
    private readonly INotificationRepository _notificationRepository;

    public NotificationsController(
        IUserHelper userHelper,
        IUserRepository userRepository,
        INotificationRepository notificationRepository)
    {
        _userHelper = userHelper;
        _userRepository = userRepository;
        _notificationRepository = notificationRepository;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userRepository.GetUserByEmailAsync(User.Identity.Name);
        var userRole = await _userHelper.GetRoleNameAsync(user);
        var notifications = _notificationRepository.GetNotifications(user.Id);
        return View(notifications);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return RedirectToAction("NotFound404", "Errors", new { entityName = "Notification" });
        }

        var notification = await _notificationRepository.GetNotificationByIdAsync(id.Value);
        if (notification == null)
        {
            return RedirectToAction("NotFound404", "Errors", new { entityName = "Notification" });
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
        }

        return View(notification);
    }
}