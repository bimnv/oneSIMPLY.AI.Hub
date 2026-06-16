namespace oneSIMPLY.AIHub.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public class Subscription
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int MaxRequests { get; set; }
    public int DurationDays { get; set; }
}

public class UserSubscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int RemainingRequests { get; set; }

    public Subscription Subscription { get; set; } = null!;
}

public class UsageLog
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ModelUsed { get; set; } = string.Empty;
    public int PromptTokens { get; set; }
    public int CompletionTokens { get; set; }
    public DateTime CreatedAt { get; set; }
    public string TaskType { get; set; } = string.Empty;
}

public class Transaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class SocialAccount
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Platform { get; set; } = string.Empty;
    public string PageId { get; set; } = string.Empty;
    public string PageName { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
