using portfolio.Models;

namespace portfolio.ViewModels
{
    public class PortfolioViewModel
    {
        public Course Course { get; set; } = new Course();

        public Project Project { get; set; } = new Project();

        public ProjectImage ProjectImage { get; set; } = new ProjectImage();
    }
}
