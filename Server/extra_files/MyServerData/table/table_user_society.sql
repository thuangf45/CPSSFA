-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 5️ User Society
-- ======================
CREATE TABLE [user_society] (
    user_society_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    user_society_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    reputation_score INT DEFAULT 100,
    number_follower INT DEFAULT 0,
    number_following INT DEFAULT 0,
    number_post INT DEFAULT 0,
    number_comment INT DEFAULT 0,

    user_guid UNIQUEIDENTIFIER NOT NULL,
    user_id BIGINT NOT NULL,

    UNIQUE (user_society_guid)
);

-- === FK ===
ALTER TABLE [user_society]
ADD CONSTRAINT FK_UserSociety_User_UserId
FOREIGN KEY(user_id) REFERENCES [user](user_id);

ALTER TABLE [user_society]
ADD CONSTRAINT FK_UserSociety_User_UserGuid
FOREIGN KEY(user_guid) REFERENCES [user](user_guid);

-- === Check constraints ===
ALTER TABLE [user_society]
ADD CONSTRAINT CK_UserSociety_PositiveCounts
CHECK (
    reputation_score >= 0 AND
    number_follower >= 0 AND
    number_following >= 0 AND
    number_post >= 0 AND
    number_comment >= 0
);

GO
-- === Trigger ===
-- Trigger INSERT
CREATE TRIGGER TRG_UserSociety_Insert
ON [user_society]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user_society',
        CAST(i.user_society_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_UserSociety_Update
ON [user_society]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user_society',
        CAST(i.user_society_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.user_society_id = i.user_society_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.user_society_id = i.user_society_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.user_society_id = d.user_society_id;
END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_UserSociety_Delete
ON [user_society]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user_society',
        CAST(d.user_society_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO