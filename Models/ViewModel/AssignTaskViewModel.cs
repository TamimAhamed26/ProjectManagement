using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProjectManagement.Models
{
    public class AssignTaskViewModel
    {
        [Required] 
        public int? DepartmentId { get; set; }

        [Required] 
        public string UserId { get; set; }

        [Required] 
        public int? CategoryId { get; set; }

        [Required][Range(1, int.MaxValue)] 
        public int TaskListId { get; set; }

        [Required][DataType(DataType.Date)] 
        public DateTime DueDate { get; set; }

        [NotMapped]
        public IFormFile? ReferenceFile { get; set; }
        [NotMapped]
        public string? ReferenceLink { get; set; }

        [ValidateNever]
        public List<SelectListItem> Departments { get; set; } = new List<SelectListItem>();
        [ValidateNever]
        public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
        [ValidateNever]
        public List<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        [ValidateNever]
        public List<SelectListItem> Tasks { get; set; } = new List<SelectListItem>();
    }
}
