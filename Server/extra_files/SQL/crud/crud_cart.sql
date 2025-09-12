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

    SELECT @@ROWCOUNT AS Result;
END
GO


-- ======================
-- READ
-- ======================
CREATE OR ALTER PROCEDURE Cart_Read
    @carts dbo.[Cart] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [cart] c
    JOIN @carts ct ON ct.CartId = c.cart_id;
END
GO

CREATE OR ALTER PROCEDURE Cart_ReadAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [cart];
    -- ORDER BY cart_id DESC
    -- OFFSET (@PageNumber - 1) * @PageSize ROWS
    -- FETCH NEXT @PageSize ROWS ONLY;
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

    SELECT @@ROWCOUNT AS Result;
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

    DELETE c
    FROM [cart] c
    JOIN @carts ct ON c.cart_id = ct.CartId;

    SELECT @@ROWCOUNT AS Result;
END
GO
