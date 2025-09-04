CREATE DATABASE base;
GO

USE base;
GO

CREATE TABLE [data_audit] (
    audit_id BIGINT IDENTITY(1,1),
    table_name NVARCHAR(100),     -- Tên bảng thay đổi
    record_id NVARCHAR(100),      -- Khóa chính của bản ghi
    action_type NVARCHAR(10),     -- INSERT, UPDATE, DELETE
    action_time DATETIME DEFAULT GETDATE(),
    action_by NVARCHAR(100) NULL, -- Người thao tác (nếu có)
    PRIMARY KEY (audit_id),

    -- Lưu toàn bộ dữ liệu dạng JSON (linh hoạt cho mọi bảng)
    data NVARCHAR(MAX)
);

-- ======================
-- 1️ Thực thể gốc: Account
-- ======================
CREATE TABLE [account] (
    account_id UNIQUEIDENTIFIER DEFAULT NEWID(),
    created_at DATETIME DEFAULT GETDATE(),
    role NVARCHAR(50) DEFAULT 'User' NOT NULL,
    PRIMARY KEY (account_id)
);

-- Check constraints
ALTER TABLE [account]
ADD CONSTRAINT CK_Account_CreatedAt CHECK (created_at <= GETDATE());

-- ======================
-- 2️ Account Identity (multi-login)
-- ======================
CREATE TABLE [account_identity] (
    identity_id UNIQUEIDENTIFIER DEFAULT NEWID(),
    account_id UNIQUEIDENTIFIER NOT NULL,
    provider NVARCHAR(50) NOT NULL,        -- local, google, facebook, phone...
    provider_key NVARCHAR(255) NOT NULL,   -- email, OAuth sub_id, phone
    password_hash VARBINARY(256) NULL,     -- chỉ cho provider = local
    created_at DATETIME DEFAULT GETDATE(),
    last_used DATETIME NULL,
    is_verified BIT DEFAULT 0,
    PRIMARY KEY (identity_id)
);


-- === FK ===
ALTER TABLE [account_identity]
ADD CONSTRAINT FK_AccountIdentity_Account_AccountId
FOREIGN KEY(account_id) REFERENCES [account](account_id);


-- === Unique & Check ===
ALTER TABLE [account_identity]
ADD CONSTRAINT UQ_AccountIdentity_ProviderKey UNIQUE(provider, provider_key);

ALTER TABLE [account_identity]
ADD CONSTRAINT CK_AccountIdentity_LocalPassword
CHECK ((provider <> 'local') OR (password_hash IS NOT NULL));

ALTER TABLE [account_identity]
ADD CONSTRAINT CK_AccountIdentity_ProviderKey_NotEmpty
CHECK (LEN(provider_key) > 0);

ALTER TABLE [account_identity]
ADD CONSTRAINT CK_AccountIdentity_LastUsed
CHECK (last_used IS NULL OR last_used <= GETDATE());

-- ======================
-- 3️ User Profile
-- ======================
CREATE TABLE [user] (
    account_id UNIQUEIDENTIFIER NOT NULL,
    full_name NVARCHAR(200) NOT NULL,
    avatar NVARCHAR(500) NULL,  --https://cdn2.fptshop.com.vn/small/avatar_trang_1_cd729c335b.jpg
    bio NVARCHAR(500) NULL,
    user_address NVARCHAR(500) NULL,
    birthday DATE DEFAULT NULL,
    gender NVARCHAR(20) DEFAULT 'Unknown',
    updated_at DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (account_id)
);

-- === FK ===
ALTER TABLE [user]
ADD CONSTRAINT FK_User_Account
FOREIGN KEY(account_id) REFERENCES [account](account_id);

-- === Check constraints ===
ALTER TABLE [user]
ADD CONSTRAINT CK_User_Birthday
CHECK (birthday IS NULL OR (birthday <= GETDATE() AND DATEDIFF(YEAR, birthday, GETDATE()) <= 250));

