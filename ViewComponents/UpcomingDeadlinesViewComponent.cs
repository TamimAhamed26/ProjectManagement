using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ProjectManagement.Data;
using ProjectManagement.Models;
using System.Linq;

public class UpcomingDeadlinesViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UpcomingDeadlinesViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IViewComponentResult> InvokeAsync(int count = 5)
    {
        var user = await _userManager.GetUserAsync(HttpContext.User);
        if (user == null)
        {
            return View(Enumerable.Empty<AssignedTask>());
        }

        var tasks = _context.AssignedTasks
            .Where(t => t.UserId == user.Id
                        && (t.Status == ProjectManagement.Models.TaskStatus.Pending
                            || t.Status == ProjectManagement.Models.TaskStatus.InProgress
                            || t.Status == ProjectManagement.Models.TaskStatus.PendingConfirmation))
            .OrderBy(t => t.DueDate)
            .Take(count)
            .ToList();

        return View(tasks);
    }
}
