-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 5.1 Post
-- ======================
CREATE Type [Post] AS TABLE
(
    PostId BIGINT NOT NULL PRIMARY KEY,
    PostGuid UNIQUEIDENTIFIER NULL,

    Content NVARCHAR(1000) NULL,
    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,
    NumberComment INT NULL,
    NumberReaction INT NULL,

    ParentId BIGINT NULL,     -- NULL = post gốc, không NULL = comment
    ParentGuid UNIQUEIDENTIFIER NULL,     -- NULL = post gốc, không NULL = comment
    UserSocietyId BIGINT NULL,
    UserSocietyGuid UNIQUEIDENTIFIER NULL
);