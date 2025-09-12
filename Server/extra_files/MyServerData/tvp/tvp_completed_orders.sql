-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 6.4 Completed orders
-- ======================
CREATE TYPE [CompletedOrders] AS TABLE
(
    CompletedOrdersId BIGINT NOT NULL PRIMARY KEY,
    CompletedOrdersGuid UNIQUEIDENTIFIER NULL,

    OrdersDetails NVARCHAR(MAX) NULL,        -- JSON mảng các order

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    UserId BIGINT NULL,
    UserGuid UNIQUEIDENTIFIER NULL

);