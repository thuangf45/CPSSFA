-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 8 File
-- ======================
CREATE TABLE [file](
    file_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    file_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    file_name NVARCHAR(200) NOT NULL,
    file_type VARCHAR(30) NOT NULL,
    file_url NVARCHAR(MAX) NOT NULL,

    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),

    UNIQUE (file_guid)
    
);

-- === Check constraints ===
ALTER TABLE [file]
ADD CONSTRAINT CK_File_URL
CHECK (LEN(file_url) > 0);

ALTER TABLE [file]
ADD CONSTRAINT CK_File_Type
CHECK (file_type IN ('video','image', 'music'));  -- bổ sung các loại target khác nếu cần

ALTER TABLE [file]
ADD CONSTRAINT CK_File_Date
CHECK (created_at <= GETDATE() AND updated_at <= GETDATE());

-- === Trigger ===
GO
-- Trigger INSERT
CREATE TRIGGER TRG_File_Insert
ON [file]
AFTER INSERT
AS
BEGIN
    -- Audit
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT 
        'file',
        CAST(i.file_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_File_Update
ON [file]
AFTER UPDATE
AS
BEGIN
    -- Audit
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT 
        'file',
        CAST(i.file_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.file_id = i.file_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i2.* FROM inserted i2 WHERE i2.file_id = i.file_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.file_id = d.file_id;
END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_File_Delete
ON [file]
AFTER DELETE
AS
BEGIN
    -- Audit
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT 
        'file',
        CAST(d.file_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO