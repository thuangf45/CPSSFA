-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 6 User Shop
-- ======================
CREATE TABLE [shop](
    shop_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    shop_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    shop_name NVARCHAR(100) NOT NULL,
    shop_description NVARCHAR(1000) NULL,
    shop_address NVARCHAR(500) NULL,
    phone_number NVARCHAR(20) NULL,
    email NVARCHAR(200) NULL,
    avg_rating DECIMAL(3,2) DEFAULT 0,

    shop_coin DECIMAL(18,2) DEFAULT 0,
    number_item INT DEFAULT 0,
    number_order INT DEFAULT 0, 
    number_review INT DEFAULT 0,
    is_active BIT DEFAULT 1,

    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),

    user_id BIGINT NOT NULL,
    user_guid UNIQUEIDENTIFIER NOT NULL,
    
    UNIQUE (shop_guid)
);

-- === FK ===
ALTER TABLE [shop]
ADD CONSTRAINT FK_Shop_User_UserId
FOREIGN KEY(user_id) REFERENCES [user](user_id);

ALTER TABLE [shop]
ADD CONSTRAINT FK_Shop_User_UserGuid
FOREIGN KEY(user_guid) REFERENCES [user](user_guid);

-- === Check constraints ===
ALTER TABLE [shop]
ADD CONSTRAINT CK_Shop_Rating
CHECK (
    avg_rating >= 0 AND
    avg_rating <= 5
);

ALTER TABLE [shop]
ADD CONSTRAINT CK_Shop_Date
CHECK (
    created_at <= GETDATE() AND
    updated_at <= GETDATE()
);

ALTER TABLE [shop]
ADD CONSTRAINT CK_Shop_PositiveCounts
CHECK (
    number_item >= 0 AND
    number_order >= 0 AND
    number_review >= 0 AND
    shop_coin >= 0
);

GO
-- === Trigger ===
-- Trigger INSERT
CREATE TRIGGER TRG_Shop_Insert
ON [shop]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'shop',
        CAST(i.shop_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_Shop_Update
ON [shop]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'shop',
        CAST(i.shop_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.shop_id = i.shop_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.shop_id = i.shop_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.shop_id = d.shop_id;
END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_Shop_Delete
ON [shop]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'shop',
        CAST(d.shop_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO