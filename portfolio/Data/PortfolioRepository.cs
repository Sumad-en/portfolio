
using Npgsql;
using portfolio.ViewModels;
using portfolio.Models;

namespace portfolio.Data
{
    public class PortfolioRepository
    {
        private readonly string _connectionString;

        public PortfolioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(
                "DefaultConnection")!;
        }

        // ==========================================
        // GET ALL PORTFOLIOS
        // ==========================================

        public async Task<List<PortfolioViewModel>> GetAllPortfoliosAsync()
        {
            var portfolios = new List<PortfolioViewModel>();

            await using var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();

            string sql = @"
                SELECT
                    c.course_id,
                    c.course_code,
                    c.course_name,
                    c.description,

                    p.project_id,
                    p.course_id,
                    p.project_title,
                    p.project_description,
                    p.video_link,
                    p.created_at,

                    i.image_id,
                    i.project_id,
                    i.image_url,
                    i.image_caption

                FROM portfolio.course c

                INNER JOIN portfolio.project p
                    ON c.course_id = p.course_id

                INNER JOIN portfolio.project_image i
                    ON p.project_id = i.project_id

                ORDER BY c.course_id, p.project_id, i.image_id";

            await using var cmd = new NpgsqlCommand(sql, con);

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var portfolio = new PortfolioViewModel
                {
                    Course = new Course
                    {
                        CourseId = reader.GetInt32(0),
                        CourseCode = reader.GetString(1),
                        CourseName = reader.GetString(2),
                        Description = reader.IsDBNull(3)
                            ? null : reader.GetString(3)
                    },

                    Project = new Project
                    {
                        ProjectId = reader.GetInt32(4),
                        CourseId = reader.GetInt32(5),
                        ProjectTitle = reader.GetString(6),
                        ProjectDescription = reader.IsDBNull(7)
                            ? null : reader.GetString(7),
                        VideoLink = reader.IsDBNull(8)
                            ? null : reader.GetString(8),
                        CreatedAt = reader.IsDBNull(9)
                            ? null : reader.GetDateTime(9)
                    },

                    ProjectImage = new ProjectImage
                    {
                        ImageId = reader.GetInt32(10),
                        ProjectId = reader.GetInt32(11),
                        ImageUrl = reader.GetString(12),
                        ImageCaption = reader.IsDBNull(13)
                            ? null : reader.GetString(13)
                    }
                };

                portfolios.Add(portfolio);
            }

            return portfolios;
        }

        // ==========================================
        // SAVE COURSE, PROJECT, AND IMAGE
        // ==========================================

        public async Task AddPortfolioAsync(PortfolioViewModel model)
        {
            await using var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();

            await using var transaction = await con.BeginTransactionAsync();

            try
            {
                // 1. SAVE COURSE

                string courseSql = @"
                    INSERT INTO portfolio.course
                    (course_code, course_name, description)
                    VALUES (@code, @name, @description)
                    RETURNING course_id";

                int courseId;

                await using (var cmd = new NpgsqlCommand(
                    courseSql, con, transaction))
                {
                    cmd.Parameters.AddWithValue(
                        "@code", model.Course.CourseCode.Trim());

                    cmd.Parameters.AddWithValue(
                        "@name", model.Course.CourseName.Trim());

                    cmd.Parameters.AddWithValue(
                        "@description",
                        (object?)model.Course.Description ?? DBNull.Value);

                    courseId = Convert.ToInt32(
                        await cmd.ExecuteScalarAsync());
                }

                // 2. SAVE PROJECT

                string projectSql = @"
                    INSERT INTO portfolio.project
                    (course_id, project_title,
                     project_description, video_link)
                    VALUES (@course, @title, @description, @video)
                    RETURNING project_id";

                int projectId;

                await using (var cmd = new NpgsqlCommand(
                    projectSql, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@course", courseId);

                    cmd.Parameters.AddWithValue(
                        "@title", model.Project.ProjectTitle.Trim());

                    cmd.Parameters.AddWithValue(
                        "@description",
                        (object?)model.Project.ProjectDescription ?? DBNull.Value);

                    cmd.Parameters.AddWithValue(
                        "@video",
                        (object?)model.Project.VideoLink ?? DBNull.Value);

                    projectId = Convert.ToInt32(
                        await cmd.ExecuteScalarAsync());
                }

                // 3. SAVE PROJECT IMAGE

                string imageSql = @"
                    INSERT INTO portfolio.project_image
                    (project_id, image_url, image_caption)
                    VALUES (@project, @url, @caption)";

                await using (var cmd = new NpgsqlCommand(
                    imageSql, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@project", projectId);

                    cmd.Parameters.AddWithValue(
                        "@url", model.ProjectImage.ImageUrl.Trim());

                    cmd.Parameters.AddWithValue(
                        "@caption",
                        (object?)model.ProjectImage.ImageCaption ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }

                // SAVE ALL THREE RECORDS

                await transaction.CommitAsync();


            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }
    }
}
