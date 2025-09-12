-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 2️ Account Identity (multi-login)
-- ======================
CREATE TYPE [AccountIdentity] AS TABLE
(
    IdentityId BIGINT NOT NULL PRIMARY KEY,  -- ID tự tăng
    IdentityGuid UNIQUEIDENTIFIER NULL,

    Provider NVARCHAR(50) NOT NULL,        -- local, google, facebook, phone...
    ProviderKey NVARCHAR(255) NULL,   -- email, OAuth sub_id, phone
    PasswordHash VARBINARY(256) NULL,     -- chỉ cho provider = local
    CreatedAt DATETIME NULL,
    LastUsed DATETIME NULL,
    IsVerified BIT NULL,

    AccountId BIGINT NULL,
    AccountGuid UNIQUEIDENTIFIER NULL
);