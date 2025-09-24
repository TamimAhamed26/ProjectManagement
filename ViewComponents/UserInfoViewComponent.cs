using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Data;
using ProjectManagement.Models;
using System.Threading.Tasks;

namespace ProjectManagement.ViewComponents
{
    public class UserInfoViewComponent : ViewComponent
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserInfoViewComponent(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            ApplicationUser? user = null;
            string? department = null;
            string? role = null;

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var currentUser = await _userManager.GetUserAsync(UserClaimsPrincipal);

                if (currentUser != null)
                {
                    // Eager load Department
                    user = await _context.Users
                        .Include(u => u.Department)
                        .FirstOrDefaultAsync(u => u.Id == currentUser.Id);

                    department = user?.Department?.Name;

                    var roles = await _userManager.GetRolesAsync(user);
                    role = string.Join(", ", roles);
                }
            }

            var model = new UserInfoViewModel
            {
                Name = user?.Name ?? "Guest",
                Department = department,
                Role = role ?? ""
            };

            return View(model);
        }
    }

    public class UserInfoViewModel
    {
        public string Name { get; set; } = "";
        public string? Department { get; set; }
        public string Role { get; set; } = "";
    }
}
