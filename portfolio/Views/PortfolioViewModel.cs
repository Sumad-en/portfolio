using Microsoft.AspNetCore.Http;
using portfolio.Models;

namespace portfolio.ViewModels
{
    public class PortfolioViewModel
    {
        public Course Course { get; set; } = new();
        public Project Project { get; set; } = new();
        public ProjectImage ProjectImage { get; set; } = new();

        // Holds the uploaded file from the browser form
        public IFormFile? ImageFile { get; set; }
    }
}
