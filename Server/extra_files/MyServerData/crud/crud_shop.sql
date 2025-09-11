-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE Shop_Create
    @shops dbo.[Shop] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [shop] (
            shop_guid,
            shop_name, 
            shop_description, 
            shop_address, 
            phone_number, 
            email, 
            avg_rating,
            shop_coin, 
            number_item, 
            number_order, 
            number_review, 
            is_active,
            created_at, 
            updated_at,
            user_id, 
            user_guid
        )
        SELECT
            COALESCE(ShopGuid, NEWID()),
            ShopName, 
            ShopDescription, 
            ShopAddress, 
            PhoneNumber, 
            Email, 
            COALESCE(AvgRating,0),
            COALESCE(ShopCoin,0), 
            COALESCE(NumberItem,0), 
            COALESCE(NumberOrder,0), 
            COALESCE(NumberTeview,0), 
            COALESCE(IsActive,1),
            COALESCE(CreatedAt, GETDATE()), 
            COALESCE(UpdatedAt, GETDATE()),
            UserId, 
            UserGuid
        FROM @shops;

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
CREATE OR ALTER PROCEDURE Shop_Read
    @shops dbo.[Shop] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [shop] s
    JOIN @shops sh ON s.shop_id = sh.ShopId;
END
GO

CREATE OR ALTER PROCEDURE Shop_ReadAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [shop];
END
GO

-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE Shop_Update
    @shops dbo.[Shop] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE s
        SET
            s.shop_guid        = COALESCE(sh.ShopGuid, s.shop_guid),
            s.shop_name        = COALESCE(sh.ShopName, s.shop_name),
            s.shop_description = COALESCE(sh.ShopDescription, s.shop_description),
            s.shop_address     = COALESCE(sh.ShopAddress, s.shop_address),
            s.phone_number     = COALESCE(sh.PhoneNumber, s.phone_number),
            s.email            = COALESCE(sh.Email, s.email),
            s.avg_rating       = COALESCE(sh.AvgRating, s.avg_rating),
            s.shop_coin        = COALESCE(sh.ShopCoin, s.shop_coin),
            s.number_item      = COALESCE(sh.NumberItem, s.number_item),
            s.number_order     = COALESCE(sh.NumberOrder, s.number_order),
            s.number_review    = COALESCE(sh.NumberTeview, s.number_review),
            s.is_active        = COALESCE(sh.IsActive, s.is_active),
            s.created_at       = COALESCE(sh.CreatedAt, s.created_at),
            s.updated_at       = COALESCE(sh.UpdatedAt, s.updated_at),
            s.user_id          = COALESCE(sh.UserId, s.user_id),
            s.user_guid        = COALESCE(sh.UserGuid, s.user_guid)
        FROM [shop] s
        JOIN @shops sh ON s.shop_id = sh.ShopId;

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
CREATE OR ALTER PROCEDURE Shop_Delete
    @shops dbo.[Shop] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE s
        FROM [shop] s
        JOIN @shops sh ON s.shop_id = sh.ShopId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO
