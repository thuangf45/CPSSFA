-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE Film_Create
    @films dbo.[Film] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [film] (
            film_guid, 
            film_name, 
            film_description, 
            film_cost,
            avg_rating, 
            number_review, 
            number_view,
            created_at, 
            updated_at,
            file_id, 
            file_guid, 
            user_guid, 
            user_id
        )
        SELECT
            COALESCE(FilmGuid, NEWID()),
            FilName, 
            FilmDescription, 
            FilmCost,
            COALESCE(AvgRating, 0), 
            COALESCE(NumberReview, 0), 
            COALESCE(NumberView, 0),
            COALESCE(CreatedAt, GETDATE()), 
            COALESCE(UpdatedAt, GETDATE()),
            FileId, 
            FileGuid, 
            UserGuid, 
            UserId
        FROM @films;

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
CREATE OR ALTER PROCEDURE Film_Read
    @films dbo.[Film] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [film] f
    JOIN @films fl ON f.film_id = fl.FilmId;
END
GO


-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE Film_Update
    @films dbo.[Film] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE f
        SET
            f.film_guid        = COALESCE(fl.FilmGuid, f.film_guid),
            f.film_name        = COALESCE(fl.FilName, f.film_name),
            f.film_description = COALESCE(fl.FilmDescription, f.film_description),
            f.film_cost        = COALESCE(fl.FilmCost, f.film_cost),
            f.avg_rating       = COALESCE(fl.AvgRating, f.avg_rating),
            f.number_review    = COALESCE(fl.NumberReview, f.number_review),
            f.number_view      = COALESCE(fl.NumberView, f.number_view),
            f.created_at       = COALESCE(fl.CreatedAt, f.created_at),
            f.updated_at       = COALESCE(fl.UpdatedAt, f.updated_at),
            f.file_id          = COALESCE(fl.FileId, f.file_id),
            f.file_guid        = COALESCE(fl.FileGuid, f.file_guid),
            f.user_guid        = COALESCE(fl.UserGuid, f.user_guid),
            f.user_id          = COALESCE(fl.UserId, f.user_id)
        FROM [film] f
        JOIN @films fl ON f.film_id = fl.FilmId;

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
CREATE OR ALTER PROCEDURE Film_Delete
    @films dbo.[Film] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE f
        FROM [film] f
        JOIN @films fl ON f.film_id = fl.FilmId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO
