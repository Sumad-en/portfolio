
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models
{
    public class Project
    {
        public int ProjectId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        [StringLength(150)]
        public string ProjectTitle { get; set; } = "";

        public string? ProjectDescription { get; set; }

        [StringLength(500)]
        public string? VideoLink { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}
