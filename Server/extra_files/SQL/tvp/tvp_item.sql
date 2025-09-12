-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 6.3 Item
-- ======================
CREATE TYPE [Item] AS TABLE
(
    ItemId BIGINT NOT NULL PRIMARY KEY,
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