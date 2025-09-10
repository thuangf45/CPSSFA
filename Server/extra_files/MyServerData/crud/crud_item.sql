-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE Item_Create
    @items dbo.[Item] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [item] (
            item_guid,
            item_name, 
            item_description,
            avg_rating, 
            number_review, 
            price, 
            stock, 
            is_active,
            created_at, 
            updated_at,
            shop_id, 
            shop_guid
        )
        SELECT
            COALESCE(ItemGuid, NEWID()),
            ItemName, 
            ItemDescription,
            COALESCE(AvgRating,0), 
            COALESCE(NumberReview,0), 
            COALESCE(Price,0), 
            COALESCE(Stock,0), 
            COALESCE(IsActive,1),
            COALESCE(CreatedAt, GETDATE()), 
            COALESCE(UpdatedAt, GETDATE()),
            ShopId, 
            ShopGuid
        FROM @items;

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
CREATE OR ALTER PROCEDURE Item_Read
    @items dbo.[Item] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [item] i
    JOIN @items it ON i.item_id = it.ItemId;
END
GO


-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE Item_Update
    @items dbo.[Item] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE i
        SET
            i.item_guid        = COALESCE(it.ItemGuid, i.item_guid),
            i.item_name        = COALESCE(it.ItemName, i.item_name),
            i.item_description = COALESCE(it.ItemDescription, i.item_description),
            i.avg_rating       = COALESCE(it.AvgRating, i.avg_rating),
            i.number_review    = COALESCE(it.NumberReview, i.number_review),
            i.price            = COALESCE(it.Price, i.price),
            i.stock            = COALESCE(it.Stock, i.stock),
            i.is_active        = COALESCE(it.IsActive, i.is_active),
            i.created_at       = COALESCE(it.CreatedAt, i.created_at),
            i.updated_at       = COALESCE(it.UpdatedAt, i.updated_at),
            i.shop_id          = COALESCE(it.ShopId, i.shop_id),
            i.shop_guid        = COALESCE(it.ShopGuid, i.shop_guid)
        FROM [item] i
        JOIN @items it ON i.item_id = it.ItemId;

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
CREATE OR ALTER PROCEDURE Item_Delete
    @items dbo.[Item] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE i
        FROM [item] i
        JOIN @items it ON i.item_id = it.ItemId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO
