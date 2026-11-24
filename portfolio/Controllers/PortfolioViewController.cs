
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using portfolio.ViewModels;

namespace portfolio.Controllers
{
    public class PortfolioViewController : Controller
    {
        private readonly string connectionString =
            "Host=localhost;Port=5432;Database=portfolio_db;Username=postgres;Password=prayerforlovingsorrow";

        [HttpGet]
        public IActionResult Create()
        {
            return View(new PortfolioViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(PortfolioViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using var con = new NpgsqlConnection(connectionString);
            con.Open();

            using var transaction = con.BeginTransaction();

            try
            {
                // 1. INSERT COURSE
                string courseSql = @"
                    INSERT INTO portfolio.course
                    (course_code, course_name, description)
                    VALUES (@code, @name, @description)
                    RETURNING course_id";

                using var courseCmd = new NpgsqlCommand(
                    courseSql, con, transaction);

                courseCmd.Parameters.AddWithValue(
                    "@code", model.Course.CourseCode.Trim());

                courseCmd.Parameters.AddWithValue(
                    "@name", model.Course.CourseName.Trim());

                courseCmd.Parameters.AddWithValue(
                    "@description",
                    (object?)model.Course.Description ?? DBNull.Value);

                int courseId = Convert.ToInt32(
                    courseCmd.ExecuteScalar());

                // 2. INSERT PROJECT
                string projectSql = @"
                    INSERT INTO portfolio.project
                    (course_id, project_title,
                     project_description, video_link)
                    VALUES (@course, @title, @description, @video)
                    RETURNING project_id";

                using var projectCmd = new NpgsqlCommand(
                    projectSql, con, transaction);

                projectCmd.Parameters.AddWithValue(
                    "@course", courseId);

                projectCmd.Parameters.AddWithValue(
                    "@title", model.Project.ProjectTitle.Trim());

                projectCmd.Parameters.AddWithValue(
                    "@description",
                    (object?)model.Project.ProjectDescription ?? DBNull.Value);

                projectCmd.Parameters.AddWithValue(
                    "@video",
                    (object?)model.Project.VideoLink ?? DBNull.Value);

                int projectId = Convert.ToInt32(
                    projectCmd.ExecuteScalar());

                // 3. INSERT PROJECT IMAGE
                string imageSql = @"
                    INSERT INTO portfolio.project_image
                    (project_id, image_url, image_caption)
                    VALUES (@project, @url, @caption)";

                using var imageCmd = new NpgsqlCommand(
                    imageSql, con, transaction);

                imageCmd.Parameters.AddWithValue(
                    "@project", projectId);

                imageCmd.Parameters.AddWithValue(
                    "@url", model.ProjectImage.ImageUrl.Trim());

                imageCmd.Parameters.AddWithValue(
                    "@caption",
                    (object?)model.ProjectImage.ImageCaption ?? DBNull.Value);

                imageCmd.ExecuteNonQuery();

                // SAVE ALL THREE
                transaction.Commit();

                TempData["Success"] =
                    "Portfolio created successfully!";

                return RedirectToAction("Create");
            }
            catch (Exception)
            {
                transaction.Rollback();

                ModelState.AddModelError("",
                    "Something went wrong while saving your portfolio.");

                return View(model);
            }
        }
    }
}
