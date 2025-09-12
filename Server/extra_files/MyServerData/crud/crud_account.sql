-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE Account_Create
    @accounts dbo.[Account] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO [account] (
        account_guid, 
        role, 
        created_at
    )
    SELECT 
        AccountGuid, 
        Role, 
        CreatedAt
    FROM @accounts;

    SELECT @@ROWCOUNT AS Result;
END
GO


-- ======================
-- READ
-- ======================
CREATE OR ALTER PROCEDURE Account_Read
    @accounts dbo.[Account] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [account] a
    JOIN @accounts acc ON acc.AccountId = a.account_id
END
GO

CREATE OR ALTER PROCEDURE Account_ReadAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [account] a;
END
GO

-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE Account_Update
    @accounts dbo.[Account] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE a
    SET 
        a.account_guid = COALESCE(acc.AccountGuid, a.account_guid),
        a.role = COALESCE(acc.Role, a.role),
        a.created_at = COALESCE(acc.CreatedAt, a.created_at)
    FROM [account] a
    JOIN @accounts acc ON a.account_id = acc.AccountId;

    SELECT @@ROWCOUNT AS Result;
END
GO


-- ======================
-- DELETE
-- ======================
CREATE OR ALTER PROCEDURE Account_Delete
    @accounts dbo.[Account] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    DELETE a
    FROM [account] a
    JOIN @accounts acc ON a.account_id = acc.AccountId;

    SELECT @@ROWCOUNT AS Result;
END
GO
