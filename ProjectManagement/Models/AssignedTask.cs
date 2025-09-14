using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProjectManagement.Models
{

    public enum TaskStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Overdue = 4,
        PendingConfirmation = 5
    }

    public class AssignedTask
    {
        public int AssignedTaskId { get; set; }

        public string? AssignedById { get; set; }   
        public virtual ApplicationUser? AssignedBy { get; set; }

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

        public string? Remarks { get; set; }


        public string? ReferenceFilePath { get; set; }   
        [NotMapped]
        public IFormFile? ReferenceFile { get; set; }    //by admin
        public string? ReferenceLink { get; set; }

        public string? SubmissionFilePath { get; set; }  
        [NotMapped]
        public IFormFile? SubmissionFile { get; set; }   //  by employee

        public string? SubmissionLink { get; set; }

        public virtual Tasklist Task { get; set; }
        public virtual ApplicationUser User { get; set; }



        public void UpdateOverdueStatus()
        {
            if ((Status == TaskStatus.InProgress || Status == TaskStatus.PendingConfirmation)
                && DueDate.Date < DateTime.Today)
            {
                Status = TaskStatus.Overdue;
            }
            else { 
                if (Status == TaskStatus.Overdue && DueDate.Date >= DateTime.Today)
                {
                    Status = TaskStatus.InProgress;  
                }
            }
        }

    }

}