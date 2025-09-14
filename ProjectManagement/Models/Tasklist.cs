using System.ComponentModel.DataAnnotations;

namespace ProjectManagement.Models
{
    public class Tasklist
    {
        public int TasklistId { get; set; }
        [StringLength(50, ErrorMessage = "Title must be less than or equal to 50 characters.")]

        public string Title { get; set; }
        [StringLength(200, ErrorMessage = "Description must be less than or equal to 200 characters.")]
        public string? Description { get; set; }

     
      public int? CategoryId { get; set; }


      public   Category? Category { get; set; }


     
      public ICollection<AssignedTask> AssignedTasks { get; set; } = new List<AssignedTask>();

    }
}