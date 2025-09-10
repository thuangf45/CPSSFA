-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE Reaction_Create
    @reactions dbo.[Reaction] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [reaction] (
            reaction_guid,
            reaction_type,
            created_at,
            updated_at,
            post_id,
            post_guid,
            user_society_id,
            user_society_guid
        )
        SELECT
            COALESCE(ReactionGuid, NEWID()),
            ReactionType,
            COALESCE(CreatedAt, GETDATE()),
            COALESCE(UpdatedAt, GETDATE()),
            PostId,
            PostGuid,
            UserSocietyId,
            UserSocietyGuid
        FROM @reactions;

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
CREATE OR ALTER PROCEDURE Reaction_Read
    @reactions dbo.[Reaction] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [reaction] r
    JOIN @reactions rx ON r.reaction_id = rx.ReactionId;
END
GO


-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE Reaction_Update
    @reactions dbo.[Reaction] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE r
        SET
            r.reaction_guid     = COALESCE(rx.ReactionGuid, r.reaction_guid),
            r.reaction_type     = COALESCE(rx.ReactionType, r.reaction_type),
            r.created_at        = COALESCE(rx.CreatedAt, r.created_at),
            r.updated_at        = COALESCE(rx.UpdatedAt, r.updated_at),
            r.post_id           = COALESCE(rx.PostId, r.post_id),
            r.post_guid         = COALESCE(rx.PostGuid, r.post_guid),
            r.user_society_id   = COALESCE(rx.UserSocietyId, r.user_society_id),
            r.user_society_guid = COALESCE(rx.UserSocietyGuid, r.user_society_guid)
        FROM [reaction] r
        JOIN @reactions rx ON r.reaction_id = rx.ReactionId;

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
CREATE OR ALTER PROCEDURE Reaction_Delete
    @reactions dbo.[Reaction] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE r
        FROM [reaction] r
        JOIN @reactions rx ON r.reaction_id = rx.ReactionId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO
