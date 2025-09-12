-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 6.3 Item
-- ======================
CREATE TABLE [item](
    item_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    item_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    item_name NVARCHAR(200) NOT NULL,
    item_description NVARCHAR(1000) NULL,

    avg_rating DECIMAL(3,2) DEFAULT 0,
    number_review INT DEFAULT 0,
    price DECIMAL(18,2) NOT NULL DEFAULT 0,
    stock INT NOT NULL DEFAULT 0,            -- số lượng tồn kho
    is_active BIT DEFAULT 1,                 -- có đang bán hay không

    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),

    shop_id BIGINT NOT NULL,       -- item thuộc shop nào
    shop_guid UNIQUEIDENTIFIER NOT NULL,

    UNIQUE (item_guid)
);

-- === FK ===
ALTER TABLE [item]
ADD CONSTRAINT FK_Item_Shop_ShopId
FOREIGN KEY(shop_id) REFERENCES [shop](shop_id);

ALTER TABLE [item]
ADD CONSTRAINT FK_Item_Shop_ShopGuid
FOREIGN KEY(shop_guid) REFERENCES [shop](shop_guid);

-- === Check constraints ===
ALTER TABLE [item]
ADD CONSTRAINT CK_Item_Positive
CHECK (price >= 0 AND stock >= 0 AND avg_rating >= 0 AND avg_rating <= 5 AND number_review >= 0);

ALTER TABLE [item]
ADD CONSTRAINT CK_Item_Date
CHECK (created_at <= GETDATE() AND updated_at <= GETDATE());

ALTER TABLE [item] 
ADD CONSTRAINT CK_Item_Name_NotEmpty 
CHECK (LEN(item_name) > 0 AND LEN(item_description) > 0);


-- === Trigger ===
GO
-- Trigger INSERT
CREATE TRIGGER TRG_Item_Insert
ON [item]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'item',
        CAST(i.item_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_Item_Update
ON [item]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'item',
        CAST(i.item_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.item_id = i.item_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i2.* FROM inserted i2 WHERE i2.item_id = i.item_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.item_id = d.item_id;

END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_Item_Delete
ON [item]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'item',
        CAST(d.item_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO