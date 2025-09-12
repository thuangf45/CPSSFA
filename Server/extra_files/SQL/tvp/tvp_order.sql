-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 6.1 Order
-- ======================
CREATE TYPE [Order] AS TABLE
(
    OrderId BIGINT NOT NULL PRIMARY KEY,
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