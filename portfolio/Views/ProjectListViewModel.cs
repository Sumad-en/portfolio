
namespace portfolio.ViewModels
{
    public class ProjectListViewModel
    {
        public int ProjectId { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; } = "";
        public string ProjectTitle { get; set; } = "";
        public string? ProjectDescription { get; set; }
        public string? VideoLink { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
