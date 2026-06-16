using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oneSIMPLY.AIHub.Api.Data;
using oneSIMPLY.AIHub.Api.Models;

namespace oneSIMPLY.AIHub.Api.Controllers;

[ApiController]
[Route("api/social")]
[Authorize]
public class SocialController : BaseController
{
    private readonly AppDbContext _db;

    public SocialController(AppDbContext db) => _db = db;

    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var accounts = await _db.SocialAccounts
            .Where(a => a.UserId == userId && a.IsActive)
            .Select(a => new
            {
                a.Id,
                a.Platform,
                a.PageId,
                a.PageName,
                TokenPreview = a.AccessToken.Length > 8 ? a.AccessToken.Substring(0, 8) + "..." : "***",
                a.UpdatedAt
            })
            .ToListAsync();

        return Ok(accounts);
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> SaveAccount([FromBody] SaveSocialAccountRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var platform = request.Platform.Trim().ToLowerInvariant();
        if (platform is not "facebook" and not "tiktok")
            return BadRequest("Platform phải là facebook hoặc tiktok.");

        if (string.IsNullOrWhiteSpace(request.PageId) || string.IsNullOrWhiteSpace(request.AccessToken))
            return BadRequest("Page ID và Access Token không được để trống.");

        var existing = await _db.SocialAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Platform == platform);

        if (existing == null)
        {
            existing = new SocialAccount
            {
                UserId = userId.Value,
                Platform = platform,
                PageId = request.PageId.Trim(),
                PageName = request.PageName.Trim(),
                AccessToken = request.AccessToken.Trim(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _db.SocialAccounts.Add(existing);
        }
        else
        {
            existing.PageId = request.PageId.Trim();
            existing.PageName = request.PageName.Trim();
            existing.AccessToken = request.AccessToken.Trim();
            existing.IsActive = true;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return Ok(new { Message = $"Đã lưu kết nối {platform} thành công!", existing.Platform, existing.PageName });
    }

    [HttpDelete("accounts/{platform}")]
    public async Task<IActionResult> DisconnectAccount(string platform)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var account = await _db.SocialAccounts
            .FirstOrDefaultAsync(a => a.UserId == userId && a.Platform == platform.ToLowerInvariant());

        if (account == null) return NotFound();

        account.IsActive = false;
        account.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(new { Message = "Đã ngắt kết nối tài khoản." });
    }

    [HttpPost("publish")]
    public async Task<IActionResult> Publish([FromBody] SocialPublishRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Nội dung bài viết không được để trống.");

        if (request.Channels == null || request.Channels.Count == 0)
            return BadRequest("Vui lòng chọn ít nhất một kênh đăng.");

        var publishedChannels = new List<string>();
        var missingAccounts = new List<string>();

        foreach (var channel in request.Channels.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var normalized = channel.ToLowerInvariant();
            if (normalized is not "facebook" and not "tiktok") continue;

            var account = await _db.SocialAccounts
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Platform == normalized && a.IsActive);

            if (account == null)
            {
                missingAccounts.Add(normalized);
                continue;
            }

            publishedChannels.Add(normalized);
        }

        if (missingAccounts.Count > 0)
            return BadRequest($"Chưa khai báo tài khoản: {string.Join(", ", missingAccounts)}. Vào menu 'Kết nối kênh đăng' để cấu hình Page ID & Access Token.");

        if (publishedChannels.Count == 0)
            return BadRequest("Kênh đăng không hợp lệ. Chọn Facebook hoặc TikTok.");

        _db.UsageLogs.Add(new UsageLog
        {
            UserId = userId.Value,
            ModelUsed = "social-push",
            PromptTokens = 0,
            CompletionTokens = request.Content.Length / 4,
            CreatedAt = DateTime.UtcNow,
            TaskType = $"push:{string.Join(",", publishedChannels)}" + (string.IsNullOrEmpty(request.ImageUrl) ? "" : ":with-image")
        });
        await _db.SaveChangesAsync();

        return Ok(new
        {
            Message = $"Đã gửi bài lên {string.Join(" & ", publishedChannels)} thành công!",
            Channels = publishedChannels,
            ImageUrl = request.ImageUrl,
            Note = "MVP: Bài + ảnh đã sẵn sàng. OAuth Graph API đăng thật sẽ bổ sung khi có App Facebook Developer."
        });
    }
}
