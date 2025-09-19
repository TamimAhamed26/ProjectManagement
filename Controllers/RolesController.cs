using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models;

namespace ProjectManagement.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> UserList()
        {
            var currentuserId = _userManager.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
            var users = _userManager.Users.Where(u => u.Id != currentuserId).ToList();
            var roles = _roleManager.Roles.ToList();
            var departments = await _context.Departments.ToListAsync();

            var model = new UserRolesViewModel
            {
                Users = users,
                Roles = roles,
                Departments = departments
            };

            foreach (var user in model.Users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                model.UserRoles[user.Id] = userRoles.FirstOrDefault();

                if (model.UserRoles[user.Id] == "Manager" || model.UserRoles[user.Id] == "Employee")
                {
                    model.UserDepartments[user.Id] = user.DepartmentId; 
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string roleName, int? departmentId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            if (!currentUserRoles.Contains("SuperAdmin") && !currentUserRoles.Contains("Admin"))
                return Forbid();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return RedirectToAction("UserList");

            // ✅ Check if roleName is null or empty
            if (string.IsNullOrEmpty(roleName) || !await _roleManager.RoleExistsAsync(roleName))
            {
                TempData["Error"] = "Please select a valid role before assigning.";
                return RedirectToAction("UserList");
            }

            var userCurrentRoles = await _userManager.GetRolesAsync(user);

            if (currentUserRoles.Contains("Admin") && (roleName == "SuperAdmin" || roleName == "Admin"))
            {
                TempData["Error"] = "Admins cannot assign Admin or SuperAdmin roles.";
                return RedirectToAction("UserList");
            }

            if (userCurrentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, userCurrentRoles);

            await _userManager.AddToRoleAsync(user, roleName);

            if ((roleName == "Manager" || roleName == "Employee") && departmentId.HasValue)
            {
                user.DepartmentId = departmentId.Value;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }
            else
            {
                user.DepartmentId = null;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("UserList");
        }

        [HttpGet]
        public async Task<IActionResult> GetUnassignedUsers()
        {
            var users = await _userManager.Users
                .Where(u => u.DepartmentId == null) 
                .ToListAsync();

            var result = new List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Manager") || roles.Contains("Employee"))
                {
                    result.Add(new
                    {
                        id = user.Id,
                        name = user.Name,
                        userName = user.UserName,
                        role = roles.FirstOrDefault()
                    });
                }
            }

            return Json(result);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, string roleName)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            role.Name = roleName;
            var result = await _roleManager.UpdateAsync(role);

            

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                TempData["Error"] = "Role not found.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.DeleteAsync(role);

           

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
            {
                TempData["Error"] = "Role name cannot be empty.";
                return RedirectToAction(nameof(Index));
            }

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                TempData["Error"] = $"The role '{roleName}' already exists.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));

          

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> AssignUserToDepartment(string userId, int departmentId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Json(new { success = false, message = "User not found" });

            user.DepartmentId = departmentId;
            _context.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


    }
}
