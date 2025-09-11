-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE File_Create
    @files dbo.[File] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [file] (
            file_guid,
            file_name, 
            file_type, 
            file_url,
            created_at, 
            updated_at
        )
        SELECT
            COALESCE(FileGuid, NEWID()),
            FileName, 
            FileType, 
            FileUrl,
            COALESCE(CreatedAt, GETDATE()),
            COALESCE(UpdatedAt, GETDATE())
        FROM @files;

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
CREATE OR ALTER PROCEDURE File_Read
    @files dbo.[File] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [file] f
    JOIN @files fl ON f.file_id = fl.FileId;
END
GO

CREATE OR ALTER PROCEDURE File_ReadAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [file];
END
GO

-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE File_Update
    @files dbo.[File] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE f
        SET
            f.file_guid  = COALESCE(fl.FileGuid, f.file_guid),
            f.file_name  = COALESCE(fl.FileName, f.file_name),
            f.file_type  = COALESCE(fl.FileType, f.file_type),
            f.file_url   = COALESCE(fl.FileUrl, f.file_url),
            f.created_at = COALESCE(fl.CreatedAt, f.created_at),
            f.updated_at = COALESCE(fl.UpdatedAt, f.updated_at)
        FROM [file] f
        JOIN @files fl ON f.file_id = fl.FileId;

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
CREATE OR ALTER PROCEDURE File_Delete
    @files dbo.[File] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE f
        FROM [file] f
        JOIN @files fl ON f.file_id = fl.FileId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO
