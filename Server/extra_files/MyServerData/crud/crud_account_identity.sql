 -- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE AccountIdentity_Create
    @account_identites dbo.[AccountIdentity] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [account_identity] (
            identity_guid, 
            provider, 
            provider_key, 
            password_hash, 
            created_at, 
            last_used, 
            is_verified, 
            account_id,
            account_guid
        )
        SELECT 
            IdentityGuid, 
            Provider, 
            ProviderKey,
            PasswordHash, 
            CreatedAt, 
            LastUsed, 
            IsVerified, 
            AccountId, 
            AccountGuid
        FROM @account_identites;

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
CREATE OR ALTER PROCEDURE AccountIdentity_Read
    @account_identities dbo.[AccountIdentity] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [account_identity] ai
    JOIN @account_identities ais ON ais.IdentityId = ai.identity_id;
END
GO

CREATE OR ALTER PROCEDURE AccountIdentity_ReadAll
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [account_identity];
END
GO


-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE AccountIdentity_Update
    @account_identities dbo.[AccountIdentity] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE ai
        SET 
            ai.identity_guid = COALESCE(ais.IdentityGuid, ai.identity_guid),
            ai.provider      = COALESCE(ais.Provider, ai.provider),
            ai.provider_key  = COALESCE(ais.ProviderKey, ai.provider_key), 
            ai.password_hash = COALESCE(ais.PasswordHash, ai.password_hash), 
            ai.created_at    = COALESCE(ais.CreatedAt, ai.created_at), 
            ai.last_used     = COALESCE(ais.LastUsed, ai.last_used), 
            ai.is_verified   = COALESCE(ais.IsVerified, ai.is_verified), 
            ai.account_id    = COALESCE(ais.AccountId, ai.account_id), 
            ai.account_guid  = COALESCE(ais.AccountGuid, ai.account_guid)
        FROM [account_identity] ai
        JOIN @account_identities ais ON ai.identity_id = ais.IdentityId;


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
CREATE OR ALTER PROCEDURE AccountIdentity_Delete
    @account_identities dbo.[AccountIdentity] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE ai
        FROM [account_identity] ai
        JOIN @account_identities ais ON ai.identity_id = ais.IdentityId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO
