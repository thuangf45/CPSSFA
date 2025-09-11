-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE Review_Create
    @reviews dbo.[Review] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [review] (
            review_guid,
            target_type, 
            rating, content,
            created_at, 
            updated_at,
            user_guid, 
            user_id,
            target_guid, 
            target_id
        )
        SELECT
            COALESCE(ReviewGuid, NEWID()),
            TargetType, 
            Rating, 
            Content,
            COALESCE(CreatedAt, GETDATE()),
            COALESCE(UpdatedAt, GETDATE()),
            UserGuid, 
            UserId,
            TargetGuid, 
            TargetId
        FROM @reviews;

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
CREATE OR ALTER PROCEDURE Review_Read
    @reviews dbo.[Review] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [review] r
    JOIN @reviews rv ON r.review_id = rv.ReviewId;
END
GO

CREATE OR ALTER PROCEDURE Review_ReadAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [review];
END
GO


-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE Review_Update
    @reviews dbo.[Review] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE r
        SET
            r.review_guid = COALESCE(rv.TargetGuid,r.review_guid),
            r.target_type = COALESCE(rv.TargetType, r.target_type),
            r.rating      = COALESCE(rv.Rating, r.rating),
            r.content     = COALESCE(rv.Content, r.content),
            r.created_at  = COALESCE(rv.CreatedAt, r.created_at),
            r.updated_at  = COALESCE(rv.UpdatedAt, r.updated_at),
            r.user_guid   = COALESCE(rv.UserGuid, r.user_guid),
            r.user_id     = COALESCE(rv.UserId, r.user_id),
            r.target_guid = COALESCE(rv.TargetGuid, r.target_guid),
            r.target_id   = COALESCE(rv.TargetId, r.target_id)
        FROM [review] r
        JOIN @reviews rv ON r.review_id = rv.ReviewId;

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
CREATE OR ALTER PROCEDURE Review_Delete
    @reviews dbo.[Review] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE r
        FROM [review] r
        JOIN @reviews rv ON r.review_id = rv.ReviewId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO
