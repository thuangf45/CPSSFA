-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE Order_Create
    @orders dbo.[Order] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [order] (
            order_guid,
            order_name, 
            order_description, 
            order_details,
            shop_address, 
            shoppers_address, 
            shoppers_phone_number,
            form_shopping, 
            payment_status, 
            shipping_status, 
            payment_method,
            total_amount, 
            discount_amount,
            created_at, 
            updated_at,
            shop_id, 
            shop_guid, 
            user_id, 
            user_guid
        )
        SELECT
            COALESCE(OrderGuid, NEWID()),
            OrderName, 
            OrderDescription, 
            OrderDetails,
            ShopAddress, 
            ShoppersAddress, 
            ShoppersPhoneNumber,
            COALESCE(FormShopping,'offline'), 
            COALESCE(PaymentStatus,'paid'), 
            ShippingStatus, 
            COALESCE(PaymentMethod,'cash in person'),
            COALESCE(TotalAmount,0), 
            COALESCE(DiscountAmount,0),
            COALESCE(CreatedAt, GETDATE()), 
            COALESCE(UpdatedAt, GETDATE()),
            ShopId, 
            ShopGuid, 
            UserId, 
            UserGuid
        FROM @orders;

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
CREATE OR ALTER PROCEDURE Order_Read
    @orders dbo.[Order] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [order] o
    JOIN @orders orw ON o.order_id = orw.OrderId;
END
GO

CREATE OR ALTER PROCEDURE Order_ReadAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [order];
END
GO

-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE Order_Update
    @orders dbo.[Order] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE o
        SET
            o.order_guid            = COALESCE(orw.OrderGuid, o.order_guid),
            o.order_name            = COALESCE(orw.OrderName, o.order_name),
            o.order_description     = COALESCE(orw.OrderDescription, o.order_description),
            o.order_details         = COALESCE(orw.OrderDetails, o.order_details),
            o.shop_address          = COALESCE(orw.ShopAddress, o.shop_address),
            o.shoppers_address      = COALESCE(orw.ShoppersAddress, o.shoppers_address),
            o.shoppers_phone_number = COALESCE(orw.ShoppersPhoneNumber, o.shoppers_phone_number),
            o.form_shopping         = COALESCE(orw.FormShopping, o.form_shopping),
            o.payment_status        = COALESCE(orw.PaymentStatus, o.payment_status),
            o.shipping_status       = COALESCE(orw.ShippingStatus, o.shipping_status),
            o.payment_method        = COALESCE(orw.PaymentMethod, o.payment_method),
            o.total_amount          = COALESCE(orw.TotalAmount, o.total_amount),
            o.discount_amount       = COALESCE(orw.DiscountAmount, o.discount_amount),
            o.created_at            = COALESCE(orw.CreatedAt, o.created_at),
            o.updated_at            = COALESCE(orw.UpdatedAt, o.updated_at),
            o.shop_id               = COALESCE(orw.ShopId, o.shop_id),
            o.shop_guid             = COALESCE(orw.ShopGuid, o.shop_guid),
            o.user_id               = COALESCE(orw.UserId, o.user_id),
            o.user_guid             = COALESCE(orw.UserGuid, o.user_guid)
        FROM [order] o
        JOIN @orders orw ON o.order_id = orw.OrderId;

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
CREATE OR ALTER PROCEDURE Order_Delete
    @orders dbo.[Order] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE o
        FROM [order] o
        JOIN @orders orw ON o.order_id = orw.OrderId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO
