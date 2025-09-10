-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 1️ Account
-- ======================
CREATE TYPE [Account] AS TABLE
(
    AccountId BIGINT NULL,
    AccountGuid UNIQUEIDENTIFIER NULL,
    Role NVARCHAR(50) NULL,
    CreatedAt DATETIME NULL
);

-- ======================
-- 2️ Account Identity (multi-login)
-- ======================
CREATE TYPE [AccountIdentity] AS TABLE
(
    IdentityId BIGINT NULL,  -- ID tự tăng
    IdentityGuid UNIQUEIDENTIFIER NULL,

    Provider NVARCHAR(50) NULL,        -- local, google, facebook, phone...
    ProviderKey NVARCHAR(255) NULL,   -- email, OAuth sub_id, phone
    PasswordHash VARBINARY(256) NULL,     -- chỉ cho provider = local
    CreatedAt DATETIME NULL,
    LastUsed DATETIME NULL,
    IsVerified BIT NULL,

    AccountId BIGINT NULL,
    AccountGuid UNIQUEIDENTIFIER NULL
);

-- ======================
-- 3️ User Profile
-- ======================
CREATE TYPE [User] AS TABLE 
(
    UserId BIGINT NULL,
    UserGuid UNIQUEIDENTIFIER NULL,

    FullName NVARCHAR(200) NULL,
    Avatar NVARCHAR(500) NULL,
    Bio NVARCHAR(500) NULL,
    UserAddress NVARCHAR(500) NULL,
    Birthday DATE NULL,
    Gender NVARCHAR(20) NULL,
    UpdatedAt DATETIME NULL,

    AccountId BIGINT NULL,
    AccountGuid UNIQUEIDENTIFIER NULL
);

-- ======================
-- 4️ User Bank
-- ======================
CREATE TYPE [UserBank] AS TABLE
(
    UserBankId BIGINT NULL,
    AccountNumber NVARCHAR(100) NULL,

    AccountAmount BIGINT NULL,
    Currency NVARCHAR(20) NULL,   -- linh hoạt cho crypto
    UpdatedAt DATETIME NULL,
    
    UserGuid UNIQUEIDENTIFIER NULL,
    UserId BIGINT NULL
);

-- ======================
-- 5️ User Society
-- ======================
CREATE TYPE [UserSociety] AS TABLE
(
    UserSocietyId BIGINT NULL,
    UserSocietyGuid UNIQUEIDENTIFIER NULL,

    ReputationScore INT NULL,
    NumberFollower INT NULL,
    NumberFollowing INT NULL,
    NumberPost INT NULL,
    NumberComment INT NULL,

    UserGuid UNIQUEIDENTIFIER NULL,
    UserId BIGINT NULL
);

-- ======================
-- 5.1 Post
-- ======================
CREATE Type [Post] AS TABLE
(
    PostId BIGINT NULL,
    PostGuid UNIQUEIDENTIFIER NULL,

    Content NVARCHAR(1000) NULL,
    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,
    NumberComment INT NULL,
    NumberReaction INT NULL,

    ParentId BIGINT NULL,     -- NULL = post gốc, không NULL = comment
    ParentGuid UNIQUEIDENTIFIER NULL,     -- NULL = post gốc, không NULL = comment
    UserSocietyId BIGINT NULL,
    UserSocietyGuid UNIQUEIDENTIFIER NULL
);

-- ======================
-- 5.2 Reaction
-- ======================
CREATE Type [Reaction] AS TABLE(
    ReactionId BIGINT NULL,
    ReactionGuid UNIQUEIDENTIFIER NULL,

    ReactionType NVARCHAR(20) NULL,
    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    PostId BIGINT NULL,
    PostGuid UNIQUEIDENTIFIER NULL,
    UserSocietyId BIGINT NULL,
    UserSocietyGuid UNIQUEIDENTIFIER NULL
);



-- ======================
-- 6 User Shop
-- ======================
CREATE TYPE [Shop] AS TABLE
(
    ShopId BIGINT NULL,
    ShopGuid UNIQUEIDENTIFIER NULL,

    ShopName NVARCHAR(100) NULL,
    ShopDescription NVARCHAR(1000) NULL,
    ShopAddress NVARCHAR(500) NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Email NVARCHAR(200) NULL,
    AvgRating DECIMAL(3,2) NULL,

    ShopCoin DECIMAL(18,2) NULL,
    NumberItem INT NULL,
    NumberOrder INT NULL, 
    NumberTeview INT NULL,
    IsActive BIT NULL,

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    UserId BIGINT NULL,
    UserGuid UNIQUEIDENTIFIER NULL
);


