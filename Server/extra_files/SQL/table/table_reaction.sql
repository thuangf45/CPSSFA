-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 5.2 Reaction
-- ======================
CREATE TABLE [reaction](
    reaction_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    reaction_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    reaction_type NVARCHAR(20),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),

    post_id BIGINT NOT NULL,
    post_guid UNIQUEIDENTIFIER NOT NULL,
    user_society_id BIGINT NOT NULL,
    user_society_guid UNIQUEIDENTIFIER NOT NULL,

    UNIQUE(reaction_guid)
);

-- === FK ===
ALTER TABLE [reaction]
ADD CONSTRAINT FK_Reaction_Post_PostId
FOREIGN KEY(post_id) REFERENCES [post](post_id);

ALTER TABLE [reaction]
ADD CONSTRAINT FK_Reaction_Post_PostGuid
FOREIGN KEY(post_guid) REFERENCES [post](post_guid);

ALTER TABLE [reaction]
ADD CONSTRAINT FK_Reaction_UserSocietyId
FOREIGN KEY(user_society_id) REFERENCES [user_society](user_society_id);

ALTER TABLE [reaction]
ADD CONSTRAINT FK_Reaction_UserSocietyGuid
FOREIGN KEY(user_society_guid) REFERENCES [user_society](user_society_guid);

-- === Check constraints ===
ALTER TABLE [reaction]
ADD CONSTRAINT CK_Reaction_Date
CHECK (
    created_at <= GETDATE() AND
    updated_at <= GETDATE()
);

ALTER TABLE [reaction]
ADD CONSTRAINT CK_Reaction_Type
CHECK (
    reaction_type IN ('like', 'heart', 'haha', 'huhu')
);

GO
-- === Trigger ===
-- Trigger INSERT
CREATE TRIGGER TRG_Reaction_Insert
ON [reaction]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'reaction',
        CAST(i.reaction_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;

END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_Reaction_Update
ON [reaction]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'reaction',
        CAST(i.reaction_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.reaction_id = i.reaction_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.reaction_id = i.reaction_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.reaction_id = d.reaction_id;

END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_Reaction_Delete
ON [reaction]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'reaction',
        CAST(d.reaction_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO