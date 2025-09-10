:setvar DatabaseName "base"

SET QUOTED_IDENTIFIER ON;
GO

-- Drop database if it exists
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'$(DatabaseName)')
BEGIN
    DROP DATABASE [$(DatabaseName)];
END
GO

-- Create database
CREATE DATABASE [$(DatabaseName)];
GO



-- Include schema and CRUD scripts
:r schema/base.sql
:r schema/type.sql
:r crud/crud_account.sql
:r crud/crud_account_identity.sql
:r crud/crud_cart.sql
:r crud/crud_completed_order.sql
:r crud/crud_file.sql
:r crud/crud_film.sql
:r crud/crud_item.sql
:r crud/crud_order.sql
:r crud/crud_post.sql
:r crud/crud_reaction.sql
:r crud/crud_review.sql
:r crud/crud_shop.sql
:r crud/crud_user.sql
:r crud/crud_user_bank.sql
:r crud/crud_user_society.sql
:r crud/crud_voucher.sql