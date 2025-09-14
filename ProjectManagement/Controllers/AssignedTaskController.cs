using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models;
using TaskStatus = ProjectManagement.Models.TaskStatus;

namespace ProjectManagement.Controllers
{
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
        private async Task<string?> SaveFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0) return null;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                throw new InvalidOperationException("Invalid file format.");

            if (file.Length > 10 * 1024 * 1024)
                throw new InvalidOperationException("File size cannot exceed 10 MB.");

            string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", folderName);
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            string safeFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadDir, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/{folderName}/{safeFileName}";
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            if (User.IsInRole("SuperAdmin") || User.IsInRole("Admin") || User.IsInRole("Manager"))
                return RedirectToAction(nameof(AIndex));
            else
                return RedirectToAction(nameof(EIndex));
        }

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> EIndex(string? dueDateFilter, int? taskId)
        {
            var user = await _userManager.GetUserAsync(User);

            var query = _context.AssignedTasks
                .Include(t => t.Task)
                .Where(t => t.UserId == user.Id);

            if (taskId.HasValue)
            {
                query = query.Where(t => t.AssignedTaskId == taskId.Value);
            }

            if (!taskId.HasValue && !string.IsNullOrWhiteSpace(dueDateFilter))
            {
                var today = DateTime.Today;

                switch (dueDateFilter)
                {
                    case "7days":
                        query = query.Where(t => t.DueDate >= today && t.DueDate <= today.AddDays(7));
                        break;
                    case "15days":
                        query = query.Where(t => t.DueDate >= today && t.DueDate <= today.AddDays(15));
                        break;
                    case "30days":
                        query = query.Where(t => t.DueDate >= today && t.DueDate <= today.AddDays(30));
                        break;
                }
            }

            var tasks = await query.ToListAsync();

            // mark overdue
            foreach (var task in tasks)
            {
                task.UpdateOverdueStatus();
            }
            await _context.SaveChangesAsync();

            //  Ordering
            tasks = tasks
                .OrderBy(t => t.Status == ProjectManagement.Models.TaskStatus.Pending ? 0 :
                              t.Status == ProjectManagement.Models.TaskStatus.Overdue ? 1 :
                              t.Status == ProjectManagement.Models.TaskStatus.InProgress ? 2 :
                              t.Status == ProjectManagement.Models.TaskStatus.PendingConfirmation ? 3 : 4)
                .ThenBy(t => t.DueDate)
                .ToList();

            return View(tasks);
        }



        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        public async Task<IActionResult> AIndex()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);

            IQueryable<AssignedTask> query = _context.AssignedTasks
                .Include(t => t.Task)
                .Include(t => t.AssignedBy)
                .Include(t => t.User);

            if (!roles.Contains("SuperAdmin"))
            {
                query = query.Where(t => t.AssignedById == currentUser.Id);
            }

            var tasks = await query.ToListAsync();

            //  mark overdue
            foreach (var task in tasks)
            {
                task.UpdateOverdueStatus();
            }
            await _context.SaveChangesAsync();

            return View(tasks);
        }



        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Assign()
        {
            var departments = await _context.Departments.ToListAsync();
            var viewModel = new AssignTaskViewModel
            {
                Departments = departments.Select(d => new SelectListItem
                { Value = d.DepartmentId.ToString(), Text = d.Name }).ToList(),
                Employees = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Select Employee --" } },
                Categories = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Select Category --" } },
                Tasks = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Select Task --" } },
                DueDate = DateTime.Now.AddDays(7)
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> Assign(AssignTaskViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Departments = _context.Departments
                    .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.Name }).ToList();
                model.Employees = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Select Employee --" } };
                model.Categories = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Select Category --" } };
                model.Tasks = new List<SelectListItem> { new SelectListItem { Value = "", Text = "-- Select Task --" } };
                return View(model);
            }

            var currentUser = await _userManager.GetUserAsync(User);

            var assignedTask = new AssignedTask
            {
                TaskListId = model.TaskListId,
                UserId = model.UserId,
                AssignedById = currentUser?.Id, //  who assigned
                AssignedDate = DateTime.Now,
                DueDate = model.DueDate,
                Status = ProjectManagement.Models.TaskStatus.Pending,
                ReferenceLink = model.ReferenceLink
            };

            // reference file if uploaded
            if (model.ReferenceFile != null)
            {
                try
                {
                    assignedTask.ReferenceFilePath = await SaveFileAsync(model.ReferenceFile, "TaskReferences");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("ReferenceFile", ex.Message);
                    return View(model);
                }
            }

            _context.AssignedTasks.Add(assignedTask);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateDueDate(int id, DateTime dueDate)
        {
            var task = await _context.AssignedTasks.FindAsync(id);
            if (task == null)
                return NotFound();

            if (task.Status == ProjectManagement.Models.TaskStatus.Completed)
            {
                TempData["Error"] = "Cannot change due date of a completed task.";
                return RedirectToAction(nameof(Index));
            }

            task.DueDate = dueDate;
            task.UpdateOverdueStatus();

            await _context.SaveChangesAsync();

            TempData["Success"] = "Due date updated successfully.";
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetEmployeesByDepartment(int departmentId)
        {
            var employees = await _userManager.GetUsersInRoleAsync("Employee");
            var filtered = employees.Where(e => e.DepartmentId == departmentId)
                                    .Select(e => new { e.Id, e.UserName }).ToList();

            return Json(filtered);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetCategoriesByDepartment(int departmentId)
        {
            var categories = await _context.Categories
                .Where(c => c.DepartmentId == departmentId)
                .Select(c => new { c.CategoryId, c.Name })
                .ToListAsync();

            return Json(categories);
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public async Task<IActionResult> GetTasksByCategory(int categoryId)
        {
            var assignedTaskIds = await _context.AssignedTasks
                .Select(a => a.TaskListId)
                .ToListAsync();

            var tasks = await _context.Tasklists
                .Where(t => t.CategoryId == categoryId && !assignedTaskIds.Contains(t.TasklistId))
                .Select(t => new { t.TasklistId, t.Title })
                .ToListAsync();

            return Json(tasks);

        }
        [Authorize(Roles = "Employee")]
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(
     int id,
     ProjectManagement.Models.TaskStatus? newStatus,
     IFormFile? submissionFile,
     string? submissionLink,
     string? remark,
     string? returnUrl 
 )
        {
            var user = await _userManager.GetUserAsync(User);
            var task = await _context.AssignedTasks.FindAsync(id);

            if (task == null || task.UserId != user.Id)
                return Unauthorized();

            // Auto mark overdue
            if (task.Status != ProjectManagement.Models.TaskStatus.Completed &&
                task.DueDate.Date < DateTime.Today)
            {
                task.Status = ProjectManagement.Models.TaskStatus.Overdue;
            }

            // Add remark
            if (!string.IsNullOrWhiteSpace(remark) &&
                task.Status != ProjectManagement.Models.TaskStatus.Completed)
            {
                task.Remarks = string.IsNullOrWhiteSpace(task.Remarks)
                    ? $"[Remark by {user.UserName} at {DateTime.Now}] {remark.Trim()}"
                    : $"{task.Remarks}\n[Remark by {user.UserName} at {DateTime.Now}] {remark.Trim()}";
            }

            if (newStatus.HasValue)
            {
                switch (task.Status)
                {
                    case ProjectManagement.Models.TaskStatus.Pending:
                        if (newStatus == ProjectManagement.Models.TaskStatus.InProgress)
                            task.Status = ProjectManagement.Models.TaskStatus.InProgress;
                        else if (newStatus == ProjectManagement.Models.TaskStatus.Completed)
                            task.Status = ProjectManagement.Models.TaskStatus.PendingConfirmation;
                        break;

                    case ProjectManagement.Models.TaskStatus.InProgress:
                    case ProjectManagement.Models.TaskStatus.Overdue:
                        if (newStatus == ProjectManagement.Models.TaskStatus.Completed)
                            task.Status = ProjectManagement.Models.TaskStatus.PendingConfirmation;
                        break;

                    case ProjectManagement.Models.TaskStatus.PendingConfirmation:
                        if (newStatus == ProjectManagement.Models.TaskStatus.InProgress)
                            task.Status = ProjectManagement.Models.TaskStatus.InProgress;
                        break;

                    default:
                        TempData["Error"] = "Invalid status transition.";
                        return RedirectToAction(nameof(EIndex));
                }

                if (submissionFile != null)
                {
                    try
                    {
                        task.SubmissionFilePath = await SaveFileAsync(submissionFile, "TaskSubmissions");
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = ex.Message;
                        return RedirectToAction(nameof(EIndex));
                    }
                }

                if (!string.IsNullOrWhiteSpace(submissionLink))
                {
                    task.SubmissionLink = submissionLink.Trim();
                }

                if (task.Status == ProjectManagement.Models.TaskStatus.PendingConfirmation)
                    task.SubmitDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(EIndex));
        }


        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> AddAdminRemark(int id, string remark)
        {
            var task = await _context.AssignedTasks.FindAsync(id);
            if (task == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(currentUser);
            bool isSuperAdmin = roles.Contains("SuperAdmin");

            if (task.Status == ProjectManagement.Models.TaskStatus.Completed && !isSuperAdmin)
            {
                TempData["Error"] = "Cannot add remarks to a completed task.";
                return RedirectToAction(nameof(Index));
            }

            var username = currentUser?.UserName ?? "Admin";

            if (!string.IsNullOrWhiteSpace(remark))
            {
                var newRemark = $"[Remark by {username} at {DateTime.Now}] {remark.Trim()}";
                task.Remarks = string.IsNullOrWhiteSpace(task.Remarks)
                    ? newRemark
                    : $"{task.Remarks}\n{newRemark}";

                await _context.SaveChangesAsync();
                TempData["Success"] = "Remark added successfully.";
            }

            return RedirectToAction(nameof(Index));
        }



        [Authorize(Roles = "Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> ReviewTask(int id, string? remark, bool confirm = false, bool reject = false)
        {
            var task = await _context.AssignedTasks.FindAsync(id);
            if (task == null) return NotFound();

            if (task.Status != ProjectManagement.Models.TaskStatus.PendingConfirmation)
            {
                TempData["Error"] = "Only tasks pending confirmation can be reviewed.";
                return RedirectToAction(nameof(Index));
            }

            var currentUser = await _userManager.GetUserAsync(User);
            var username = currentUser?.UserName ?? "Admin";

            if (reject)
            {
                task.Status = ProjectManagement.Models.TaskStatus.InProgress;
                var newRemark = string.IsNullOrWhiteSpace(remark)
                    ? $"[Rejected by {username} at {DateTime.Now}] Task sent back to In Progress."
                    : $"[Rejected by {username} at {DateTime.Now}] {remark.Trim()}";

                task.Remarks = string.IsNullOrWhiteSpace(task.Remarks)
                    ? newRemark
                    : $"{task.Remarks}\n{newRemark}";
            }
            else if (confirm)
            {
                task.Status = ProjectManagement.Models.TaskStatus.Completed;
                var newRemark = string.IsNullOrWhiteSpace(remark)
                    ? $"[Confirmed by {username} at {DateTime.Now}] Task marked as Completed."
                    : $"[Confirmed by {username} at {DateTime.Now}] {remark.Trim()}";

                task.Remarks = string.IsNullOrWhiteSpace(task.Remarks)
                    ? newRemark
                    : $"{task.Remarks}\n{newRemark}";
            }
            else
            {
                TempData["Error"] = "No action selected.";
                return RedirectToAction(nameof(Index));
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "SuperAdmin,Admin,Manager")]
        [HttpPost]
        public async Task<IActionResult> Unassign(int id)
        {
            var task = await _context.AssignedTasks.FindAsync(id);
            if (task == null)
            {
                TempData["Error"] = "Task not found.";
                return RedirectToAction(nameof(Index));
            }

            if (task.Status != ProjectManagement.Models.TaskStatus.Pending &&
                task.Status != ProjectManagement.Models.TaskStatus.Overdue) // ✅ also allow overdue
            {
                TempData["Error"] = "Only Pending or Overdue tasks can be unassigned.";
                return RedirectToAction(nameof(Index));
            }

            _context.AssignedTasks.Remove(task);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Task has been unassigned successfully.";
            return RedirectToAction(nameof(Index));
        }


    }
}
