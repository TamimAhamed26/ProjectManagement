Support for ASP.NET Core Identity was added to your project.

For setup and configuration information, see https://go.microsoft.com/fwlink/?linkid=2116645.
aA2!S@
AsA!@#s12

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

////////////
@model ProjectManagement.Models.AssignTaskViewModel
@{
    ViewData["Title"] = "Assign Task";
}

<h2>Assign Task</h2>

<form asp-action="Assign" method="post">
    <div class="form-group">
        <label asp-for="TaskListId">Task</label>
        <select asp-for="TaskListId" asp-items="Model.Tasks" class="form-control">
            <option value="">-- Select Task --</option>
        </select>
        <span asp-validation-for="TaskListId" class="text-danger"></span>
    </div>

    <div class="form-group">
        <label asp-for="UserId">Assign To</label>
        <select asp-for="UserId" asp-items="Model.Employees" class="form-control">
            <option value="">-- Select Employee --</option>
        </select>
        <span asp-validation-for="UserId" class="text-danger"></span>
    </div>

    <div class="form-group">
        <label asp-for="DueDate"></label>
        <input asp-for="DueDate" type="date" class="form-control" />
        <span asp-validation-for="DueDate" class="text-danger"></span>
    </div>

    <button type="submit" class="btn btn-success">Assign</button>
</form>

@model IEnumerable<ProjectManagement.Models.AssignedTask>
@using ProjectManagement.Models
@{
    ViewData["Title"] = "Assigned Tasks";
}

<h2>Assigned Tasks</h2>

<table class="table">
    <thead>
        <tr>
            <th>Task Title</th>
            <th>Assigned To</th>
            <th>Status</th>
            <th>Due Date</th>
            <th>Remarks</th>
            <th>Actions</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model)
        {
            <tr>
                <td>@item.Task.Title</td>
                <td>@item.User?.Name ?? "Unassigned"</td>
                <td>@item.Status</td>
                <td>@item.DueDate.ToShortDateString()</td>
                <td>@item.Remarks</td>
                <td>
                    @if (User.IsInRole("Employee") && item.UserId == User.FindFirst("sub")?.Value)
                    {
                        <form asp-action="UpdateStatus" method="post">
                            <input type="hidden" name="id" value="@item.AssignedTaskId" />
                            <select name="newStatus">
                                <option value="1">Pending</option>
                                <option value="2">In Progress</option>
                                <option value="3">Completed</option>
                                <option value="4">Overdue</option>
                            </select>
                            <button type="submit" class="btn btn-sm btn-primary">Update</button>
                        </form>
                    }

                    @if ((User.IsInRole("Admin") || User.IsInRole("Manager")) && item.Status == ProjectManagement.Models.TaskStatus.Completed)
                    {
                        <form asp-action="AddRemarks" method="post">
                            <input type="hidden" name="id" value="@item.AssignedTaskId" />
                            <input type="text" name="remarks" placeholder="Enter remarks" />
                            <button type="submit" class="btn btn-sm btn-warning">Add</button>

                        </form>
                    }
                </td>
            </tr>
        }
    </tbody>
</table>


using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace ProjectManagement.Models
{
    public class AssignTaskViewModel
    {
        [Required]
        public int TaskListId { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        public List<SelectListItem> Tasks { get; set; }
        public List<SelectListItem> Employees { get; set; }
    }

}



using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Models;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfileController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }


    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        return View(user);
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ApplicationUser model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.PicturePath = user.PicturePath;
            return View(model);
        }

        user.Name = model.Name?.Trim();
        user.PhoneNo = model.PhoneNo?.Trim();
        user.Email = model.Email?.Trim();

        if (model.Picture != null && model.Picture.Length > 0)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(model.Picture.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("Picture", "Only JPG, JPEG, and PNG formats are allowed.");
                model.PicturePath = user.PicturePath;
                return View(model);
            }

            if (model.Picture.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("Picture", "File size cannot exceed 5 MB.");
                model.PicturePath = user.PicturePath;
                return View(model);
            }

            string uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pictures");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            string safeFileName = $"{Guid.NewGuid()}{extension}";
            string filePath = Path.Combine(uploadDir, safeFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await model.Picture.CopyToAsync(stream);
            }

            if (!string.IsNullOrEmpty(user.PicturePath))
            {
                string oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.PicturePath.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    try
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deleting old file: {ex.Message}");
                    }
                }
            }

            user.PicturePath = $"/Pictures/{safeFileName}";
        }

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View(model);
    }

}

@model ProjectManagement.Models.ApplicationUser

@{
    ViewData["Title"] = "Edit Profile";
}

<h2>Edit Profile</h2>

@if (TempData["SuccessMessage"] != null)
{
    <div class="alert alert-success">@TempData["SuccessMessage"]</div>
}

