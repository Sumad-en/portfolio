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
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        // 1. GET ALL PORTFOLIOS
        public async Task<List<PortfolioViewModel>> GetAllPortfoliosAsync()
        {
            var portfolios = new List<PortfolioViewModel>();

            await using var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();

            string sql = @"
                SELECT
                    c.course_id, c.course_code, c.course_name, c.description,
                    p.project_id, p.course_id, p.project_title, p.project_description, p.video_link, p.created_at,
                    i.image_id, i.project_id, i.image_url, i.image_caption
                FROM portfolio.course c
                INNER JOIN portfolio.project p ON c.course_id = p.course_id
                INNER JOIN portfolio.project_image i ON p.project_id = i.project_id
                ORDER BY c.course_id, p.project_id, i.image_id";

            await using var cmd = new NpgsqlCommand(sql, con);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                portfolios.Add(new PortfolioViewModel
                {
                    Course = new Course
                    {
                        CourseId = reader.GetInt32(0),
                        CourseCode = reader.GetString(1),
                        CourseName = reader.GetString(2),
                        Description = reader.IsDBNull(3) ? null : reader.GetString(3)
                    },
                    Project = new Project
                    {
                        ProjectId = reader.GetInt32(4),
                        CourseId = reader.GetInt32(5),
                        ProjectTitle = reader.GetString(6),
                        ProjectDescription = reader.IsDBNull(7) ? null : reader.GetString(7),
                        VideoLink = reader.IsDBNull(8) ? null : reader.GetString(8),
                        CreatedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
                    },
                    ProjectImage = new ProjectImage
                    {
                        ImageId = reader.GetInt32(10),
                        ProjectId = reader.GetInt32(11),
                        ImageUrl = reader.IsDBNull(12) ? null : reader.GetString(12),
                        ImageCaption = reader.IsDBNull(13) ? null : reader.GetString(13)
                    }
                });
            }

            return portfolios;
        }

        // 2. GET SINGLE PORTFOLIO BY COURSE ID
        public async Task<PortfolioViewModel?> GetPortfolioByCourseIdAsync(int courseId)
        {
            await using var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();

            string sql = @"
                SELECT
                    c.course_id, c.course_code, c.course_name, c.description,
                    p.project_id, p.course_id, p.project_title, p.project_description, p.video_link, p.created_at,
                    i.image_id, i.project_id, i.image_url, i.image_caption
                FROM portfolio.course c
                INNER JOIN portfolio.project p ON c.course_id = p.course_id
                INNER JOIN portfolio.project_image i ON p.project_id = i.project_id
                WHERE c.course_id = @courseId";

            await using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@courseId", courseId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new PortfolioViewModel
                {
                    Course = new Course
                    {
                        CourseId = reader.GetInt32(0),
                        CourseCode = reader.GetString(1),
                        CourseName = reader.GetString(2),
                        Description = reader.IsDBNull(3) ? null : reader.GetString(3)
                    },
                    Project = new Project
                    {
                        ProjectId = reader.GetInt32(4),
                        CourseId = reader.GetInt32(5),
                        ProjectTitle = reader.GetString(6),
                        ProjectDescription = reader.IsDBNull(7) ? null : reader.GetString(7),
                        VideoLink = reader.IsDBNull(8) ? null : reader.GetString(8),
                        CreatedAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9)
                    },
                    ProjectImage = new ProjectImage
                    {
                        ImageId = reader.GetInt32(10),
                        ProjectId = reader.GetInt32(11),
                        ImageUrl = reader.IsDBNull(12) ? null : reader.GetString(12),
                        ImageCaption = reader.IsDBNull(13) ? null : reader.GetString(13)
                    }
                };
            }

            return null;
        }

        // 3. GET RAW IMAGE BYTES FOR <img src="...">
        public async Task<(byte[] Data, string ContentType)?> GetImageByIdAsync(int imageId)
        {
            await using var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();

            string sql = @"
                SELECT image_data, content_type
                FROM portfolio.project_image
                WHERE image_id = @imageId AND image_data IS NOT NULL";

            await using var cmd = new NpgsqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@imageId", imageId);

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                byte[] data = (byte[])reader["image_data"];
                string contentType = reader.IsDBNull(1) ? "image/jpeg" : reader.GetString(1);
                return (data, contentType);
            }

            return null;
        }

        // 4. ADD PORTFOLIO (WITH BYTEA IMAGE STORAGE)
        public async Task AddPortfolioAsync(PortfolioViewModel model)
        {
            await using var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();
            await using var transaction = await con.BeginTransactionAsync();

            try
            {
                string courseSql = @"
                    INSERT INTO portfolio.course (course_code, course_name, description)
                    VALUES (@code, @name, @description)
                    RETURNING course_id";

                int courseId;
                await using (var cmd = new NpgsqlCommand(courseSql, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@code", model.Course.CourseCode.Trim());
                    cmd.Parameters.AddWithValue("@name", model.Course.CourseName.Trim());
                    cmd.Parameters.AddWithValue("@description", (object?)model.Course.Description ?? DBNull.Value);
                    courseId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                string projectSql = @"
                    INSERT INTO portfolio.project (course_id, project_title, project_description, video_link)
                    VALUES (@course, @title, @description, @video)
                    RETURNING project_id";

                int projectId;
                await using (var cmd = new NpgsqlCommand(projectSql, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@course", courseId);
                    cmd.Parameters.AddWithValue("@title", model.Project.ProjectTitle.Trim());
                    cmd.Parameters.AddWithValue("@description", (object?)model.Project.ProjectDescription ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@video", (object?)model.Project.VideoLink ?? DBNull.Value);
                    projectId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }

                string imageSql = @"
                    INSERT INTO portfolio.project_image (project_id, image_url, image_caption, image_data, content_type)
                    VALUES (@project, @url, @caption, @data, @contentType)";

                await using (var cmd = new NpgsqlCommand(imageSql, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@project", projectId);
                    cmd.Parameters.AddWithValue("@url", (object?)model.ProjectImage.ImageUrl?.Trim() ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@caption", (object?)model.ProjectImage.ImageCaption ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@data", (object?)model.ProjectImage.ImageData ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@contentType", (object?)model.ProjectImage.ContentType ?? DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 5. UPDATE PORTFOLIO (WITH OPTIONAL NEW IMAGE)
        public async Task UpdatePortfolioAsync(PortfolioViewModel model)
        {
            await using var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();
            await using var transaction = await con.BeginTransactionAsync();

            try
            {
                string courseSql = @"
                    UPDATE portfolio.course
                    SET course_code = @code, course_name = @name, description = @description
                    WHERE course_id = @courseId";

                await using (var cmd = new NpgsqlCommand(courseSql, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@code", model.Course.CourseCode.Trim());
                    cmd.Parameters.AddWithValue("@name", model.Course.CourseName.Trim());
                    cmd.Parameters.AddWithValue("@description", (object?)model.Course.Description ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@courseId", model.Course.CourseId);
                    await cmd.ExecuteNonQueryAsync();
                }

                string projectSql = @"
                    UPDATE portfolio.project
                    SET project_title = @title, project_description = @description, video_link = @video
                    WHERE project_id = @projectId";

                await using (var cmd = new NpgsqlCommand(projectSql, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@title", model.Project.ProjectTitle.Trim());
                    cmd.Parameters.AddWithValue("@description", (object?)model.Project.ProjectDescription ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@video", (object?)model.Project.VideoLink ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@projectId", model.Project.ProjectId);
                    await cmd.ExecuteNonQueryAsync();
                }

                string imageSql = model.ProjectImage.ImageData != null
                    ? @"UPDATE portfolio.project_image
                        SET image_caption = @caption, image_data = @data, content_type = @contentType
                        WHERE image_id = @imageId"
                    : @"UPDATE portfolio.project_image
                        SET image_caption = @caption
                        WHERE image_id = @imageId";

                await using (var cmd = new NpgsqlCommand(imageSql, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@caption", (object?)model.ProjectImage.ImageCaption ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@imageId", model.ProjectImage.ImageId);

                    if (model.ProjectImage.ImageData != null)
                    {
                        cmd.Parameters.AddWithValue("@data", model.ProjectImage.ImageData);
                        cmd.Parameters.AddWithValue("@contentType", model.ProjectImage.ContentType ?? "image/jpeg");
                    }

                    await cmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        // 6. DELETE PORTFOLIO
        public async Task DeletePortfolioAsync(int courseId)
        {
            await using var con = new NpgsqlConnection(_connectionString);
            await con.OpenAsync();
            await using var transaction = await con.BeginTransactionAsync();

            try
            {
                string deleteImagesSql = @"
                    DELETE FROM portfolio.project_image
                    WHERE project_id IN (SELECT project_id FROM portfolio.project WHERE course_id = @courseId)";

                await using (var cmd = new NpgsqlCommand(deleteImagesSql, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    await cmd.ExecuteNonQueryAsync();
                }

                string deleteProjectsSql = @"
                    DELETE FROM portfolio.project
                    WHERE course_id = @courseId";

                await using (var cmd = new NpgsqlCommand(deleteProjectsSql, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    await cmd.ExecuteNonQueryAsync();
                }

                string deleteCourseSql = @"
                    DELETE FROM portfolio.course
                    WHERE course_id = @courseId";

                await using (var cmd = new NpgsqlCommand(deleteCourseSql, con, transaction))
                {
                    cmd.Parameters.AddWithValue("@courseId", courseId);
                    await cmd.ExecuteNonQueryAsync();
                }

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
