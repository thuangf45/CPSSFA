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
