-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 4️ User Bank
-- ======================
CREATE TYPE [UserBank] AS TABLE
(
    UserBankId BIGINT NOT NULL PRIMARY KEY,
    AccountNumber NVARCHAR(100) NULL,

    AccountAmount BIGINT NULL,
    Currency NVARCHAR(20) NULL,   -- linh hoạt cho crypto
    UpdatedAt DATETIME NULL,
    
    UserGuid UNIQUEIDENTIFIER NULL,
    UserId BIGINT NULL
);