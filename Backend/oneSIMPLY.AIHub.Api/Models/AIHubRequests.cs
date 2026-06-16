namespace oneSIMPLY.AIHub.Api.Models;

public record RegisterRequest(string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AIGenerateRequest(string Prompt, string TaskType);
public record CopywritingRequest(string ProductName, string Features, string Tone, string Formula, string? ImageUrl = null);
public record SocialPublishRequest(string Content, List<string> Channels, string? ImageUrl = null);
public record SaveSocialAccountRequest(string Platform, string PageId, string PageName, string AccessToken);
public record PurchaseRequest(int SubscriptionId);
public record BankNotification(string Description, decimal Amount, string Reference);
