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
FOREIGN KEY(account_id) REFERENCES [account](account_id) ON DELETE CASCADE;


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
FOREIGN KEY(account_id) REFERENCES [account](account_id) ON DELETE CASCADE;

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
FOREIGN KEY(account_id) REFERENCES [user](account_id) ON DELETE CASCADE;

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
FOREIGN KEY(account_id) REFERENCES [user](account_id) ON DELETE CASCADE;

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
FOREIGN KEY(parent_id) REFERENCES [post](post_id) ON DELETE NO ACTION;

ALTER TABLE [post]
ADD CONSTRAINT FK_Post_UserSociety
FOREIGN KEY(account_id) REFERENCES [user_society](account_id) ON DELETE CASCADE;

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

    UPDATE us
    SET us.number_post = us.number_post + t.cnt
    FROM user_society us
    JOIN (
        SELECT account_id, COUNT(*) AS cnt
        FROM inserted
        WHERE parent_id IS NULL
        GROUP BY account_id
    ) t ON us.account_id = t.account_id

    UPDATE p
    SET p.number_comment = p.number_comment + t.cnt
    FROM post p
    JOIN (
        SELECT parent_id, COUNT(*) AS cnt
        FROM inserted
        WHERE parent_id IS NOT NULL
        GROUP BY parent_id
    ) t ON p.parent_id= t.parent_id;
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

    -- Cập nhật số lượng post của user
    UPDATE us
    SET us.number_post = us.number_post - t.cnt
    FROM user_society us
    JOIN (
        SELECT account_id, COUNT(*) AS cnt
        FROM deleted
        WHERE parent_id IS NULL
        GROUP BY account_id
    ) t ON us.account_id = t.account_id;

    -- Cập nhật số lượng comment của các post cha còn lại
    UPDATE p
    SET p.number_comment = p.number_comment - t.cnt
    FROM post p
    JOIN (
        SELECT parent_id, COUNT(*) AS cnt
        FROM deleted
        WHERE parent_id IS NOT NULL
        GROUP BY parent_id
    ) t ON p.parent_id= t.parent_id;
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
FOREIGN KEY(post_id) REFERENCES [post](post_id) ON DELETE CASCADE;

ALTER TABLE [reaction]
ADD CONSTRAINT FK_Reaction_UserSociety
FOREIGN KEY(account_id) REFERENCES [user_society](account_id) ON DELETE CASCADE;

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

    UPDATE p
    SET p.number_reaction = p.number_reaction + t.cnt
    FROM post p
    JOIN (
        SELECT post_id, COUNT(*) AS cnt
        FROM inserted
        GROUP BY post_id
    ) t ON p.post_id= t.post_id;
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

    UPDATE p
    SET p.number_reaction = p.number_reaction - t.cnt
    FROM post p
    JOIN (
        SELECT post_id, COUNT(*) AS cnt
        FROM deleted
        GROUP BY post_id
    ) t ON p.post_id= t.post_id;
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
    shop_coin DECIMAL(18,2) DEFAULT 0,
    number_item INT DEFAULT 0,
    number_order INT DEFAULT 0, 
    avg_rating DECIMAL(3,2) DEFAULT 0,
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
FOREIGN KEY(account_id) REFERENCES [user](account_id) ON DELETE CASCADE;

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
FOREIGN KEY(account_id) REFERENCES [user](account_id) ON DELETE NO ACTION;

ALTER TABLE [order]
ADD CONSTRAINT FK_Order_Shop
FOREIGN KEY(shop_id) REFERENCES [shop](shop_id) ON DELETE CASCADE;

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

    -- Nếu đơn hàng đã thanh toán và đã giao -> cộng coin + cộng số đơn cho shop
    UPDATE s
    SET 
        s.shop_coin = s.shop_coin + i.final_amount,
        s.number_order = s.number_order + 1
    FROM shop s
    JOIN inserted i ON s.shop_id = i.shop_id
    WHERE 
        (i.form_shopping = 'online' AND i.payment_status = 'paid' AND i.shipping_status = 'delivered')
        OR
        (i.form_shopping = 'offline' AND i.payment_status = 'paid');
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


    -- Cộng coin + số đơn nếu đơn mới thỏa điều kiện mà trước đó không thỏa
    UPDATE s
    SET 
        s.shop_coin = s.shop_coin + i.final_amount,
        s.number_order = s.number_order + 1
    FROM shop s
    JOIN inserted i ON s.shop_id = i.shop_id
    JOIN deleted d ON i.order_id = d.order_id
    WHERE (
              (i.form_shopping = 'online' AND i.payment_status = 'paid' AND i.shipping_status = 'delivered')
              OR
              (i.form_shopping = 'offline' AND i.payment_status = 'paid')
          )
      AND NOT (
              (d.form_shopping = 'online' AND d.payment_status = 'paid' AND d.shipping_status = 'delivered')
              OR
              (d.form_shopping = 'offline' AND d.payment_status = 'paid')
          );

    -- Trừ coin + số đơn nếu đơn trước đó thỏa mà sau update không thỏa nữa
    UPDATE s
    SET 
        s.shop_coin = s.shop_coin - d.final_amount,
        s.number_order = s.number_order - 1
    FROM shop s
    JOIN deleted d ON s.shop_id = d.shop_id
    JOIN inserted i ON i.order_id = d.order_id
    WHERE (
              (d.form_shopping = 'online' AND d.payment_status = 'paid' AND d.shipping_status = 'delivered')
              OR
              (d.form_shopping = 'offline' AND d.payment_status = 'paid')
          )
      AND NOT (
              (i.form_shopping = 'online' AND i.payment_status = 'paid' AND i.shipping_status = 'delivered')
              OR
              (i.form_shopping = 'offline' AND i.payment_status = 'paid')
          );

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

    -- Trừ coin + số đơn cho shop nếu đơn đã hoàn thành bị xóa
    UPDATE s
    SET 
        s.shop_coin = s.shop_coin - d.final_amount,
        s.number_order = s.number_order - 1
    FROM shop s
    JOIN deleted d ON s.shop_id = d.shop_id
    WHERE 
        (d.form_shopping = 'online' AND d.payment_status = 'paid' AND d.shipping_status = 'delivered')
        OR
        (d.form_shopping = 'offline' AND d.payment_status = 'paid');
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








