using Microsoft.AspNetCore.Identity;

namespace ProjectManagement.Models
{
    public class UserRolesViewModel
    {
        public List<ApplicationUser> Users { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public Dictionary<string, string> UserRoles { get; set; } = new();

    }
}
