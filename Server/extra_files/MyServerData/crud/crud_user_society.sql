-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CRUD cho User Society
-- ======================

-- CREATE
CREATE OR ALTER PROCEDURE UserSociety_Create
    @userSocieties dbo.[UserSociety] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [user_society] (
        user_society_guid,
        reputation_score,
        number_follower,
        number_following,
        number_post,
        number_comment,
        user_guid,
        user_id
    )
    SELECT
        COALESCE(UserSocietyGuid, NEWID()),
        COALESCE(ReputationScore, 100),
        COALESCE(NumberFollower, 0),
        COALESCE(NumberFollowing, 0),
        COALESCE(NumberPost, 0),
        COALESCE(NumberComment, 0),
        UserGuid,
        UserId
    FROM @userSocieties;
    
    SELECT @@ROWCOUNT AS Result;
END
GO


-- READ
CREATE OR ALTER PROCEDURE UserSociety_Read
    @userSocieties dbo.[UserSociety] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [user_society] us
    JOIN @userSocieties u ON us.user_society_id = u.UserSocietyId;
END
GO

CREATE OR ALTER PROCEDURE UserSociety_ReadAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [user_society];
END
GO

-- UPDATE
CREATE OR ALTER PROCEDURE UserSociety_Update
    @userSocieties dbo.[UserSociety] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE us
    SET
        us.user_society_guid = COALESCE(u.UserSocietyGuid, us.user_society_guid),
        us.reputation_score  = COALESCE(u.ReputationScore, us.reputation_score),
        us.number_follower   = COALESCE(u.NumberFollower, us.number_follower),
        us.number_following  = COALESCE(u.NumberFollowing, us.number_following),
        us.number_post       = COALESCE(u.NumberPost, us.number_post),
        us.number_comment    = COALESCE(u.NumberComment, us.number_comment),
        us.user_guid         = COALESCE(u.UserGuid, us.user_guid),
        us.user_id           = COALESCE(u.UserId, us.user_id)
    FROM [user_society] us
    JOIN @userSocieties u ON us.user_society_id = u.UserSocietyId;

    SELECT @@ROWCOUNT AS Result;
END
GO


-- DELETE
CREATE OR ALTER PROCEDURE UserSociety_Delete
    @userSocieties dbo.[UserSociety] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DELETE us
    FROM [user_society] us
    JOIN @userSocieties u ON us.user_society_id = u.UserSocietyId;

    SELECT @@ROWCOUNT AS Result;
END
GO
