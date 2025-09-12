USE [$(DatabaseName)];
GO

CREATE OR ALTER PROCEDURE Service_AddUsers
    @accounts dbo.[Account] READONLY,
    @account_identites dbo.[AccountIdentity] READONLY,
    @users dbo.[User] READONLY,
    @userSocieties dbo.[UserSociety] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @AccCount INT,
            @AccIdCount INT,
            @UserCount INT,
            @UserSocietyCount INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Gọi từng procedure và lấy kết quả
        EXEC @AccCount = Account_Create @accounts;
        EXEC @AccIdCount = AccountIdentity_Create @account_identites;

        IF (@AccCount <> @AccIdCount)
            THROW 50001, 'Account and AccountIdentity count mismatch', 1;

        EXEC @UserCount = User_Create @users;

        IF (@UserCount <> @AccCount)
            THROW 50002, 'User and Account count mismatch', 1;

        EXEC @UserSocietyCount = UserSociety_Create @userSocieties;

        IF (@UserSocietyCount <> @UserCount)
            THROW 50003, 'UserSociety and User count mismatch', 1;

        COMMIT TRANSACTION;
        SELECT @UserCount AS Result;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 0 AS Result;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE Service_GetUsers
AS
BEGIN
    SET NOCOUNT ON;

    -- Gọi trực tiếp các procedure, mỗi cái trả về một bảng
    EXEC Account_ReadAll;
    EXEC AccountIdentity_ReadAll;
    EXEC User_ReadAll;
    EXEC UserSociety_ReadAll;
    
END
GO

CREATE OR ALTER PROCEDURE Service_RemoveUsers
    @userSocieties dbo.[UserSociety] READONLY,
    @users dbo.[User] READONLY,
    @account_identites dbo.[AccountIdentity] READONLY,
    @accounts dbo.[Account] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserSocietyCount INT,
            @UserCount INT,
            @AccIdCount INT,
            @AccCount INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Xóa dữ liệu theo thứ tự ngược với thêm
        EXEC @UserSocietyCount = UserSociety_Delete @userSocieties;
        EXEC @UserCount = User_Delete @users;

        IF (@UserSocietyCount <> @UserCount)
            THROW 50003, 'UserSociety and User count mismatch', 1;

        EXEC @AccIdCount = AccountIdentity_Delete @account_identites;

        IF (@AccIdCount <> @UserCount)
            THROW 50002, 'AccountIdentity and User count mismatch', 1;

        EXEC @AccCount = Account_Delete @accounts;

        IF (@AccCount <> @AccIdCount)
            THROW 50001, 'Account and AccountIdentity count mismatch', 1;

        COMMIT TRANSACTION;
        SELECT @AccCount AS Result; -- trả về số lượng account xóa
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 0 AS Result;
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE Service_UpdateUserProfies
    @users dbo.[User] READONLY,
    @userSocieties dbo.[UserSociety] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @UserCount INT,
            @UserSocietyCount INT;

    BEGIN TRY
        BEGIN TRANSACTION;

        EXEC @UserCount = User_Update @users;
        EXEC @UserSocietyCount = UserSociety_Update @userSocieties;

        IF (@UserCount <> @UserSocietyCount)
            THROW 50001, 'User and UserSociety count mismatch', 1;

        COMMIT TRANSACTION;
        SELECT @UserCount AS Result; -- trả về số lượng user đã cập nhật
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SELECT 0 AS Result;
    END CATCH
END


