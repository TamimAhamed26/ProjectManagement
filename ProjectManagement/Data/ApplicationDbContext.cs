using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectManagement.Models;

namespace ProjectManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
      public DbSet<Category> Categories { get; set; }
      public DbSet<Tasklist> Tasklists { get; set; }
      public DbSet<AssignedTask> AssignedTasks { get; set; }
      public DbSet<Department> Departments { get; set; }

    }
}
