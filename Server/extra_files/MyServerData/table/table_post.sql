-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 5.1 Post
-- ======================
CREATE TABLE [post](
    post_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    post_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    content NVARCHAR(1000),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    number_comment INT DEFAULT 0,
    number_reaction INT DEFAULT 0,

    parent_id BIGINT NULL,     -- NULL = post gốc, không NULL = comment
    parent_guid UNIQUEIDENTIFIER NULL,     -- NULL = post gốc, không NULL = comment
    user_society_id BIGINT NOT NULL,
    user_society_guid UNIQUEIDENTIFIER NOT NULL,

    UNIQUE (post_guid)
);

-- === FK ===
ALTER TABLE [post]
ADD CONSTRAINT FK_Post_Post_ParentId
FOREIGN KEY(parent_id) REFERENCES [post](post_id);

ALTER TABLE [post]
ADD CONSTRAINT FK_Post_Post_ParentGuid
FOREIGN KEY(parent_guid) REFERENCES [post](post_guid);

ALTER TABLE [post]
ADD CONSTRAINT FK_Post_UserSociety_UserSocietyId
FOREIGN KEY(user_society_id) REFERENCES [user_society](user_society_id);

ALTER TABLE [post]
ADD CONSTRAINT FK_Post_UserSociety_UserSocietyGuid
FOREIGN KEY(user_society_guid) REFERENCES [user_society](user_society_guid);

-- === Check constraints ===
ALTER TABLE [post]
ADD CONSTRAINT CK_Post_Date
CHECK (
    created_at <= GETDATE() AND
    updated_at <= GETDATE()
);

ALTER TABLE [post]
ADD CONSTRAINT CK_Post_PositiveCounts
CHECK (
    number_comment >= 0 AND
    number_reaction >= 0
);

ALTER TABLE [post]
ADD CONSTRAINT CK_Post_Parent
CHECK (
    (parent_id IS NULL AND parent_guid IS NULL)
    OR
    (parent_id != post_id AND parent_guid != post_guid)
);

GO
-- === Trigger ===
-- Trigger INSERT
CREATE TRIGGER TRG_Post_Insert
ON [post]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'post',
        CAST(i.post_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_Post_Update
ON [post]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'post',
        CAST(i.post_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.post_id = i.post_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.post_id = i.post_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.post_id = d.post_id;

END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_Post_Delete
ON [post]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'post',
        CAST(d.post_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO