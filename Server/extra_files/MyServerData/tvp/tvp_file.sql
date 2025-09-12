-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 8 File
-- ======================
CREATE TYPE [File] AS TABLE
(
    FileId BIGINT NOT NULL PRIMARY KEY,
    FileGuid UNIQUEIDENTIFIER NULL,

    FileName NVARCHAR(200) NULL,
    FileType VARCHAR(30) NULL,
    FileUrl NVARCHAR(MAX) NULL,

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL
    
);