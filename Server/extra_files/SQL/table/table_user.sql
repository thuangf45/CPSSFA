-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 3️ User Profile
-- ======================
CREATE TABLE [user] (
    user_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    full_name NVARCHAR(200) NOT NULL,
    avatar NVARCHAR(500) NULL,  --https://cdn2.fptshop.com.vn/small/avatar_trang_1_cd729c335b.jpg
    bio NVARCHAR(500) NULL,
    user_address NVARCHAR(500) NULL,
    birthday DATE DEFAULT NULL,
    gender NVARCHAR(20) DEFAULT 'Unknown',
    updated_at DATETIME DEFAULT GETDATE(),

    account_id BIGINT NOT NULL,
    account_guid UNIQUEIDENTIFIER,
    
    UNIQUE (user_guid)
);

-- === FK ===
ALTER TABLE [user]
ADD CONSTRAINT FK_User_Account_AccountId
FOREIGN KEY(account_id) REFERENCES [account](account_id);

ALTER TABLE [user]
ADD CONSTRAINT FK_User_Account_AccountGuid
FOREIGN KEY(account_guid) REFERENCES [account](account_guid);

-- === Check constraints ===
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

GO
-- === Trigger ===
-- Trigger INSERT
CREATE TRIGGER TRG_User_Insert
ON [user]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user',
        CAST(i.user_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_User_Update
ON [user]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user',
        CAST(i.user_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.user_id = i.user_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.user_id = i.user_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.user_id = d.user_id;

END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_User_Delete
ON [user]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user',
        CAST(d.user_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO