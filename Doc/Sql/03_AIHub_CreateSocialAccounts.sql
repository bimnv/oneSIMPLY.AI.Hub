/*
  oneSIMPLY AI Hub - Bảng kết nối tài khoản MXH (Facebook / TikTok)
  Quy ước: AIHub_SocialAccounts
*/
USE [oneSIMPLY_TH];
GO

IF OBJECT_ID(N'dbo.AIHub_SocialAccounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AIHub_SocialAccounts
    (
        Id           INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_AIHub_SocialAccounts PRIMARY KEY,
        UserId       INT                NOT NULL,
        Platform     NVARCHAR(50)       NOT NULL,
        PageId       NVARCHAR(100)      NOT NULL,
        PageName     NVARCHAR(200)      NOT NULL,
        AccessToken  NVARCHAR(1000)     NOT NULL,
        IsActive     BIT                NOT NULL CONSTRAINT DF_AIHub_SocialAccounts_IsActive DEFAULT (1),
        CreatedAt    DATETIME2(0)       NOT NULL CONSTRAINT DF_AIHub_SocialAccounts_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt    DATETIME2(0)       NOT NULL CONSTRAINT DF_AIHub_SocialAccounts_UpdatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_AIHub_SocialAccounts_Users FOREIGN KEY (UserId) REFERENCES dbo.AIHub_Users (Id),
        CONSTRAINT UQ_AIHub_SocialAccounts_User_Platform UNIQUE (UserId, Platform)
    );
END
GO

PRINT N'oneSIMPLY AI Hub: Hoàn tất tạo bảng AIHub_SocialAccounts.';
GO
