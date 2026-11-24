
using Npgsql;
using portfolio.ViewModels;

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

        // SAVE COURSE, PROJECT, AND IMAGE
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
