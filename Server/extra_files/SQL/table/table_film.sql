-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 9 Film
-- ======================
CREATE TABLE [film](
    film_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    film_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    film_name NVARCHAR(200) NOT NULL,
    film_description NVARCHAR(1000) NOT NULL,
    film_cost DECIMAL(18,2) DEFAULT 0,

    avg_rating DECIMAL(3,2) DEFAULT 0,
    number_review INT DEFAULT 0,
    number_view INT DEFAULT 0,

    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),

    file_id BIGINT NOT NULL,
    file_guid UNIQUEIDENTIFIER NOT NULL,
    user_guid UNIQUEIDENTIFIER NOT NULL,
    user_id BIGINT NOT NULL,

    UNIQUE (film_guid)

);

-- === FK ===
ALTER TABLE [film]
ADD CONSTRAINT FK_Film_User_UserId
FOREIGN KEY(user_id) REFERENCES [user](user_id);

ALTER TABLE [film]
ADD CONSTRAINT FK_Film_User_UserGuid
FOREIGN KEY(user_guid) REFERENCES [user](user_guid);

ALTER TABLE [film]
ADD CONSTRAINT FK_Film_File_FileId
FOREIGN KEY(file_id) REFERENCES [file](file_id);

ALTER TABLE [film]
ADD CONSTRAINT FK_Film_File_FileGuid
FOREIGN KEY(file_guid) REFERENCES [file](file_guid);

-- === Check constraints ===
ALTER TABLE [film]
ADD CONSTRAINT CK_Film_Rating
CHECK (    
    avg_rating >= 0 AND
    avg_rating <= 5
);

ALTER TABLE [film]
ADD CONSTRAINT CK_Film_Positive
CHECK (

    number_view >= 0 AND
    number_review >= 0
); 

ALTER TABLE [film]
ADD CONSTRAINT CK_Film_Date
CHECK (created_at <= GETDATE() AND updated_at <= GETDATE());

-- === Trigger ===
GO
-- Trigger INSERT
CREATE TRIGGER TRG_Film_Insert
ON [film]
AFTER INSERT
AS
BEGIN
    -- Audit
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT 
        'film',
        CAST(i.film_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;

END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_Film_Update
ON [film]
AFTER UPDATE
AS
BEGIN
    -- Audit
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT 
        'film',
        CAST(i.film_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.film_id = i.film_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i2.* FROM inserted i2 WHERE i2.film_id = i.film_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.film_id = d.film_id;
END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_Film_Delete
ON [film]
AFTER DELETE
AS
BEGIN
    -- Audit
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT 
        'film',
        CAST(d.film_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO