-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 1️ Account
-- ======================
CREATE TYPE [Account] AS TABLE
(
    AccountId BIGINT NOT NULL PRIMARY KEY,
    AccountGuid UNIQUEIDENTIFIER NULL,
    Role NVARCHAR(50) NULL,
    CreatedAt DATETIME NULL
);