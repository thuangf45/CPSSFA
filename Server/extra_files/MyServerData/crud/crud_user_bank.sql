-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CRUD cho User Bank
-- ======================

-- CREATE
CREATE OR ALTER PROCEDURE UserBank_Create
    @userBanks dbo.[UserBank] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [user_bank] (
            account_number,
            account_amount,
            currency,
            updated_at,
            user_guid,
            user_id
        )
        SELECT 
            AccountNumber,
            ISNULL(AccountAmount, 0),
            ISNULL(Currency, 'USD'),
            ISNULL(UpdatedAt, GETDATE()),
            UserGuid,
            UserId
        FROM @userBanks;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT -1 AS Result;
    END CATCH
END
GO


-- READ (lấy toàn bộ dữ liệu)
CREATE OR ALTER PROCEDURE UserBank_Read
    @userBanks dbo.[UserBank] READONLY
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * 
    FROM [user_bank] ub
    JOIN @userBanks u ON ub.user_bank_id = u.UserBankId;
END
GO

CREATE OR ALTER PROCEDURE UserBank_ReadAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT * 
    FROM [user_bank];
END
GO



-- UPDATE toàn bộ (theo PK)
CREATE OR ALTER PROCEDURE UserBank_Update
    @userBanks dbo.[UserBank] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE ub
        SET 
            ub.account_number = COALESCE(u.AccountNumber, ub.account_number),
            ub.account_amount = COALESCE(u.AccountAmount, ub.account_amount),
            ub.currency       = COALESCE(u.Currency, ub.currency),
            ub.updated_at     = COALESCE(u.UpdatedAt, GETDATE()),
            ub.user_guid      = COALESCE(u.UserGuid, ub.user_guid),
            ub.user_id        = COALESCE(u.UserId, ub.user_id)
        FROM [user_bank] ub
        JOIN @userBanks u ON ub.user_bank_id = u.UserBankId;


        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT -1 AS Result;
    END CATCH
END
GO


-- DELETE (theo PK)
CREATE OR ALTER PROCEDURE UserBank_Delete
    @userBankIds dbo.[UserBank] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE ub
        FROM [user_bank] ub
        JOIN @userBankIds u ON ub.user_bank_id = u.UserBankId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT -1 AS Result;
    END CATCH
END
GO
