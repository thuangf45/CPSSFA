USE [$(DatabaseName)];
GO

-- ======================
-- 6.5 Cart
-- ======================
CREATE TYPE [Cart] AS TABLE
(
    CartId BIGINT NOT NULL PRIMARY KEY,
    CartGuid UNIQUEIDENTIFIER NULL,

    CartDetails NVARCHAR(MAX) NULL,   -- JSON mảng các item: item_id, quantity, price

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    UserId BIGINT NULL,
    UserGuid UNIQUEIDENTIFIER NULL

);