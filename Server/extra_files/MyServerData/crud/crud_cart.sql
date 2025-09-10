-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE Cart_Create
    @carts dbo.[Cart] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [cart] (
            cart_guid,
            cart_details,
            created_at, 
            updated_at,
            user_id, 
            user_guid
        )
        SELECT
            COALESCE(CartGuid, NEWID()),
            CartDetails,
            COALESCE(CreatedAt, GETDATE()),
            COALESCE(UpdatedAt, GETDATE()),
            UserId, 
            UserGuid
        FROM @carts;

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
CREATE OR ALTER PROCEDURE Cart_Read
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [cart];
END
GO


-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE Cart_Update
    @carts dbo.[Cart] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE c
        SET
            c.cart_guid    = COALESCE(ct.CartGuid, c.cart_guid),
            c.cart_details = COALESCE(ct.CartDetails, c.cart_details),
            c.created_at   = COALESCE(ct.CreatedAt, c.created_at),
            c.updated_at   = COALESCE(ct.UpdatedAt, c.updated_at),
            c.user_id      = COALESCE(ct.UserId, c.user_id),
            c.user_guid    = COALESCE(ct.UserGuid, c.user_guid)
        FROM [cart] c
        JOIN @carts ct ON c.cart_id = ct.CartId;

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
CREATE OR ALTER PROCEDURE Cart_Delete
    @carts dbo.[Cart] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE c
        FROM [cart] c
        JOIN @carts ct ON c.cart_id = ct.CartId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO
