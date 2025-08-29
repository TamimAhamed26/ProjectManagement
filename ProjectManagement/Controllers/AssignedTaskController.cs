using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models;

[Authorize]
public class AssignedTaskController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AssignedTaskController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        // Get all roles for the user (could be multiple)
        var roles = await _userManager.GetRolesAsync(user);
        // Pick the first role, or show "No Role" if none
        ViewBag.CurrentRole = roles.FirstOrDefault() ?? "No Role";

        List<AssignedTask> tasks;

        if (User.IsInRole("SuperAdmin") || User.IsInRole("Admin") || User.IsInRole("Manager"))
        {
            tasks = await _context.AssignedTasks
                .Include(t => t.Task)
                .Include(t => t.User)
                .ToListAsync();
        }
        else
        {
            tasks = await _context.AssignedTasks
                .Include(t => t.Task)
                .Where(t => t.UserId == user.Id)
                .ToListAsync();
        }

        return View(tasks);
    }

    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Assign()
    {
        var tasks = await _context.Tasklists.ToListAsync();
        var employees = await _userManager.GetUsersInRoleAsync("Employee");

        var viewModel = new AssignTaskViewModel
        {
            Tasks = tasks.Select(t => new SelectListItem { Value = t.TasklistId.ToString(), Text = t.Title }).ToList(),
            Employees = employees.Select(e => new SelectListItem { Value = e.Id, Text = e.UserName }).ToList(),
            DueDate = DateTime.Now.AddDays(7) // default due date
        };

        return View(viewModel);
    }

    [HttpPost]

    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Assign(AssignTaskViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                                          .Select(e => e.ErrorMessage)
                                          .ToList();
            TempData["Error"] = string.Join(" | ", errors);

            model.Tasks = _context.Tasklists
                .Select(t => new SelectListItem { Value = t.TasklistId.ToString(), Text = t.Title }).ToList();

            model.Employees = (await _userManager.GetUsersInRoleAsync("Employee"))
                .Select(e => new SelectListItem { Value = e.Id, Text = e.UserName }).ToList();

            return View(model);
        }


        var assignedTask = new AssignedTask
        {
            TaskListId = model.TaskListId,
            UserId = model.UserId,
            AssignedDate = DateTime.Now,
            DueDate = model.DueDate,
            Status = ProjectManagement.Models.TaskStatus.Pending
        };

        _context.AssignedTasks.Add(assignedTask);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }


    //  Employee updates status 
    [Authorize(Roles = "Employee")]
    public async Task<IActionResult> UpdateStatus(int id, ProjectManagement.Models.TaskStatus newStatus)
    {
        var user = await _userManager.GetUserAsync(User);
        var task = await _context.AssignedTasks.FindAsync(id);

        if (task == null || task.UserId != user.Id)
            return Unauthorized();

        task.Status = newStatus;

        if (newStatus == ProjectManagement.Models.TaskStatus.Completed)
            task.SubmitDate = DateTime.Now;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> AddRemark(int id, string remark)
    {
        var task = await _context.AssignedTasks.FindAsync(id);
        if (task == null || task.Status != ProjectManagement.Models.TaskStatus.Completed)
            return BadRequest("Task not found or not completed yet.");

        task.Remarks = remark;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}