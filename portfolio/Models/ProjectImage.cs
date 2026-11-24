
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models
{
    public class ProjectImage
    {
        public int ImageId { get; set; }

        [Required]
        public int ProjectId { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = "";

        [StringLength(255)]
        public string? ImageCaption { get; set; }
    }
}