-- ======================
-- 6.1 Order
-- ======================
CREATE TYPE [Order] AS TABLE
(
    OrderId BIGINT NULL,
    OrderGuid UNIQUEIDENTIFIER NULL,

    OrderName NVARCHAR(100) NULL,
    OrderDescription NVARCHAR(1000) NULL,
    OrderDetails NVARCHAR(MAX) NULL,        

    ShopAddress NVARCHAR(500) NULL, 
    ShoppersAddress NVARCHAR(20) NULL,
    ShoppersPhoneNumber VARCHAR(20) NULL,

    FormShopping NVARCHAR(20) NULL,
    PaymentStatus NVARCHAR(20) NULL,
    ShippingStatus NVARCHAR(30) NULL,
    PaymentMethod NVARCHAR(20) NULL,

    TotalAmount DECIMAL(18,2) NULL,       -- tổng tiền hàng
    DiscountAmount DECIMAL(18,2) NULL,    -- số tiền giảm
    FinalAmount DECIMAL(18,2) NULL, -- số tiền phải trả

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    ShopId BIGINT NULL,
    ShopGuid UNIQUEIDENTIFIER NULL,
    UserId BIGINT NULL,
    UserGuid UNIQUEIDENTIFIER NULL
);


-- ======================
-- 6.2 voucher
-- ======================
CREATE TYPE [Voucher] AS TABLE 
(
    VoucherId BIGINT NULL,
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

-- ======================
-- 6.3 Item
-- ======================
CREATE TYPE [Item] AS TABLE
(
    ItemId BIGINT NULL,
    ItemGuid UNIQUEIDENTIFIER NULL,

    ItemName NVARCHAR(200) NULL,
    ItemDescription NVARCHAR(1000) NULL,

    AvgRating DECIMAL(3,2) NULL,
    NumberReview INT NULL,
    Price DECIMAL(18,2) NULL,
    Stock INT NULL,            -- số lượng tồn kho
    IsActive BIT NULL,                 -- có đang bán hay không

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    ShopId BIGINT NULL,       -- item thuộc shop nào
    ShopGuid UNIQUEIDENTIFIER NULL
);

-- ======================
-- 6.4 Completed orders
-- ======================
CREATE TYPE [CompletedOrders] AS TABLE
(
    CompletedOrdersId BIGINT NULL,
    CompletedOrdersGuid UNIQUEIDENTIFIER NULL,

    OrdersDetails NVARCHAR(MAX) NULL,        -- JSON mảng các order

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    UserId BIGINT NULL,
    UserGuid UNIQUEIDENTIFIER NULL

);

-- ======================
-- 6.5 Review
-- ======================
CREATE TYPE [Cart] AS TABLE
(
    CartId BIGINT NULL,
    CartGuid UNIQUEIDENTIFIER NULL,

    CartDetails NVARCHAR(MAX) NULL,   -- JSON mảng các item: item_id, quantity, price

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    UserId BIGINT NULL,
    UserGuid UNIQUEIDENTIFIER NULL

);

-- ======================
-- 7 Review
-- ======================
CREATE TYPE [Review] AS TABLE
(
    ReviewId BIGINT NULL,
    ReviewGuid UNIQUEIDENTIFIER NULL,

    TargetType NVARCHAR(50) NULL,          -- 'item', 'shop', … để phân biệt loại target
    Rating INT DEFAULT NULL,                       -- điểm đánh giá 0-5
    Content NVARCHAR(2000) NULL,                -- nội dung review

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    UserGuid UNIQUEIDENTIFIER NULL,       -- người viết review
    UserId BIGINT NULL,       
    TargetGuid UNIQUEIDENTIFIER NULL,        -- đối tượng được review (item, shop, …)
    TargetId BIGINT NULL

);

-- ======================
-- 8 File
-- ======================
CREATE TYPE [File] AS TABLE
(
    FileID BIGINT NULL,
    FileGuid UNIQUEIDENTIFIER NULL,

    FileName NVARCHAR(200) NULL,
    FileType VARCHAR(30) NULL,
    FileUrl NVARCHAR(MAX) NULL,

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL
    
);

-- ======================
-- 9 Film
-- ======================
CREATE TYPE [Film] AS TABLE(
    FilmId BIGINT NULL,
    FilmGuid UNIQUEIDENTIFIER NULL,

    FilName NVARCHAR(200) NULL,
    FilmDescription NVARCHAR(1000) NULL,
    FilmCost DECIMAL(18,2) NULL,

    AvgRating DECIMAL(3,2) NULL,
    NumberReview INT NULL,
    NumberView INT NULL,

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    FileId BIGINT NULL,
    FileGuid UNIQUEIDENTIFIER NULL,
    UserGuid UNIQUEIDENTIFIER NULL,
    UserId BIGINT NULL

);