<form asp-action="Edit" method="post" enctype="multipart/form-data">
    <input type="hidden" asp-for="Id" />

    <div class="form-group">
        <label asp-for="Name"></label>
        <input asp-for="Name" class="form-control" />
        <span asp-validation-for="Name" class="text-danger"></span>
    </div>

    <div class="form-group">
        <label asp-for="PhoneNo"></label>
        <input asp-for="PhoneNo" class="form-control" />
        <span asp-validation-for="PhoneNo" class="text-danger"></span>
    </div>

    <div class="form-group">
        <label asp-for="Email"></label>
        <input asp-for="Email" class="form-control" />
        <span asp-validation-for="Email" class="text-danger"></span>
    </div>

    <div class="form-group">
        <label asp-for="Picture">Profile Picture</label>
        <input asp-for="Picture" type="file" class="form-control-file" />
        <span asp-validation-for="Picture" class="text-danger"></span>
    </div>

    @if (!string.IsNullOrEmpty(Model.PicturePath))
    {
        <div class="form-group">
            <label>Current Picture:</label><br />
            <img src="@Model.PicturePath" alt="Profile Picture" width="150" class="img-thumbnail" />
        </div>
    }

    <button type="submit" class="btn btn-primary">Save</button>
    <a asp-action="Index" class="btn btn-secondary">Cancel</a>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}

@model ProjectManagement.Models.ApplicationUser

<h2>My Profile</h2>

<div class="mb-3">
    @if (!string.IsNullOrEmpty(Model.PicturePath))
    {
        <img src="@Model.PicturePath" alt="Profile Picture" width="150" class="img-thumbnail mb-3" />
    }
    else
    {
        <img src="/images/default-profile.png" alt="Default Profile Picture" width="150" class="img-thumbnail mb-3" />
    }
</div>

<div>
    <p><strong>Name:</strong> @Model.Name</p>
    <p><strong>Email:</strong> @Model.Email</p>
    <p><strong>Phone:</strong> @Model.PhoneNo</p>
    <a asp-action="Edit" class="btn btn-primary">Edit Profile</a>
</div>


@using Microsoft.AspNetCore.Identity
@using ProjectManagement.Models
@inject UserManager<ApplicationUser> UserManager

@{
    ApplicationUser currentUser = null;
    string pictureUrl = "/images/default-profile.png";

    if (User.Identity.IsAuthenticated)
    {
        currentUser = await UserManager.GetUserAsync(User);
        if (!string.IsNullOrEmpty(currentUser?.PicturePath))
        {
            pictureUrl = currentUser.PicturePath;
        }
    }
}
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - ProjectManagement</title>
    <script type="importmap"></script>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/ProjectManagement.styles.css" asp-append-version="true" />
</head>
<body>
    <header>
        <nav class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-3">
            <div class="container-fluid">
                <a class="navbar-brand" asp-area="" asp-controller="Home" asp-action="Index">ProjectManagement</a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target=".navbar-collapse" aria-controls="navbarSupportedContent"
                        aria-expanded="false" aria-label="Toggle navigation">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="navbar-collapse collapse d-sm-inline-flex justify-content-between">
                    <ul class="navbar-nav flex-grow-1">
                        <li class="nav-item">
                            <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Index">Home</a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Privacy">Privacy</a>
                        </li>
                    </ul>
                    <ul class="navbar-nav">
                        @if (User.Identity.IsAuthenticated)
                        {
                            <li class="nav-item d-flex align-items-center">
                                <img src="@pictureUrl" alt="Profile Picture" class="rounded-circle" style="width:32px; height:32px; object-fit:cover; margin-right:8px;" />
                                <a class="nav-link" asp-area="Identity" asp-page="/Account/Manage/Index">Profile (@User.Identity.Name)</a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" asp-area="Identity" asp-page="/Account/Logout" asp-route-returnUrl="@Url.Action("Index", "Home", new { area = "" })">Logout</a>
                            </li>
                        }
                        else
                        {
                            <li class="nav-item">
                                <a class="nav-link" asp-area="Identity" asp-page="/Account/Login">Login</a>
                            </li>
                            <li class="nav-item">
                                <a class="nav-link" asp-area="Identity" asp-page="/Account/Register">Register</a>
                            </li>
                        }
                    </ul>
                </div>
            </div>
        </nav>
    </header>
    <div class="container">
        <main role="main" class="pb-3">
            @RenderBody()
        </main>
    </div>

    <footer class="border-top footer text-muted">
        <div class="container">
            &copy; 2025 - ProjectManagement - <a asp-area="" asp-controller="Home" asp-action="Privacy">Privacy</a>
        </div>
    </footer>
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
