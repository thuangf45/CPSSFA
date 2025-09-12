-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 6.2 voucher
-- ======================
CREATE TYPE [Voucher] AS TABLE 
(
    VoucherId BIGINT NOT NULL PRIMARY KEY,
    VoucherGuid UNIQUEIDENTIFIER NULL,

    Code NVARCHAR(50) NULL,        -- mã voucher duy nhất
    Description NVARCHAR(200) NULL,

    DiscountType NVARCHAR(20) NULL,      -- 'percent' | 'amount'
    DiscountValue DECIMAL(18,2) NULL,    -- giá trị giảm (tùy loại)
    MaxDiscount DECIMAL(18,2) NULL,          -- (tùy chọn) giới hạn số tiền giảm tối đa khi dùng % 

    ValidFrom DATETIME NULL,
    ValidTo DATETIME NULL,
    IsActive BIT NULL,

    MaxUsage INT NULL,                  -- số lượt tối đa (0 = unlimited)
    UsedCount INT NULL,                 -- số lượt đã dùng
    UsedUsers NVARCHAR(MAX) NULL,            -- JSON array: ["user1","user2"]

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL
);