
using Microsoft.AspNetCore.Mvc;
using portfolio.Data;
using portfolio.Models;

namespace portfolio.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly string connectionString =
            "Host=localhost;Port=5432;Database=portfolio_db;Username=postgres;Password=YOUR_PASSWORD";

        // ==========================================
        // COURSE CRUD
        // ==========================================

        [HttpGet("courses")]
        public IActionResult GetCourses()
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"SELECT course_id, course_code,
                           course_name, description
                           FROM portfolio.course
                           ORDER BY course_id";

            using var cmd = new NpgsqlCommand(sql, con);
            using var reader = cmd.ExecuteReader();

            DataTable dt = new DataTable();
            dt.Load(reader);

            return Ok(dt);
        }

        [HttpGet("courses/{id}")]
        public IActionResult GetCourse(int id)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"SELECT * FROM portfolio.course
                           WHERE course_id = @id";

            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return NotFound("Course not found.");

            var course = new
            {
                CourseId = reader.GetInt32(0),
                CourseCode = reader.GetString(1),
                CourseName = reader.GetString(2),
                Description = reader.IsDBNull(3)
                    ? null : reader.GetString(3)
            };

            return Ok(course);
        }

        [HttpPost("courses")]
        public IActionResult CreateCourse(
            [FromBody] CourseRequest course)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"INSERT INTO portfolio.course
                           (course_code, course_name, description)
                           VALUES (@code, @name, @description)
                           RETURNING course_id";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@code",
                course.CourseCode.Trim());

            cmd.Parameters.AddWithValue("@name",
                course.CourseName.Trim());

            cmd.Parameters.AddWithValue("@description",
                (object?)course.Description ?? DBNull.Value);

            int id = Convert.ToInt32(cmd.ExecuteScalar());

            return Ok(new
            {
                Message = "Course created successfully.",
                CourseId = id
            });
        }

        [HttpPut("courses/{id}")]
        public IActionResult UpdateCourse(
            int id, [FromBody] CourseRequest course)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"UPDATE portfolio.course
                           SET course_code = @code,
                               course_name = @name,
                               description = @description
                           WHERE course_id = @id";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@code",
                course.CourseCode.Trim());
            cmd.Parameters.AddWithValue("@name",
                course.CourseName.Trim());
            cmd.Parameters.AddWithValue("@description",
                (object?)course.Description ?? DBNull.Value);

            int rows = cmd.ExecuteNonQuery();

            if (rows == 0)
                return NotFound("Course not found.");

            return Ok("Course updated successfully.");
        }

        [HttpDelete("courses/{id}")]
        public IActionResult DeleteCourse(int id)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"DELETE FROM portfolio.course
                           WHERE course_id = @id";

            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);

            try
            {
                int rows = cmd.ExecuteNonQuery();

                if (rows == 0)
                    return NotFound("Course not found.");

                return Ok("Course deleted successfully.");
            }
            catch (PostgresException ex)
                when (ex.SqlState == "23503")
            {
                return BadRequest(
                    "Cannot delete course because it has projects.");
            }
        }

        // ==========================================
        // PROJECT CRUD
        // ==========================================

        [HttpGet("projects")]
        public IActionResult GetProjects()
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"
                SELECT p.project_id,
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

            DataTable dt = new DataTable();
            dt.Load(reader);

            return Ok(dt);
        }

        [HttpGet("projects/{id}")]
        public IActionResult GetProject(int id)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"SELECT * FROM portfolio.project
                           WHERE project_id = @id";

            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return NotFound("Project not found.");

            var project = new
            {
                ProjectId = reader.GetInt32(0),
                CourseId = reader.GetInt32(1),
                ProjectTitle = reader.GetString(2),
                ProjectDescription = reader.IsDBNull(3)
                    ? null : reader.GetString(3),
                VideoLink = reader.IsDBNull(4)
                    ? null : reader.GetString(4),
                CreatedAt = reader.IsDBNull(5)
                    ? (DateTime?)null : reader.GetDateTime(5)
            };

            return Ok(project);
        }

        [HttpPost("projects")]
        public IActionResult CreateProject(
            [FromBody] ProjectRequest project)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"
                INSERT INTO portfolio.project
                (course_id, project_title,
                 project_description, video_link)
                VALUES (@course, @title, @description, @video)
                RETURNING project_id";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@course", project.CourseId);
            cmd.Parameters.AddWithValue("@title",
                project.ProjectTitle.Trim());
            cmd.Parameters.AddWithValue("@description",
                (object?)project.ProjectDescription ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@video",
                (object?)project.VideoLink ?? DBNull.Value);

            int id = Convert.ToInt32(cmd.ExecuteScalar());

            return Ok(new
            {
                Message = "Project created successfully.",
                ProjectId = id
            });
        }

        [HttpPut("projects/{id}")]
        public IActionResult UpdateProject(
            int id, [FromBody] ProjectRequest project)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"
                UPDATE portfolio.project
                SET course_id = @course,
                    project_title = @title,
                    project_description = @description,
                    video_link = @video
                WHERE project_id = @id";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@course", project.CourseId);
            cmd.Parameters.AddWithValue("@title",
                project.ProjectTitle.Trim());
            cmd.Parameters.AddWithValue("@description",
                (object?)project.ProjectDescription ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@video",
                (object?)project.VideoLink ?? DBNull.Value);

            int rows = cmd.ExecuteNonQuery();

            if (rows == 0)
                return NotFound("Project not found.");

            return Ok("Project updated successfully.");
        }

        [HttpDelete("projects/{id}")]
        public IActionResult DeleteProject(int id)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"DELETE FROM portfolio.project
                           WHERE project_id = @id";

            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();

            if (rows == 0)
                return NotFound("Project not found.");

            return Ok("Project deleted successfully.");
        }

        // ==========================================
        // PROJECT IMAGE CRUD
        // ==========================================

        [HttpGet("images")]
        public IActionResult GetImages()
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"SELECT * FROM portfolio.project_image
                           ORDER BY image_id";

            using var cmd = new NpgsqlCommand(sql, con);
            using var reader = cmd.ExecuteReader();

            DataTable dt = new DataTable();
            dt.Load(reader);

            return Ok(dt);
        }

        [HttpGet("images/{id}")]
        public IActionResult GetImage(int id)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"SELECT * FROM portfolio.project_image
                           WHERE image_id = @id";

            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return NotFound("Image not found.");

            var image = new
            {
                ImageId = reader.GetInt32(0),
                ProjectId = reader.GetInt32(1),
                ImageUrl = reader.GetString(2),
                ImageCaption = reader.IsDBNull(3)
                    ? null : reader.GetString(3)
            };

            return Ok(image);
        }

        [HttpPost("images")]
        public IActionResult CreateImage(
            [FromBody] ImageRequest image)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"
                INSERT INTO portfolio.project_image
                (project_id, image_url, image_caption)
                VALUES (@project, @url, @caption)
                RETURNING image_id";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@project", image.ProjectId);
            cmd.Parameters.AddWithValue("@url", image.ImageUrl.Trim());
            cmd.Parameters.AddWithValue("@caption",
                (object?)image.ImageCaption ?? DBNull.Value);

            int id = Convert.ToInt32(cmd.ExecuteScalar());

            return Ok(new
            {
                Message = "Image created successfully.",
                ImageId = id
            });
        }

        [HttpPut("images/{id}")]
        public IActionResult UpdateImage(
            int id, [FromBody] ImageRequest image)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"
                UPDATE portfolio.project_image
                SET project_id = @project,
                    image_url = @url,
                    image_caption = @caption
                WHERE image_id = @id";

            using var cmd = new NpgsqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@project", image.ProjectId);
            cmd.Parameters.AddWithValue("@url", image.ImageUrl.Trim());
            cmd.Parameters.AddWithValue("@caption",
                (object?)image.ImageCaption ?? DBNull.Value);

            int rows = cmd.ExecuteNonQuery();

            if (rows == 0)
                return NotFound("Image not found.");

            return Ok("Image updated successfully.");
        }

        [HttpDelete("images/{id}")]
        public IActionResult DeleteImage(int id)
        {
            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            string sql = @"DELETE FROM portfolio.project_image
                           WHERE image_id = @id";

            using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@id", id);

            int rows = cmd.ExecuteNonQuery();

            if (rows == 0)
                return NotFound("Image not found.");

            return Ok("Image deleted successfully.");
        }
    }

    // ==========================================
    // REQUEST MODELS
    // ==========================================

    public class CourseRequest
    {
        public string CourseCode { get; set; } = "";
        public string CourseName { get; set; } = "";
        public string? Description { get; set; }
    }

    public class ProjectRequest
    {
        public int CourseId { get; set; }
        public string ProjectTitle { get; set; } = "";
        public string? ProjectDescription { get; set; }
        public string? VideoLink { get; set; }
    }

    public class ImageRequest
    {
        public int ProjectId { get; set; }
        public string ImageUrl { get; set; } = "";
        public string? ImageCaption { get; set; }
    }
}
