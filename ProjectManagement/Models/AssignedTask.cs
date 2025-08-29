using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProjectManagement.Models
{

    public enum TaskStatus
    {
        Pending = 1,
        InProgress,
        Completed,
        Overdue
    }
    public class AssignedTask
    {
        public int AssignedTaskId { get; set; }

        [ForeignKey("Task")]
        public int TaskListId { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }   // Identity User uses string Id (GUID), not int

        [DataType(DataType.Date)]
        public DateTime AssignedDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? SubmitDate { get; set; }

        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        [StringLength(50)]
        public string? Remarks { get; set; }

        // Navigation properties
        public virtual Tasklist Task { get; set; }
        public virtual ApplicationUser User { get; set; }

    }

}