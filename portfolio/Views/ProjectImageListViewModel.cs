
namespace portfolio.ViewModels
{
    public class ProjectImageListViewModel
    {
        public int ImageId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectTitle { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public string? ImageCaption { get; set; }
    }
}
