CREATE DATABASE base;
GO

USE base;
GO

-- ======================
-- 1️⃣ Thực thể gốc: Account
-- ======================
CREATE TABLE [account] (
    account_id UNIQUEIDENTIFIER DEFAULT NEWID(),
    created_at DATETIME DEFAULT GETDATE(),
    is_active BIT DEFAULT 1,
    role NVARCHAR(50) DEFAULT 'User' NOT NULL,
    PRIMARY KEY (account_id)
);

-- Check constraints
ALTER TABLE [account]
ADD CONSTRAINT CK_Account_CreatedAt CHECK (created_at <= GETDATE());

ALTER TABLE [account]
ADD CONSTRAINT CK_Account_IsActive CHECK (is_active IN (0,1));


-- ======================
-- 2️⃣ Account Identity (multi-login)
-- ======================
CREATE TABLE [account_identity] (
    identity_id UNIQUEIDENTIFIER DEFAULT NEWID(),
    account_id UNIQUEIDENTIFIER NOT NULL,
    provider NVARCHAR(50) NOT NULL,        -- local, google, facebook, phone...
    provider_key NVARCHAR(255) NOT NULL,   -- email, OAuth sub_id, phone
    password_hash VARBINARY(256) NULL,     -- chỉ cho provider = local
    created_at DATETIME DEFAULT GETDATE(),
    last_used DATETIME NULL,
    is_verified BIT DEFAULT 0,
    PRIMARY KEY (identity_id)
);

-- FK
ALTER TABLE [account_identity]
ADD CONSTRAINT FK_AccountIdentity_Account_AccountId
FOREIGN KEY(account_id) REFERENCES [account](account_id);

-- Unique & Check
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


-- ======================
-- 3️⃣ User Profile
-- ======================
CREATE TABLE [user] (
    account_id UNIQUEIDENTIFIER NOT NULL,
    full_name NVARCHAR(200) NOT NULL,
    avatar NVARCHAR(500) NULL,  --https://cdn2.fptshop.com.vn/small/avatar_trang_1_cd729c335b.jpg
    bio NVARCHAR(500) NULL,
    user_address NVARCHAR(500) NULL,
    birthday DATE DEFAULT NULL,
    gender NVARCHAR(20) DEFAULT 'Unknown',
    updated_at DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (account_id)
);

-- FK
ALTER TABLE [user]
ADD CONSTRAINT FK_User_Account
FOREIGN KEY(account_id) REFERENCES [account](account_id);

-- Check constraints
ALTER TABLE [user]
ADD CONSTRAINT CK_User_Birthday
CHECK (birthday IS NULL OR (birthday <= GETDATE() AND DATEDIFF(YEAR, birthday, GETDATE()) <= 250));

ALTER TABLE [user]
ADD CONSTRAINT CK_User_Gender
CHECK (gender IN ('Unknown','Male','Female'));

ALTER TABLE [user]
ADD CONSTRAINT CK_User_UpdatedAt
CHECK (updated_at <= GETDATE());

ALTER TABLE [user]
ADD CONSTRAINT CK_User_FullName_NotEmpty
CHECK (LEN(full_name) > 0);

ALTER TABLE [user]
ADD CONSTRAINT CK_User_Avatar_Url
CHECK (avatar IS NULL OR avatar LIKE 'http%');


-- ======================
-- 4️⃣ User Bank
-- ======================
CREATE TABLE [user_bank] (
    account_number NVARCHAR(100) PRIMARY KEY,
    account_id UNIQUEIDENTIFIER NOT NULL,
    account_amount BIGINT DEFAULT 0,
    currency NVARCHAR(20) DEFAULT 'USD',   -- linh hoạt cho crypto
    updated_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY(account_id) REFERENCES [account](account_id)
);

-- Check constraints
ALTER TABLE [user_bank]
ADD CONSTRAINT CK_UserBank_Amount CHECK (account_amount >= 0);

ALTER TABLE [user_bank]
ADD CONSTRAINT CK_UserBank_Currency
CHECK (currency IS NULL OR LEN(currency) <= 20);

ALTER TABLE [user_bank]
ADD CONSTRAINT CK_UserBank_UpdatedAt
CHECK (updated_at <= GETDATE());


-- ======================
-- 5️⃣ User Society
-- ======================
CREATE TABLE [user_society] (
    account_id UNIQUEIDENTIFIER NOT NULL,
    reputation_score INT DEFAULT 100,
    number_follower INT DEFAULT 0,
    number_following INT DEFAULT 0,
    number_post INT DEFAULT 0,
    number_comment INT DEFAULT 0,
    PRIMARY KEY(account_id),
    FOREIGN KEY(account_id) REFERENCES [account](account_id)
);

-- Check constraints
ALTER TABLE [user_society]
ADD CONSTRAINT CK_UserSociety_PositiveCounts
CHECK (
    reputation_score >= 0 AND
    number_follower >= 0 AND
    number_following >= 0 AND
    number_post >= 0 AND
    number_comment >= 0
);
