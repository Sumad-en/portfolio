
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using portfolio.Models;

namespace portfolio.Controllers
{
    public class CoursesController : Controller
    {
        private readonly string connectionString =
            "Host=aws-1-ap-southeast-2.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.jzbssvuqquqktmmklzrz;Password=prayerforlovingsorrow;SSL Mode=Require;Trust Server Certificate=true";

        public IActionResult Index()
        {
            var courses = new List<Course>();

            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"
                SELECT course_id, course_code,
                       course_name, description
                FROM portfolio.course
                ORDER BY course_id";

            using var cmd = new NpgsqlCommand(sql, con);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                courses.Add(new Course
                {
                    CourseId = reader.GetInt32(0),
                    CourseCode = reader.GetString(1),
                    CourseName = reader.GetString(2),
                    Description = reader.IsDBNull(3)
                        ? null : reader.GetString(3)
                });
            }

            return View(courses);
        }
    }
}
