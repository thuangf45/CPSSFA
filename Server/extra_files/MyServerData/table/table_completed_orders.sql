-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 6.4 Completed orders
-- ======================
CREATE TABLE [completed_orders](
    completed_orders_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    completed_orders_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    orders_details NVARCHAR(MAX) NULL,        -- JSON mảng các order

    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),

    user_id BIGINT NOT NULL,
    user_guid UNIQUEIDENTIFIER NOT NULL,

    UNIQUE (completed_orders_guid)

);

-- === FK ===
ALTER TABLE [completed_orders]
ADD CONSTRAINT FK_CompletedOrders_User_UserId
FOREIGN KEY(user_id) REFERENCES [user](user_id);

ALTER TABLE [completed_orders]
ADD CONSTRAINT FK_CompletedOrders_User_UserGuid
FOREIGN KEY(user_guid) REFERENCES [user](user_guid);

-- === Check constraints ===
ALTER TABLE [completed_orders]
ADD CONSTRAINT CK_CompletedOrders_Date
CHECK (created_at <= GETDATE() AND updated_at <= GETDATE());

-- === Trigger ===
-- Trigger INSERT
GO
CREATE TRIGGER TRG_CompletedOrders_Insert
ON [completed_orders]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT
        'completed_orders',
        CAST(i.completed_orders_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_CompletedOrders_Update
ON [completed_orders]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT
        'completed_orders',
        CAST(i.completed_orders_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.completed_orders_id = i.completed_orders_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i2.* FROM inserted i2 WHERE i2.completed_orders_id = i.completed_orders_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.completed_orders_id = d.completed_orders_id;

END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_CompletedOrders_Delete
ON [completed_orders]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit(table_name, record_id, action_type, data)
    SELECT
        'completed_orders',
        CAST(d.completed_orders_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO