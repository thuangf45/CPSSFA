-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 5️ User Society
-- ======================
CREATE TYPE [UserSociety] AS TABLE
(
    UserSocietyId BIGINT NOT NULL PRIMARY KEY,
    UserSocietyGuid UNIQUEIDENTIFIER NULL,

    ReputationScore INT NULL,
    NumberFollower INT NULL,
    NumberFollowing INT NULL,
    NumberPost INT NULL,
    NumberComment INT NULL,

    UserGuid UNIQUEIDENTIFIER NULL,
    UserId BIGINT NULL
);