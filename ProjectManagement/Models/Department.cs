using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Models
{
    public class Department
    {
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Name must be less than or equal to 50 characters.")]
        public string Name { get; set; }

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }

}
