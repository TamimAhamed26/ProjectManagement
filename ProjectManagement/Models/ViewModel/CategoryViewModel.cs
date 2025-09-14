namespace ProjectManagement.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public int TaskCount { get; set; }
        public List<string>? TaskTitles { get; set; }
        public List<int>? TaskIds { get; set; } // <-- add this
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }
    }
}
