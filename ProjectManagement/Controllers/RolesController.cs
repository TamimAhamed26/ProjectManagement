using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.Models;

namespace ProjectManagement.Controllers
{
    [Authorize] 
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }
        [Authorize(Roles = "SuperAdmin")] 
        public IActionResult Index()
        {
            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        public async Task<IActionResult> UserList()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            if (!currentUserRoles.Contains("SuperAdmin") && !currentUserRoles.Contains("Admin"))
            {
                return Forbid(); 
            }

            var users = _userManager.Users.ToList();
            var roles = _roleManager.Roles.ToList();

            
            var filteredUsers = new List<ApplicationUser>();
            foreach (var user in users)
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                
                if (currentUserRoles.Contains("Admin") && userRoles.Contains("SuperAdmin"))
                    continue;

               
                if (currentUserRoles.Contains("Admin") && userRoles.Contains("Admin"))
                    continue;

                filteredUsers.Add(user);
            }

            var model = new UserRolesViewModel
            {
                Users = filteredUsers,
                Roles = roles
            };

            foreach (var user in filteredUsers)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                model.UserRoles[user.Id] = userRoles.FirstOrDefault();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignRole(string userId, string roleName)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserRoles = await _userManager.GetRolesAsync(currentUser);

            if (!currentUserRoles.Contains("SuperAdmin") && !currentUserRoles.Contains("Admin"))
                return Forbid();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !await _roleManager.RoleExistsAsync(roleName))
                return RedirectToAction("UserList");

            var userCurrentRoles = await _userManager.GetRolesAsync(user);

            if (currentUserRoles.Contains("Admin") && (roleName == "SuperAdmin" || roleName == "Admin"))
            {
                TempData["Error"] = "Admins cannot assign Admin or SuperAdmin roles.";
                return RedirectToAction("UserList");
            }

            if (userCurrentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, userCurrentRoles);

            await _userManager.AddToRoleAsync(user, roleName);

            return RedirectToAction("UserList");
        }
    }
}
