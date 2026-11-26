
using System.ComponentModel.DataAnnotations;

namespace portfolio.Models
{
    public class ProjectImage
    {
        public int ImageId { get; set; }
        public int ProjectId { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageCaption { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ContentType { get; set; }
    }

}
