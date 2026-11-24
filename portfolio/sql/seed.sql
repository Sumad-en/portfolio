-- ==========================================
-- INSERT DATA: COURSE
-- ==========================================
INSERT INTO portfolio.course (course_code, course_name, description)
VALUES
('BSIT', 'Bachelor of Science in Information Technology',
 'Information technology course focused on programming, systems, and technology.');



-- ==========================================
-- INSERT DATA: PROJECT
-- ==========================================
INSERT INTO portfolio.project
(course_id, project_title, project_description, video_link)
VALUES
(1, 'Student Portfolio Website',
 'A personal website showcasing skills, projects, and achievements.',
 'https://www.youtube.com/watch?v=example1');


-- ==========================================
-- INSERT DATA: PROJECT IMAGE
-- ==========================================
INSERT INTO portfolio.project_image
(project_id, image_url, image_caption)
VALUES
(1, 'https://example.com/images/portfolio.png',
 'Homepage of the Student Portfolio Website');