-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 7 Review
-- ======================
CREATE TYPE [Review] AS TABLE
(
    ReviewId BIGINT NOT NULL PRIMARY KEY,
    ReviewGuid UNIQUEIDENTIFIER NULL,

    TargetType NVARCHAR(50) NULL,          -- 'item', 'shop', … để phân biệt loại target
    Rating INT DEFAULT NULL,                       -- điểm đánh giá 0-5
    Content NVARCHAR(2000) NULL,                -- nội dung review

    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    UserGuid UNIQUEIDENTIFIER NULL,       -- người viết review
    UserId BIGINT NULL,       
    TargetGuid UNIQUEIDENTIFIER NULL,        -- đối tượng được review (item, shop, …)
    TargetId BIGINT NULL

);