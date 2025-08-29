using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
namespace ProjectManagement.Models
{
    public class AssignTaskViewModel
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a task.")]
        public int TaskListId { get; set; }


        [Required(ErrorMessage = "Please select an employee.")]
        public string UserId { get; set; }


        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [ValidateNever]
        public List<SelectListItem> Tasks { get; set; }

        [ValidateNever]
        public List<SelectListItem> Employees { get; set; }
    }
}
