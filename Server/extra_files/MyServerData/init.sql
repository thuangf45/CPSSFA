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

-- Include Table scripts
:r table/table_account.sql
:r table/table_account_identity.sql
:r table/table_user.sql
:r table/table_user_bank.sql
:r table/table_user_society.sql
:r table/table_post.sql
:r table/table_reaction.sql
:r table/table_shop.sql
:r table/table_order.sql
:r table/table_voucher.sql
:r table/table_item.sql
:r table/table_completed_orders.sql
:r table/table_cart.sql
:r table/table_review.sql
:r table/table_file.sql
:r table/table_film.sql

-- Include tvp scripts
:r tvp/tvp_account.sql
:r tvp/tvp_account_identity.sql
:r tvp/tvp_user.sql
:r tvp/tvp_user_bank.sql
:r tvp/tvp_user_society.sql
:r tvp/tvp_post.sql
:r tvp/tvp_reaction.sql
:r tvp/tvp_shop.sql
:r tvp/tvp_order.sql
:r tvp/tvp_voucher.sql
:r tvp/tvp_item.sql
:r tvp/tvp_completed_orders.sql
:r tvp/tvp_cart.sql
:r tvp/tvp_review.sql
:r tvp/tvp_file.sql
:r tvp/tvp_film.sql

-- Include Crud scripts
:r crud/crud_account.sql
:r crud/crud_account_identity.sql
:r crud/crud_user.sql
:r crud/crud_user_bank.sql
:r crud/crud_user_society.sql
:r crud/crud_post.sql
:r crud/crud_reaction.sql
:r crud/crud_shop.sql
:r crud/crud_order.sql
:r crud/crud_voucher.sql
:r crud/crud_item.sql
:r crud/crud_completed_orders.sql
:r crud/crud_cart.sql
:r crud/crud_review.sql
:r crud/crud_file.sql
:r crud/crud_film.sql