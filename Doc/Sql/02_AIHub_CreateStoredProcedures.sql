/*
  oneSIMPLY AI Hub - Stored Procedures
  Quy ước đặt tên: API_AIHub_xxx
  Database: oneSIMPLY_TH (BINHNV\SQL2022)
*/
USE [oneSIMPLY_TH];
GO

CREATE OR ALTER PROCEDURE dbo.API_AIHub_Subscription_List
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Name, Price, MaxRequests, DurationDays
    FROM dbo.AIHub_Subscriptions
    ORDER BY Price;
END
GO

CREATE OR ALTER PROCEDURE dbo.API_AIHub_Subscription_SeedDefaults
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.AIHub_Subscriptions)
    BEGIN
        INSERT INTO dbo.AIHub_Subscriptions (Name, Price, MaxRequests, DurationDays)
        VALUES
            (N'Free Trial', 0, 10, 30),
            (N'Starter', 390000, 50, 30),
            (N'Professional', 990000, 200, 30);
    END

    SELECT Id, Name, Price, MaxRequests, DurationDays
    FROM dbo.AIHub_Subscriptions
    ORDER BY Price;
END
GO

CREATE OR ALTER PROCEDURE dbo.API_AIHub_User_GetByEmail
    @Email NVARCHAR(256)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, Email, PasswordHash, CreatedAt, IsActive
    FROM dbo.AIHub_Users
    WHERE Email = @Email;
END
GO

CREATE OR ALTER PROCEDURE dbo.API_AIHub_User_Create
    @Email NVARCHAR(256),
    @PasswordHash NVARCHAR(512)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.AIHub_Users (Email, PasswordHash, CreatedAt, IsActive)
    VALUES (@Email, @PasswordHash, SYSUTCDATETIME(), 1);

    SELECT Id, Email, PasswordHash, CreatedAt, IsActive
    FROM dbo.AIHub_Users
    WHERE Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.API_AIHub_UserSubscription_GetActive
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (1)
        us.Id,
        us.UserId,
        us.SubscriptionId,
        us.StartDate,
        us.EndDate,
        us.IsActive,
        us.RemainingRequests,
        s.Name AS SubscriptionName,
        s.Price AS SubscriptionPrice,
        s.MaxRequests AS SubscriptionMaxRequests,
        s.DurationDays AS SubscriptionDurationDays
    FROM dbo.AIHub_UserSubscriptions us
    INNER JOIN dbo.AIHub_Subscriptions s ON s.Id = us.SubscriptionId
    WHERE us.UserId = @UserId
      AND us.IsActive = 1
      AND us.EndDate > SYSUTCDATETIME()
    ORDER BY us.EndDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.API_AIHub_UserSubscription_Create
    @UserId INT,
    @SubscriptionId INT,
    @StartDate DATETIME2(0),
    @EndDate DATETIME2(0),
    @RemainingRequests INT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.AIHub_UserSubscriptions (UserId, SubscriptionId, StartDate, EndDate, IsActive, RemainingRequests)
    VALUES (@UserId, @SubscriptionId, @StartDate, @EndDate, 1, @RemainingRequests);

    SELECT Id, UserId, SubscriptionId, StartDate, EndDate, IsActive, RemainingRequests
    FROM dbo.AIHub_UserSubscriptions
    WHERE Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.API_AIHub_UserSubscription_DeductRequest
    @UserSubscriptionId INT,
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.AIHub_UserSubscriptions
    SET RemainingRequests = RemainingRequests - 1
    WHERE Id = @UserSubscriptionId
      AND UserId = @UserId
      AND IsActive = 1
      AND RemainingRequests > 0
      AND EndDate > SYSUTCDATETIME();

    IF @@ROWCOUNT = 0
        THROW 50001, N'Không thể trừ hạn mức. Gói cước đã hết hoặc không hợp lệ.', 1;

    SELECT Id, UserId, SubscriptionId, StartDate, EndDate, IsActive, RemainingRequests
    FROM dbo.AIHub_UserSubscriptions
    WHERE Id = @UserSubscriptionId;
END
GO

CREATE OR ALTER PROCEDURE dbo.API_AIHub_UsageLog_Create
    @UserId INT,
    @ModelUsed NVARCHAR(100),
    @PromptTokens INT,
    @CompletionTokens INT,
    @TaskType NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.AIHub_UsageLogs (UserId, ModelUsed, PromptTokens, CompletionTokens, CreatedAt, TaskType)
    VALUES (@UserId, @ModelUsed, @PromptTokens, @CompletionTokens, SYSUTCDATETIME(), @TaskType);

    SELECT Id, UserId, ModelUsed, PromptTokens, CompletionTokens, CreatedAt, TaskType
    FROM dbo.AIHub_UsageLogs
    WHERE Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.API_AIHub_Transaction_Create
    @UserId INT,
    @Amount DECIMAL(18, 2),
    @Code NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.AIHub_Transactions (UserId, Amount, Status, Code, CreatedAt)
    VALUES (@UserId, @Amount, N'Pending', @Code, SYSUTCDATETIME());

    SELECT Id, UserId, Amount, Status, Code, CreatedAt
    FROM dbo.AIHub_Transactions
    WHERE Id = SCOPE_IDENTITY();
END
GO

CREATE OR ALTER PROCEDURE dbo.API_AIHub_Transaction_GetPendingByCode
    @Code NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, UserId, Amount, Status, Code, CreatedAt
    FROM dbo.AIHub_Transactions
    WHERE Code = @Code
      AND Status = N'Pending';
END
GO

CREATE OR ALTER PROCEDURE dbo.API_AIHub_Transaction_CompleteAndActivate
    @TransactionId INT,
    @SubscriptionId INT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRANSACTION;

    DECLARE @UserId INT;
    DECLARE @DurationDays INT;
    DECLARE @MaxRequests INT;

    UPDATE dbo.AIHub_Transactions
    SET Status = N'Success'
    WHERE Id = @TransactionId
      AND Status = N'Pending';

    IF @@ROWCOUNT = 0
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50002, N'Giao dịch không tồn tại hoặc đã được xử lý.', 1;
    END

    SELECT @UserId = UserId
    FROM dbo.AIHub_Transactions
    WHERE Id = @TransactionId;

    SELECT @DurationDays = DurationDays, @MaxRequests = MaxRequests
    FROM dbo.AIHub_Subscriptions
    WHERE Id = @SubscriptionId;

    IF @DurationDays IS NULL
    BEGIN
        ROLLBACK TRANSACTION;
        THROW 50003, N'Gói cước không tồn tại.', 1;
    END

    IF EXISTS (SELECT 1 FROM dbo.AIHub_UserSubscriptions WHERE UserId = @UserId AND IsActive = 1)
    BEGIN
        UPDATE dbo.AIHub_UserSubscriptions
        SET EndDate = DATEADD(DAY, @DurationDays, SYSUTCDATETIME()),
            RemainingRequests = RemainingRequests + @MaxRequests
        WHERE UserId = @UserId
          AND IsActive = 1;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.AIHub_UserSubscriptions (UserId, SubscriptionId, StartDate, EndDate, IsActive, RemainingRequests)
        VALUES (@UserId, @SubscriptionId, SYSUTCDATETIME(), DATEADD(DAY, @DurationDays, SYSUTCDATETIME()), 1, @MaxRequests);
    END

    COMMIT TRANSACTION;

    SELECT @UserId AS UserId, @SubscriptionId AS SubscriptionId, N'Success' AS Status;
END
GO

PRINT N'oneSIMPLY AI Hub: Hoàn tất tạo Stored Procedures API_AIHub_xxx.';
GO
