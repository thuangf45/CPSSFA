-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 9 Film
-- ======================
CREATE TYPE [Film] AS TABLE(
    FilmId BIGINT NOT NULL PRIMARY KEY,
    FilmGuid UNIQUEIDENTIFIER NULL,

    FilName NVARCHAR(200) NULL,
    FilmDescription NVARCHAR(1000) NULL,
    FilmCost DECIMAL(18,2) NULL,

    AvgRating DECIMAL(3,2) NULL,
    NumberReview INT NULL,
    NumberView INT NULL,

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    FileId BIGINT NULL,
    FileGuid UNIQUEIDENTIFIER NULL,
    UserGuid UNIQUEIDENTIFIER NULL,
    UserId BIGINT NULL

);