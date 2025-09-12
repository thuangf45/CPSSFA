-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE User_Create
    @users dbo.[User] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [user] (
            user_guid, 
            full_name, 
            avatar, 
            bio, 
            user_address, 
            birthday, 
            gender, 
            updated_at, 
            account_id, 
            account_guid
        )
        SELECT 
            UserGuid,
            FullName, 
            Avatar, 
            Bio, 
            UserAddress, 
            Birthday, 
            Gender, 
            UpdatedAt,
            AccountId, 
            AccountGuid
        FROM @users;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result; -- lỗi
    END CATCH;
END
GO


-- ======================
-- READ
-- ======================
CREATE OR ALTER PROCEDURE User_Read
    @users dbo.[User] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [user] u
    JOIN @users us ON u.user_id = us.UserId;
END
GO

CREATE OR ALTER PROCEDURE User_ReadAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [user];
END
GO


-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE User_Update
    @users dbo.[User] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE u
    SET
        u.user_guid    = COALESCE(us.UserGuid, u.user_guid),
        u.full_name    = COALESCE(us.FullName, u.full_name),
        u.avatar       = COALESCE(us.Avatar, u.avatar),
        u.bio          = COALESCE(us.Bio, u.bio),
        u.user_address = COALESCE(us.UserAddress, u.user_address),
        u.birthday     = COALESCE(us.Birthday, u.birthday),
        u.gender       = COALESCE(us.Gender, u.gender),
        u.updated_at   = COALESCE(us.UpdatedAt, u.updated_at),
        u.account_id   = COALESCE(us.AccountId, u.account_id),
        u.account_guid = COALESCE(us.AccountGuid, u.account_guid)
    FROM [user] u
    JOIN @users us ON u.user_id = us.UserId;

    SELECT @@ROWCOUNT AS Result;
END
GO


-- ======================
-- DELETE
-- ======================
CREATE OR ALTER PROCEDURE User_Delete
    @users dbo.[User] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DELETE u
    FROM [user] u
    JOIN @users us ON u.user_id = us.UserId;

    SELECT @@ROWCOUNT AS Result;
END
GO
