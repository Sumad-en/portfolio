
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using portfolio.ViewModels;

namespace portfolio.Controllers
{
    public class ProjectImagesController : Controller
    {
        private readonly string connectionString =
            "Host=localhost;Port=5432;Database=portfolio_db;Username=postgres;Password=YOUR_PASSWORD";

        public IActionResult Index()
        {
            var images = new List<ProjectImageListViewModel>();

            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"
                SELECT i.image_id,
                       i.project_id,
                       p.project_title,
                       i.image_url,
                       i.image_caption
                FROM portfolio.project_image i
                JOIN portfolio.project p
                    ON i.project_id = p.project_id
                ORDER BY i.image_id";

            using var cmd = new NpgsqlCommand(sql, con);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                images.Add(new ProjectImageListViewModel
                {
                    ImageId = reader.GetInt32(0),
                    ProjectId = reader.GetInt32(1),
                    ProjectTitle = reader.GetString(2),
                    ImageUrl = reader.GetString(3),
                    ImageCaption = reader.IsDBNull(4)
                        ? null : reader.GetString(4)
                });
            }

            return View(images);
        }
    }
}
