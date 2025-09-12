-- Switch to the database
USE [$(DatabaseName)];
GO

-- ======================
-- 5.2 Reaction
-- ======================
CREATE Type [Reaction] AS TABLE(
    ReactionId BIGINT NOT NULL PRIMARY KEY,
    ReactionGuid UNIQUEIDENTIFIER NULL,

    ReactionType NVARCHAR(20) NULL,
    CreatedAt DATETIME NULL,
    UpdatedAt DATETIME NULL,

    PostId BIGINT NULL,
    PostGuid UNIQUEIDENTIFIER NULL,
    UserSocietyId BIGINT NULL,
    UserSocietyGuid UNIQUEIDENTIFIER NULL
);
