/*
  oneSIMPLY AI Hub - Tạo bảng trên database oneSIMPLY_TH (BINHNV\SQL2022)
  Quy ước đặt tên bảng: AIHub_xxx
  Chạy script này một lần trước khi khởi động API lần đầu.
*/
USE [oneSIMPLY_TH];
GO

IF OBJECT_ID(N'dbo.AIHub_Users', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AIHub_Users
    (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AIHub_Users PRIMARY KEY,
        Email         NVARCHAR(256)     NOT NULL,
        PasswordHash  NVARCHAR(512)     NOT NULL,
        CreatedAt     DATETIME2(0)      NOT NULL CONSTRAINT DF_AIHub_Users_CreatedAt DEFAULT (SYSUTCDATETIME()),
        IsActive      BIT               NOT NULL CONSTRAINT DF_AIHub_Users_IsActive DEFAULT (1),
        CONSTRAINT UQ_AIHub_Users_Email UNIQUE (Email)
    );
END
GO

IF OBJECT_ID(N'dbo.AIHub_Subscriptions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AIHub_Subscriptions
    (
        Id            INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_AIHub_Subscriptions PRIMARY KEY,
        Name          NVARCHAR(100)     NOT NULL,
        Price         DECIMAL(18, 2)    NOT NULL,
        MaxRequests   INT               NOT NULL,
        DurationDays  INT               NOT NULL,
        CONSTRAINT UQ_AIHub_Subscriptions_Name UNIQUE (Name)
    );
END
GO

IF OBJECT_ID(N'dbo.AIHub_UserSubscriptions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AIHub_UserSubscriptions
    (
        Id                 INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_AIHub_UserSubscriptions PRIMARY KEY,
        UserId             INT                NOT NULL,
        SubscriptionId     INT                NOT NULL,
        StartDate          DATETIME2(0)       NOT NULL,
        EndDate            DATETIME2(0)       NOT NULL,
        IsActive           BIT                NOT NULL CONSTRAINT DF_AIHub_UserSubscriptions_IsActive DEFAULT (1),
        RemainingRequests  INT                NOT NULL,
        CONSTRAINT FK_AIHub_UserSubscriptions_Users FOREIGN KEY (UserId) REFERENCES dbo.AIHub_Users (Id),
        CONSTRAINT FK_AIHub_UserSubscriptions_Subscriptions FOREIGN KEY (SubscriptionId) REFERENCES dbo.AIHub_Subscriptions (Id)
    );

    CREATE INDEX IX_AIHub_UserSubscriptions_UserId_IsActive ON dbo.AIHub_UserSubscriptions (UserId, IsActive);
END
GO

IF OBJECT_ID(N'dbo.AIHub_UsageLogs', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AIHub_UsageLogs
    (
        Id                 INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_AIHub_UsageLogs PRIMARY KEY,
        UserId             INT                NOT NULL,
        ModelUsed          NVARCHAR(100)      NOT NULL,
        PromptTokens       INT                NOT NULL,
        CompletionTokens   INT                NOT NULL,
        CreatedAt          DATETIME2(0)       NOT NULL CONSTRAINT DF_AIHub_UsageLogs_CreatedAt DEFAULT (SYSUTCDATETIME()),
        TaskType           NVARCHAR(50)       NOT NULL,
        CONSTRAINT FK_AIHub_UsageLogs_Users FOREIGN KEY (UserId) REFERENCES dbo.AIHub_Users (Id)
    );

    CREATE INDEX IX_AIHub_UsageLogs_UserId_CreatedAt ON dbo.AIHub_UsageLogs (UserId, CreatedAt DESC);
END
GO

IF OBJECT_ID(N'dbo.AIHub_Transactions', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AIHub_Transactions
    (
        Id         INT IDENTITY(1, 1) NOT NULL CONSTRAINT PK_AIHub_Transactions PRIMARY KEY,
        UserId     INT                NOT NULL,
        Amount     DECIMAL(18, 2)     NOT NULL,
        Status     NVARCHAR(20)       NOT NULL,
        Code       NVARCHAR(100)      NOT NULL,
        CreatedAt  DATETIME2(0)       NOT NULL CONSTRAINT DF_AIHub_Transactions_CreatedAt DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT FK_AIHub_Transactions_Users FOREIGN KEY (UserId) REFERENCES dbo.AIHub_Users (Id),
        CONSTRAINT UQ_AIHub_Transactions_Code UNIQUE (Code)
    );

    CREATE INDEX IX_AIHub_Transactions_Status_Code ON dbo.AIHub_Transactions (Status, Code);
END
GO

PRINT N'oneSIMPLY AI Hub: Hoàn tất tạo bảng AIHub_xxx.';
GO
