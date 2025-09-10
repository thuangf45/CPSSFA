-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE Post_Create
    @posts dbo.[Post] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [post] (
            post_guid, 
            content, 
            created_at, 
            updated_at, 
            number_comment, 
            number_reaction,
            parent_id, 
            parent_guid,
            user_society_id, 
            user_society_guid
        )
        SELECT
            COALESCE(PostGuid, NEWID()),
            Content,
            COALESCE(CreatedAt, GETDATE()),
            COALESCE(UpdatedAt, GETDATE()),
            COALESCE(NumberComment, 0),
            COALESCE(NumberReaction, 0),
            ParentId,
            ParentGuid,
            UserSocietyId,
            UserSocietyGuid
        FROM @posts;

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
CREATE OR ALTER PROCEDURE Post_Read
    @posts dbo.[Post] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [post] p
    JOIN @posts po ON p.post_id = po.PostId;
END
GO


-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE Post_Update
    @posts dbo.[Post] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE p
        SET
            p.post_guid        = COALESCE(po.PostGuid, p.post_guid),
            p.content          = COALESCE(po.Content, p.content),
            p.created_at       = COALESCE(po.CreatedAt, p.created_at),
            p.updated_at       = COALESCE(po.UpdatedAt, p.updated_at),
            p.number_comment   = COALESCE(po.NumberComment, p.number_comment),
            p.number_reaction  = COALESCE(po.NumberReaction, p.number_reaction),
            p.parent_id        = COALESCE(po.ParentId, p.parent_id),
            p.parent_guid      = COALESCE(po.ParentGuid, p.parent_guid),
            p.user_society_id  = COALESCE(po.UserSocietyId, p.user_society_id),
            p.user_society_guid= COALESCE(po.UserSocietyGuid, p.user_society_guid)
        FROM [post] p
        JOIN @posts po ON p.post_id = po.PostId;

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
CREATE OR ALTER PROCEDURE Post_Delete
    @posts dbo.[Post] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE p
        FROM [post] p
        JOIN @posts po ON p.post_id = po.PostId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO
