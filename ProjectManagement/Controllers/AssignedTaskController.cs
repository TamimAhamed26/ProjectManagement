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

        if (User.IsInRole("Admin") || User.IsInRole("Manager"))
        {
            var allTasks = await _context.AssignedTasks
                .Include(t => t.Task)
                .Include(t => t.User)
                .ToListAsync();
            return View(allTasks);
        }
        else
        {
            var myTasks = await _context.AssignedTasks
                .Include(t => t.Task)
                .Where(t => t.UserId == user.Id)
                .ToListAsync();
            return View(myTasks);
        }
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
