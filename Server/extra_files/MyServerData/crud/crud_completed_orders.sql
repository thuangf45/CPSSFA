-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE CompletedOrders_Create
    @completed_orders dbo.[CompletedOrders] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [completed_orders] (
            completed_orders_guid,
            orders_details,
            created_at, 
            updated_at,
            user_id, 
            user_guid
        )
        SELECT
            COALESCE(CompletedOrdersGuid, NEWID()),
            OrdersDetails,
            COALESCE(CreatedAt, GETDATE()),
            COALESCE(UpdatedAt, GETDATE()),
            UserId, 
            UserGuid
        FROM @completed_orders;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO


-- ======================
-- READ
-- ======================
CREATE OR ALTER PROCEDURE CompletedOrders_Read
    @completed_orders dbo.[CompletedOrders] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [completed_orders] co
    JOIN @completed_orders c ON co.completed_orders_id = c.CompletedOrdersId;
END
GO


-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE CompletedOrders_Update
    @completed_orders dbo.[CompletedOrders] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE co
        SET
            co.completed_orders_guid = COALESCE(c.CompletedOrdersGuid, co.completed_orders_guid),
            co.orders_details        = COALESCE(c.OrdersDetails, co.orders_details),
            co.created_at            = COALESCE(c.CreatedAt, co.created_at),
            co.updated_at            = COALESCE(c.UpdatedAt, co.updated_at),
            co.user_id               = COALESCE(c.UserId, co.user_id),
            co.user_guid             = COALESCE(c.UserGuid, co.user_guid)
        FROM [completed_orders] co
        JOIN @completed_orders c ON co.completed_orders_id = c.CompletedOrdersId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO


-- ======================
-- DELETE
-- ======================
CREATE OR ALTER PROCEDURE CompletedOrders_Delete
    @completed_orders dbo.[CompletedOrders] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE co
        FROM [completed_orders] co
        JOIN @completed_orders c ON co.completed_orders_id = c.CompletedOrdersId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO
