-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 6.5 Cart
-- ======================
CREATE TABLE [cart](
    cart_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    cart_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    cart_details NVARCHAR(MAX) NULL,   -- JSON mảng các item: item_id, quantity, price

    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),

    user_id BIGINT NOT NULL,
    user_guid UNIQUEIDENTIFIER NOT NULL,

    UNIQUE (cart_guid)

);
-- === FK ===
ALTER TABLE [cart]
ADD CONSTRAINT FK_Cart_User_UserId
FOREIGN KEY(user_id) REFERENCES [user](user_id);

ALTER TABLE [cart]
ADD CONSTRAINT FK_Cart_User_UserGuid
FOREIGN KEY(user_guid) REFERENCES [user](user_guid);

-- === Check constraints ===
ALTER TABLE [cart]
ADD CONSTRAINT CK_Cart_Date
CHECK (created_at <= GETDATE() AND updated_at <= GETDATE());

-- === Trigger ===
-- Trigger INSERT
GO
CREATE TRIGGER TRG_Cart_Insert
ON [cart]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT
        'cart',
        CAST(i.cart_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_Cart_Update
ON [cart]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT
        'cart',
        CAST(i.cart_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.cart_id = i.cart_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i2.* FROM inserted i2 WHERE i2.cart_id = i.cart_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.cart_id = d.cart_id;

END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_Cart_Delete
ON [cart]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT
        'cart',
        CAST(d.cart_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO