using System.ComponentModel.DataAnnotations;

namespace portfolio.Models
{
    public class Course
    {
        public int CourseId { get; set; }

        [Required]
        [StringLength(20)]
        public string CourseCode { get; set; } = "";

        [Required]
        [StringLength(100)]
        public string CourseName { get; set; } = "";

        public string? Description { get; set; }
    }
}