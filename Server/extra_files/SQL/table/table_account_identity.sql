-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 2️ Account Identity (multi-login)
-- ======================
CREATE TABLE [account_identity] (
    identity_id BIGINT IDENTITY(1,1) PRIMARY KEY,  -- ID tự tăng
    identity_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    provider NVARCHAR(50) NOT NULL,        -- local, google, facebook, phone...
    provider_key NVARCHAR(255) NOT NULL,   -- email, OAuth sub_id, phone
    password_hash VARBINARY(256) NULL,     -- chỉ cho provider = local
    created_at DATETIME DEFAULT GETDATE(),
    last_used DATETIME NULL,
    is_verified BIT DEFAULT 0,

    account_id BIGINT NOT NULL,
    account_guid UNIQUEIDENTIFIER NOT NULL,
    UNIQUE (identity_guid)
);

-- === FK ===
ALTER TABLE [account_identity]
ADD CONSTRAINT FK_AccountIdentity_Account_AccountId
FOREIGN KEY(account_id) REFERENCES [account](account_id);

ALTER TABLE [account_identity]
ADD CONSTRAINT FK_AccountIdentity_Account_AccountGuid
FOREIGN KEY(account_guid) REFERENCES [account](account_guid);

-- === Unique & Check ===
ALTER TABLE [account_identity]
ADD CONSTRAINT UQ_AccountIdentity_ProviderKey UNIQUE(provider, provider_key);

ALTER TABLE [account_identity]
ADD CONSTRAINT CK_AccountIdentity_LocalPassword
CHECK ((provider <> 'local') OR (password_hash IS NOT NULL));

ALTER TABLE [account_identity]
ADD CONSTRAINT CK_AccountIdentity_ProviderKey_NotEmpty
CHECK (LEN(provider_key) > 0);

ALTER TABLE [account_identity]
ADD CONSTRAINT CK_AccountIdentity_LastUsed
CHECK (last_used IS NULL OR last_used <= GETDATE());