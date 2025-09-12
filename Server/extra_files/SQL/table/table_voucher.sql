-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 6.2 voucher
-- ======================
CREATE TABLE [voucher] (
    voucher_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    voucher_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    code NVARCHAR(50) UNIQUE NOT NULL,        -- mã voucher duy nhất
    description NVARCHAR(200) NULL,

    discount_type NVARCHAR(20) NOT NULL,      -- 'percent' | 'amount'
    discount_value DECIMAL(18,2) NOT NULL,    -- giá trị giảm (tùy loại)
    max_discount DECIMAL(18,2) NULL,          -- (tùy chọn) giới hạn số tiền giảm tối đa khi dùng % 

    valid_from DATETIME NOT NULL,
    valid_to DATETIME NOT NULL,
    is_active BIT DEFAULT 1,

    max_usage INT DEFAULT 1,                  -- số lượt tối đa (0 = unlimited)
    used_count INT DEFAULT 0,                 -- số lượt đã dùng
    used_users NVARCHAR(MAX) NULL,            -- JSON array: ["user1","user2"]

    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);

-- Check constraint
ALTER TABLE [voucher]
ADD CONSTRAINT CK_Voucher_Discount
CHECK (
    (discount_type = 'amount' AND discount_value >= 0) OR
    (discount_type = 'percent' AND discount_value > 0 AND discount_value <= 100 AND (max_discount IS NULL OR max_discount >= 0))
);

ALTER TABLE [voucher]
ADD CONSTRAINT CK_Voucher_Usage
CHECK (max_usage >= 0 AND used_count >= 0 AND (max_usage = 0 OR used_count <= max_usage));

ALTER TABLE [voucher]
ADD CONSTRAINT CK_Voucher_Date
CHECK (
    valid_from <= valid_to AND 
    created_at <= GETDATE() AND
    updated_at <= GETDATE()
);

-- === Trigger ===
GO
-- Trigger INSERT
CREATE TRIGGER TRG_Voucher_Insert
ON [voucher]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT
        'voucher',
        CAST(i.voucher_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_Voucher_Update
ON [voucher]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT
        'voucher',
        CAST(i.voucher_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT
                (SELECT d.* FROM deleted d WHERE d.voucher_id = i.voucher_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i2.* FROM inserted i2 WHERE i2.voucher_id = i.voucher_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.voucher_id = d.voucher_id;

END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_Voucher_Delete
ON [voucher]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT
        'voucher',
        CAST(d.voucher_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO