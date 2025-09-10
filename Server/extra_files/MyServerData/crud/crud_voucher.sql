-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- CREATE
-- ======================
CREATE OR ALTER PROCEDURE Voucher_Create
    @vouchers dbo.[Voucher] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        INSERT INTO [voucher] (
            voucher_guid,
            code, 
            description,
            discount_type, 
            discount_value, 
            max_discount,
            valid_from, 
            valid_to, 
            is_active,
            max_usage, 
            used_count, 
            used_users,
            created_at, 
            updated_at
        )
        SELECT
            COALESCE(VoucherGuid, NEWID()),
            Code, 
            Description,
            DiscountType, 
            DiscountValue, 
            MaxDiscount,
            ValidFrom, 
            ValidTo, 
            COALESCE(IsActive,1),
            COALESCE(MaxUsage,1), 
            COALESCE(UsedCount,0), 
            UsedUsers,
            COALESCE(CreatedAt, GETDATE()), 
            COALESCE(UpdatedAt, GETDATE())
        FROM @vouchers;

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
CREATE OR ALTER PROCEDURE Voucher_Read
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM [voucher];
END
GO


-- ======================
-- UPDATE
-- ======================
CREATE OR ALTER PROCEDURE Voucher_Update
    @vouchers dbo.[Voucher] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE v
        SET
            v.voucher_guid   = COALESCE(vc.VoucherGuid, v.voucher_guid),
            v.code           = COALESCE(vc.Code, v.code),
            v.description    = COALESCE(vc.Description, v.description),
            v.discount_type  = COALESCE(vc.DiscountType, v.discount_type),
            v.discount_value = COALESCE(vc.DiscountValue, v.discount_value),
            v.max_discount   = COALESCE(vc.MaxDiscount, v.max_discount),
            v.valid_from     = COALESCE(vc.ValidFrom, v.valid_from),
            v.valid_to       = COALESCE(vc.ValidTo, v.valid_to),
            v.is_active      = COALESCE(vc.IsActive, v.is_active),
            v.max_usage      = COALESCE(vc.MaxUsage, v.max_usage),
            v.used_count     = COALESCE(vc.UsedCount, v.used_count),
            v.used_users     = COALESCE(vc.UsedUsers, v.used_users),
            v.created_at     = COALESCE(vc.CreatedAt, v.created_at),
            v.updated_at     = COALESCE(vc.UpdatedAt, v.updated_at)
        FROM [voucher] v
        JOIN @vouchers vc ON v.voucher_id = vc.VoucherId;

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
CREATE OR ALTER PROCEDURE Voucher_Delete
    @vouchers dbo.[Voucher] READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE v
        FROM [voucher] v
        JOIN @vouchers vc ON v.voucher_id = vc.VoucherId;

        COMMIT TRANSACTION;
        SELECT @@ROWCOUNT AS Result;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        SELECT 0 AS Result;
    END CATCH;
END
GO
