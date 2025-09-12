-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 6 User Shop
-- ======================
CREATE TYPE [Shop] AS TABLE
(
    ShopId BIGINT NOT NULL PRIMARY KEY,
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