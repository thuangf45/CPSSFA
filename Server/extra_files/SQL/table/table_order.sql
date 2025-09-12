-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 6.1 Order
-- ======================
CREATE TABLE [order](
    order_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    order_guid UNIQUEIDENTIFIER DEFAULT NEWID(),

    order_name NVARCHAR(100) NULL,
    order_description NVARCHAR(1000) NULL,
    order_details NVARCHAR(MAX) NULL,        

    shop_address NVARCHAR(500) NOT NULL, 
    shoppers_address NVARCHAR(20) NULL,
    shoppers_phone_number VARCHAR(20) NULL,

    form_shopping NVARCHAR(20) DEFAULT 'offline',
    payment_status NVARCHAR(20) DEFAULT 'paid',
    shipping_status NVARCHAR(30) NULL,
    payment_method NVARCHAR(20) DEFAULT 'cash in person',

    total_amount DECIMAL(18,2) DEFAULT 0,       -- tổng tiền hàng
    discount_amount DECIMAL(18,2) DEFAULT 0,    -- số tiền giảm
    final_amount AS (total_amount - discount_amount) PERSISTED, -- số tiền phải trả

    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),

    shop_id BIGINT NOT NULL,
    shop_guid UNIQUEIDENTIFIER NOT NULL,
    user_id BIGINT NULL,
    user_guid UNIQUEIDENTIFIER NULL,
    
    UNIQUE (order_guid)
);

-- === FK ===
ALTER TABLE [order]
ADD CONSTRAINT FK_Order_User_UserId
FOREIGN KEY(user_id) REFERENCES [user](user_id);

ALTER TABLE [order]
ADD CONSTRAINT FK_Order_User_UserGuid
FOREIGN KEY(user_guid) REFERENCES [user](user_guid);

ALTER TABLE [order]
ADD CONSTRAINT FK_Order_Shop_ShopId
FOREIGN KEY(shop_id) REFERENCES [shop](shop_id);

ALTER TABLE [order]
ADD CONSTRAINT FK_Order_Shop_ShopGuid
FOREIGN KEY(shop_guid) REFERENCES [shop](shop_guid);

-- === Check constraints ===
ALTER TABLE [order]
ADD CONSTRAINT CK_Order_Form
CHECK (
    (form_shopping IN ('offline', 'online')) 
    AND
    (form_shopping = 'offline' AND
    user_id IS NULL AND
    user_guid IS NULL AND
    shoppers_address IS NULL AND
    shoppers_phone_number IS NULL AND
    shipping_status IS NULL AND
    payment_status IN ('paid')) 
    OR
    (form_shopping = 'online' AND
    user_id IS NOT NULL AND
    user_guid IS NOT NULL AND
    shoppers_address IS NOT NULL AND
    shoppers_phone_number IS NOT NULL AND
    shipping_status IS NOT NULL AND
    (
        (
            payment_status IN ('unpaid','paid', 'partially paid') AND
            shipping_status IN ('pending', 'processing', 'shipped', 'in transit', 'out for delivery', 'delivered')
        )
        OR (
            payment_status = 'refunded' AND shipping_status = 'cancelled'
        )
        OR (
            payment_status = 'paid' AND shipping_status = 'returned'
        )
        OR (
            payment_status = 'disputed' AND shipping_status = 'on hold'
        )
    ))
);
ALTER TABLE [order]
ADD CONSTRAINT CK_Order_PaymentMethod
CHECK (
    payment_method IN (
        'cash in person',     -- trả trực tiếp
        'cash on delivery',   -- COD
        'bank transfer',      -- chuyển khoản
        'credit card',        -- thẻ tín dụng
        'debit card',         -- thẻ ghi nợ
        'e-wallet',           -- ví điện tử
        'paypal',             -- thanh toán PayPal
        'voucher'             -- phiếu quà tặng
    )
);

ALTER TABLE [order]
ADD CONSTRAINT CK_Order_PositiveCounts
CHECK (
    total_amount >= 0 AND
    discount_amount >= 0 AND
    final_amount >= 0 AND
    LEN(order_details) > 0
);

ALTER TABLE [order]
ADD CONSTRAINT CK_Order_Date
CHECK (
    created_at <= GETDATE() AND
    updated_at <= GETDATE()
);

GO
-- === Trigger ===
-- Trigger INSERT
CREATE TRIGGER TRG_Order_Insert
ON [order]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'order',
        CAST(i.order_guid AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_Order_Update
ON [order]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'order',
        CAST(i.order_guid AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.order_id = i.order_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.order_id = i.order_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.order_id = d.order_id;

END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_Order_Delete
ON [order]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'order',
        CAST(d.order_guid AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO