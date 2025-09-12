-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 4️ User Bank
-- ======================
CREATE TABLE [user_bank] (
    user_bank_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    account_number NVARCHAR(100),

    account_amount BIGINT DEFAULT 0,
    currency NVARCHAR(20) DEFAULT 'USD',   -- linh hoạt cho crypto
    updated_at DATETIME DEFAULT GETDATE(),
    
    user_guid UNIQUEIDENTIFIER,
    user_id BIGINT NOT NULL,
    UNIQUE (account_number)
);

-- === FK ===
ALTER TABLE [user_bank]
ADD CONSTRAINT FK_UserBank_User_UserId
FOREIGN KEY(user_id) REFERENCES [user](user_id);

ALTER TABLE [user_bank]
ADD CONSTRAINT FK_UserBank_User_UserGuid
FOREIGN KEY(user_guid) REFERENCES [user](user_guid);

-- === Check constraints ===
ALTER TABLE [user_bank]
ADD CONSTRAINT CK_UserBank_Amount CHECK (account_amount >= 0);

ALTER TABLE [user_bank]
ADD CONSTRAINT CK_UserBank_Currency
CHECK (currency IS NULL OR LEN(currency) <= 20);

ALTER TABLE [user_bank]
ADD CONSTRAINT CK_UserBank_UpdatedAt
CHECK (updated_at <= GETDATE());


GO
-- === Trigger ===
-- Trigger INSERT
CREATE TRIGGER TRG_UserBank_Insert
ON [user_bank]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user_bank',
        CAST(i.account_number AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_UserBank_Update
ON [user_bank]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user_bank',
        CAST(i.account_number AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.user_bank_id = i.user_bank_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.user_bank_id = i.user_bank_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.user_bank_id = d.user_bank_id;

END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_UserBank_Delete
ON [user_bank]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user_bank',
        CAST(d.account_number AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO