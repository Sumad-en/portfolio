-- ==========================================
-- DATABASE SCHEMA: COURSE AND PROJECT
-- ==========================================
DROP SCHEMA IF EXISTS portfolio CASCADE;
CREATE SCHEMA portfolio;

-- 1. COURSE TABLE
CREATE TABLE portfolio.course (
    course_id INT GENERATED ALWAYS AS IDENTITY,

    course_code VARCHAR(20) NOT NULL,
    course_name VARCHAR(100) NOT NULL,
    description TEXT,

    CONSTRAINT pk_course PRIMARY KEY (course_id),
    CONSTRAINT uq_course_code UNIQUE (course_code),
    CONSTRAINT chk_course_code
        CHECK (LENGTH(TRIM(course_code)) >= 2),
    CONSTRAINT chk_course_name
        CHECK (LENGTH(TRIM(course_name)) > 0)
);


-- 2. PROJECT TABLE
CREATE TABLE portfolio.project (
    project_id INT GENERATED ALWAYS AS IDENTITY,

    course_id INT NOT NULL,
    project_title VARCHAR(150) NOT NULL,
    project_description TEXT,
    video_link VARCHAR(500),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT pk_project PRIMARY KEY (project_id),

    CONSTRAINT fk_project_course
        FOREIGN KEY (course_id)
        REFERENCES course(course_id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    CONSTRAINT chk_project_title
        CHECK (LENGTH(TRIM(project_title)) > 0),

    CONSTRAINT chk_video_link
        CHECK (
            video_link IS NULL
            OR video_link LIKE 'http%'
        )
);


-- 3. PROJECT IMAGE TABLE
CREATE TABLE portfolio.project_image (
    image_id INT GENERATED ALWAYS AS IDENTITY,

    project_id INT NOT NULL,
    image_url VARCHAR(500) NOT NULL,
    image_caption VARCHAR(255),

    CONSTRAINT pk_project_image PRIMARY KEY (image_id),

    CONSTRAINT fk_image_project
        FOREIGN KEY (project_id)
        REFERENCES project(project_id)
        ON UPDATE CASCADE
        ON DELETE CASCADE,

    CONSTRAINT chk_image_url
        CHECK (LENGTH(TRIM(image_url)) > 0)
);