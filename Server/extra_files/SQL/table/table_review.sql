-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 7 Review
-- ======================
CREATE TABLE [review](
    review_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    review_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    target_type NVARCHAR(50) NOT NULL,          -- 'item', 'shop', … để phân biệt loại target
    rating INT DEFAULT 5,                       -- điểm đánh giá 0-5
    content NVARCHAR(2000) NULL,                -- nội dung review

    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),

    user_guid UNIQUEIDENTIFIER NOT NULL,       -- người viết review
    user_id BIGINT NOT NULL,       
    target_guid UNIQUEIDENTIFIER NOT NULL,        -- đối tượng được review (item, shop, …)
    target_id BIGINT NOT NULL,

    UNIQUE (review_guid)
);

-- === FK ===
ALTER TABLE [review]
ADD CONSTRAINT FK_Review_User_UserId
FOREIGN KEY(user_id) REFERENCES [user](user_id);

ALTER TABLE [review]
ADD CONSTRAINT FK_Review_User_UserGuid
FOREIGN KEY(user_guid) REFERENCES [user](user_guid);

-- === Check constraints ===
ALTER TABLE [review]
ADD CONSTRAINT CK_Review_Rating
CHECK (rating >= 0 AND rating <= 5);

ALTER TABLE [review]
ADD CONSTRAINT CK_Review_TargetType
CHECK (target_type IN ('item','shop'));  -- bổ sung các loại target khác nếu cần

ALTER TABLE [review]
ADD CONSTRAINT CK_Review_Date
CHECK (created_at <= GETDATE() AND updated_at <= GETDATE());

-- === Trigger ===
GO
-- Trigger INSERT
CREATE TRIGGER TRG_Review_Insert
ON [review]
AFTER INSERT
AS
BEGIN
    -- Audit
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT 
        'review',
        CAST(i.review_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_Review_Update
ON [review]
AFTER UPDATE
AS
BEGIN
    -- Audit
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT 
        'review',
        CAST(i.review_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.review_id = i.review_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i2.* FROM inserted i2 WHERE i2.review_id = i.review_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.review_id = d.review_id;

END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_Review_Delete
ON [review]
AFTER DELETE
AS
BEGIN
    -- Audit
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT 
        'review',
        CAST(d.review_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO