
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using portfolio.Models;
using portfolio.ViewModels;

namespace portfolio.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly string connectionString =
            "Host=localhost;Port=5432;Database=portfolio_db;Username=postgres;Password=YOUR_PASSWORD";

        public IActionResult Index()
        {
            var projects = new List<ProjectListViewModel>(); ;

            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"SELECT p.project_id,
                        p.course_id,
                        c.course_name,
                        p.project_title,
                        p.project_description,
                        p.video_link,
                        p.created_at
                        FROM portfolio.project p
                        JOIN portfolio.course c
                        ON p.course_id = c.course_id
                        ORDER BY p.project_id";

            using var cmd = new NpgsqlCommand(sql, con);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                projects.Add(new ProjectListViewModel
                {
                    ProjectId = reader.GetInt32(0),
                    CourseId = reader.GetInt32(1),
                    CourseName = reader.GetString(2),
                    ProjectTitle = reader.GetString(3),
                    ProjectDescription = reader.IsDBNull(4)
          ? null : reader.GetString(4),
                    VideoLink = reader.IsDBNull(5)
          ? null : reader.GetString(5),
                    CreatedAt = reader.IsDBNull(6)
          ? null : reader.GetDateTime(6)
                });
            }

            return View(projects);
        }
    }
}
