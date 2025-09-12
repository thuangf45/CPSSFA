-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 3️ User Profile
-- ======================
CREATE TYPE [User] AS TABLE 
(
    UserId BIGINT NOT NULL PRIMARY KEY,
    UserGuid UNIQUEIDENTIFIER NULL,

    FullName NVARCHAR(200) NULL,
    Avatar NVARCHAR(500) NULL,
    Bio NVARCHAR(500) NULL,
    UserAddress NVARCHAR(500) NULL,
    Birthday DATE NULL,
    Gender NVARCHAR(20) NULL,
    UpdatedAt DATETIME NULL,

    AccountId BIGINT NULL,
    AccountGuid UNIQUEIDENTIFIER NULL
);