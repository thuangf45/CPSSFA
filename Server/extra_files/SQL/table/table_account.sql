-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 1️ Account
-- ======================
CREATE TABLE [account] (
    account_id BIGINT IDENTITY(1,1) PRIMARY KEY,  -- ID tự tăng
    account_guid UNIQUEIDENTIFIER DEFAULT NEWID(), -- GUID duy nhất toàn hệ thống

    created_at DATETIME DEFAULT GETDATE(),
    role NVARCHAR(50) DEFAULT 'User' NOT NULL,

    UNIQUE (account_guid) -- đảm bảo GUID không trùng
);

-- Check constraints
ALTER TABLE [account]
ADD CONSTRAINT CK_Account_CreatedAt CHECK (created_at <= GETDATE());