ALTER TABLE [user]
ADD CONSTRAINT CK_User_Gender
CHECK (gender IN ('Unknown','Male','Female'));

ALTER TABLE [user]
ADD CONSTRAINT CK_User_UpdatedAt
CHECK (updated_at <= GETDATE());

ALTER TABLE [user]
ADD CONSTRAINT CK_User_FullName_NotEmpty
CHECK (LEN(full_name) > 0);

ALTER TABLE [user]
ADD CONSTRAINT CK_User_Avatar_Url
CHECK (avatar IS NULL OR avatar LIKE 'http%');

GO
-- === Trigger ===
-- Trigger INSERT
CREATE TRIGGER TRG_User_Insert
ON [user]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user',
        CAST(i.account_id AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_User_Update
ON [user]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user',
        CAST(i.account_id AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.account_id = i.account_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.account_id = i.account_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.account_id = d.account_id;

    UPDATE u
    SET updated_at = GETDATE()
    FROM [user] u
    JOIN inserted i ON u.account_id = i.account_id;
END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_User_Delete
ON [user]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user',
        CAST(d.account_id AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO

-- ======================
-- 4️ User Bank
-- ======================
CREATE TABLE [user_bank] (
    account_number NVARCHAR(100),
    account_id UNIQUEIDENTIFIER NOT NULL,
    account_amount BIGINT DEFAULT 0,
    currency NVARCHAR(20) DEFAULT 'USD',   -- linh hoạt cho crypto
    updated_at DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (account_number)
);

-- === FK ===
ALTER TABLE [user_bank]
ADD CONSTRAINT FK_UserBank_User
FOREIGN KEY(account_id) REFERENCES [user](account_id);

-- === Check constraints ===
ALTER TABLE [user_bank]
ADD CONSTRAINT CK_UserBank_Amount CHECK (account_amount >= 0);

ALTER TABLE [user_bank]
ADD CONSTRAINT CK_UserBank_Currency
CHECK (currency IS NULL OR LEN(currency) <= 20);

ALTER TABLE [user_bank]
ADD CONSTRAINT CK_UserBank_UpdatedAt
CHECK (updated_at <= GETDATE());


GO
-- === Trigger ===
-- Trigger INSERT
CREATE TRIGGER TRG_UserBank_Insert
ON [user_bank]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user_bank',
        CAST(i.account_number AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_UserBank_Update
ON [user_bank]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user_bank',
        CAST(i.account_number AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.account_id = i.account_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.account_id = i.account_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.account_id = d.account_id;


    UPDATE ub
    SET updated_at = GETDATE()
    FROM user_bank ub
    JOIN inserted i ON ub.account_number = i.account_number;
END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_UserBank_Delete
ON [user_bank]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user_bank',
        CAST(d.account_number AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO


-- ======================
-- 5️ User Society
-- ======================
CREATE TABLE [user_society] (
    account_id UNIQUEIDENTIFIER NOT NULL,
    reputation_score INT DEFAULT 100,
    number_follower INT DEFAULT 0,
    number_following INT DEFAULT 0,
    number_post INT DEFAULT 0,
    number_comment INT DEFAULT 0,
    PRIMARY KEY(account_id)
);

-- === FK ===
ALTER TABLE [user_society]
ADD CONSTRAINT FK_UserSociety_User
FOREIGN KEY(account_id) REFERENCES [user](account_id);

-- === Check constraints ===
ALTER TABLE [user_society]
ADD CONSTRAINT CK_UserSociety_PositiveCounts
CHECK (
    reputation_score >= 0 AND
    number_follower >= 0 AND
    number_following >= 0 AND
    number_post >= 0 AND
    number_comment >= 0
);

GO
-- === Trigger ===
-- Trigger INSERT
CREATE TRIGGER TRG_UserSociety_Insert
ON [user_society]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user_society',
        CAST(i.account_id AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;
END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_UserSociety_Update
ON [user_society]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user_society',
        CAST(i.account_id AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.account_id = i.account_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.account_id = i.account_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.account_id = d.account_id;
END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_UserSociety_Delete
ON [user_society]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'user_society',
        CAST(d.account_id AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO


-- ======================
-- 5.1 Post
-- ======================
CREATE TABLE [post](
    post_id UNIQUEIDENTIFIER DEFAULT NEWID(),
    parent_id UNIQUEIDENTIFIER NULL,     -- NULL = post gốc, không NULL = comment
    content NVARCHAR(1000),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    number_comment INT DEFAULT 0,
    number_reaction INT DEFAULT 0,
    account_id UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY(post_id)
);

-- === FK ===
ALTER TABLE [post]
ADD CONSTRAINT FK_Post_Post
FOREIGN KEY(parent_id) REFERENCES [post](post_id);

ALTER TABLE [post]
ADD CONSTRAINT FK_Post_UserSociety
FOREIGN KEY(account_id) REFERENCES [user_society](account_id);

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
    parent_id IS NULL OR
    parent_id != post_id
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
        CAST(i.post_id AS NVARCHAR(100)),
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
        CAST(i.post_id AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.post_id = i.post_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.post_id = i.post_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.post_id = d.post_id;

    
    UPDATE p
    SET updated_at = GETDATE()
    FROM post p
    JOIN inserted i ON p.post_id = i.post_id;
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
        CAST(d.post_id AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;

    -- Xoá comment con (nếu có) theo post bị xoá
    DELETE p
    FROM post p
    JOIN deleted d ON p.parent_id = d.post_id;
END;
GO


-- ======================
-- 5.2 Reaction
-- ======================
CREATE TABLE [reaction](
    reaction_id UNIQUEIDENTIFIER DEFAULT NEWID(),
    reaction_type NVARCHAR(20),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    post_id UNIQUEIDENTIFIER NOT NULL,
    account_id UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY(reaction_id)
);

-- === FK ===
ALTER TABLE [reaction]
ADD CONSTRAINT FK_Reaction_Post
FOREIGN KEY(post_id) REFERENCES [post](post_id);

ALTER TABLE [reaction]
ADD CONSTRAINT FK_Reaction_UserSociety
FOREIGN KEY(account_id) REFERENCES [user_society](account_id);

-- === Check constraints ===
ALTER TABLE [reaction]
ADD CONSTRAINT CK_Reaction_Date
CHECK (
    created_at <= GETDATE() AND
    updated_at <= GETDATE()
);

ALTER TABLE [reaction]
ADD CONSTRAINT CK_Reaction_Type
CHECK (
    reaction_type IN ('like', 'heart', 'haha', 'huhu')
);

GO
-- === Trigger ===
-- Trigger INSERT
CREATE TRIGGER TRG_Reaction_Insert
ON [reaction]
AFTER INSERT
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'reaction',
        CAST(i.reaction_id AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;

END;
GO

-- Trigger UPDATE
CREATE TRIGGER TRG_Reaction_Update
ON [reaction]
AFTER UPDATE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'reaction',
        CAST(i.reaction_id AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.reaction_id = i.reaction_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.reaction_id = i.reaction_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.reaction_id = d.reaction_id;

    UPDATE r
    SET updated_at = GETDATE()
    FROM reaction r
    JOIN inserted i ON r.reaction_id = i.reaction_id;

END;
GO

-- Trigger DELETE
CREATE TRIGGER TRG_Reaction_Delete
ON [reaction]
AFTER DELETE
AS
BEGIN
    INSERT INTO data_audit (table_name, record_id, action_type, data)
    SELECT 
        'reaction',
        CAST(d.reaction_id AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO



-- ======================
-- 6 User Shop
-- ======================
CREATE TABLE [shop](
    shop_id UNIQUEIDENTIFIER DEFAULT NEWID(),
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
    account_id UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY(shop_id)
);

-- === FK ===
ALTER TABLE [shop]
ADD CONSTRAINT FK_Shop_User
FOREIGN KEY(account_id) REFERENCES [user](account_id);

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
        CAST(i.shop_id AS NVARCHAR(100)),
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
        CAST(i.shop_id AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.shop_id = i.shop_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.shop_id = i.shop_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.shop_id = d.shop_id;

    -- Auto update updated_at
    UPDATE s
    SET updated_at = GETDATE()
    FROM shop s
    JOIN inserted i ON s.shop_id = i.shop_id;

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
        CAST(d.shop_id AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO

-- ======================
-- 6.1 Order
-- ======================
CREATE TABLE [order](
    order_id UNIQUEIDENTIFIER DEFAULT NEWID(),
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

    shop_id UNIQUEIDENTIFIER NOT NULL,
    account_id UNIQUEIDENTIFIER NULL,
    PRIMARY KEY(order_id)
);

-- === FK ===
ALTER TABLE [order]
ADD CONSTRAINT FK_Order_User
FOREIGN KEY(account_id) REFERENCES [user](account_id);

ALTER TABLE [order]
ADD CONSTRAINT FK_Order_Shop
FOREIGN KEY(shop_id) REFERENCES [shop](shop_id);

-- === Check constraints ===
ALTER TABLE [order]
ADD CONSTRAINT CK_Order_Form
CHECK (
    (form_shopping IN ('offline', 'online')) 
    AND
    (form_shopping = 'offline' AND
    account_id IS NULL AND
    shoppers_address IS NULL AND
    shoppers_phone_number IS NULL AND
    shipping_status IS NULL AND
    payment_status IN ('paid')) 
    OR
    (form_shopping = 'online' AND
    account_id IS NOT NULL AND
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
        CAST(i.order_id AS NVARCHAR(100)),
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
        CAST(i.order_id AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.order_id = i.order_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i.* FROM inserted i2 WHERE i2.order_id = i.order_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.order_id = d.order_id;

    -- Auto update updated_at
    UPDATE o
    SET updated_at = GETDATE()
    FROM [order] o
    JOIN inserted i ON o.order_id = i.order_id;
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
        CAST(d.order_id AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO


-- ======================
-- 6.2 voucher
-- ======================
CREATE TABLE [voucher] (
    voucher_id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
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
        CAST(i.voucher_id AS NVARCHAR(100)),
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
        CAST(i.voucher_id AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT
                (SELECT d.* FROM deleted d WHERE d.voucher_id = i.voucher_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i2.* FROM inserted i2 WHERE i2.voucher_id = i.voucher_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.voucher_id = d.voucher_id;

    -- Auto update updated_at
    UPDATE v
    SET updated_at = GETDATE()
    FROM [voucher] v
    JOIN inserted i ON v.voucher_id = i.voucher_id;
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
        CAST(d.voucher_id AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO

-- ======================
-- 6.3 Item
-- ======================
CREATE TABLE [item](
    item_id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    item_name NVARCHAR(200) NOT NULL,
    item_description NVARCHAR(1000) NULL,
    avg_rating DECIMAL(3,2) DEFAULT 0,
    number_review INT DEFAULT 0,
    price DECIMAL(18,2) NOT NULL DEFAULT 0,
    stock INT NOT NULL DEFAULT 0,            -- số lượng tồn kho
    shop_id UNIQUEIDENTIFIER NOT NULL,       -- item thuộc shop nào
    is_active BIT DEFAULT 1,                 -- có đang bán hay không
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);

-- === FK ===
ALTER TABLE [item]
ADD CONSTRAINT FK_Item_Shop
FOREIGN KEY(shop_id) REFERENCES [shop](shop_id);

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
        CAST(i.item_id AS NVARCHAR(100)),
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
        CAST(i.item_id AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.item_id = i.item_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i2.* FROM inserted i2 WHERE i2.item_id = i.item_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.item_id = d.item_id;

    -- Auto update updated_at
    UPDATE it
    SET updated_at = GETDATE()
    FROM item it
    JOIN inserted i ON it.item_id = i.item_id;
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
        CAST(d.item_id AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO


-- ======================
-- 6.4 Completed orders
-- ======================
CREATE TABLE [completed_orders](
    completed_orders_id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    account_id UNIQUEIDENTIFIER NOT NULL,
    orders_details NVARCHAR(MAX) NULL,        -- JSON mảng các order
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);

-- === FK ===
ALTER TABLE [completed_orders]
ADD CONSTRAINT FK_CompletedOrders_User
FOREIGN KEY(account_id) REFERENCES [user](account_id);

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
        CAST(i.completed_orders_id AS NVARCHAR(100)),
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
        CAST(i.completed_orders_id AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.completed_orders_id = i.completed_orders_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i2.* FROM inserted i2 WHERE i2.completed_orders_id = i.completed_orders_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.completed_orders_id = d.completed_orders_id;

    UPDATE co
    SET updated_at = GETDATE()
    FROM completed_orders co
    JOIN inserted i ON co.completed_orders_id = i.completed_orders_id;
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
        CAST(d.completed_orders_id AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO

-- ======================
-- 6.5 Review
-- ======================
CREATE TABLE [cart](
    cart_id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    account_id UNIQUEIDENTIFIER NOT NULL,
    cart_details NVARCHAR(MAX) NULL,   -- JSON mảng các item: item_id, quantity, price
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);
-- === FK ===
ALTER TABLE [cart]
ADD CONSTRAINT FK_Cart_User
FOREIGN KEY(account_id) REFERENCES [user](account_id);

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
        CAST(i.cart_id AS NVARCHAR(100)),
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
        CAST(i.cart_id AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.cart_id = i.cart_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i2.* FROM inserted i2 WHERE i2.cart_id = i.cart_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.cart_id = d.cart_id;

    UPDATE c
    SET updated_at = GETDATE()
    FROM cart c
    JOIN inserted i ON c.cart_id = i.cart_id;
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
        CAST(d.cart_id AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO


-- ======================
-- 7 Review
-- ======================
CREATE TABLE [review](
    review_id UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY,
    account_id UNIQUEIDENTIFIER NOT NULL,       -- người viết review
    target_id UNIQUEIDENTIFIER NOT NULL,        -- đối tượng được review (item, shop, …)
    target_type NVARCHAR(50) NOT NULL,          -- 'item', 'shop', … để phân biệt loại target
    rating INT DEFAULT 5,                       -- điểm đánh giá 0-5
    content NVARCHAR(2000) NULL,                -- nội dung review
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);

-- === FK ===
ALTER TABLE [review]
ADD CONSTRAINT FK_Review_User
FOREIGN KEY(account_id) REFERENCES [user](account_id);

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
        CAST(i.review_id AS NVARCHAR(100)),
        'INSERT',
        (SELECT i.* FROM inserted i FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM inserted i;

    -- Auto update updated_at
    UPDATE r
    SET updated_at = GETDATE()
    FROM review r
    JOIN inserted i ON r.review_id = i.review_id;
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
        CAST(i.review_id AS NVARCHAR(100)),
        'UPDATE',
        (
            SELECT 
                (SELECT d.* FROM deleted d WHERE d.review_id = i.review_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS OldData,
                (SELECT i2.* FROM inserted i2 WHERE i2.review_id = i.review_id FOR JSON PATH, WITHOUT_ARRAY_WRAPPER) AS NewData
            FOR JSON PATH, WITHOUT_ARRAY_WRAPPER
        )
    FROM inserted i
    JOIN deleted d ON i.review_id = d.review_id;

    -- Auto update updated_at
    UPDATE r
    SET updated_at = GETDATE()
    FROM review r
    JOIN inserted i ON r.review_id = i.review_id;
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
        CAST(d.review_id AS NVARCHAR(100)),
        'DELETE',
        (SELECT d.* FROM deleted d FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)
    FROM deleted d;
END;
GO




