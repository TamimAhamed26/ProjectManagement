using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Models
{
   public class Category
   {
      public int CategoryId { get; set; }

      [Required]
      [StringLength(50, ErrorMessage = "Name must be less than or equal to 50 characters.")]
      public string Name { get; set; }

      public int? DepartmentId { get; set; }
      public virtual Department? Department{ get; set; }


        public ICollection<Tasklist> Tasklists { get; set; } = new List<Tasklist>();
   }
}
