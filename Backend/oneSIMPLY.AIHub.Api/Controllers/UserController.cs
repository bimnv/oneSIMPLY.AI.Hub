using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oneSIMPLY.AIHub.Api.Data;

namespace oneSIMPLY.AIHub.Api.Controllers;

[ApiController]
[Authorize]
public class UserController : BaseController
{
    private readonly AppDbContext _db;

    public UserController(AppDbContext db) => _db = db;

    [HttpGet("api/user/profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var activeSub = await _db.UserSubscriptions
            .Include(us => us.Subscription)
            .FirstOrDefaultAsync(us => us.UserId == userId && us.IsActive && us.EndDate > DateTime.UtcNow);

        return Ok(new
        {
            UserId = userId,
            Email = GetUserEmail() ?? "",
            PlanName = activeSub?.Subscription.Name ?? "Chưa có gói",
            RemainingRequests = activeSub?.RemainingRequests ?? 0,
            EndDate = activeSub?.EndDate,
            IsActive = activeSub != null
        });
    }

    [HttpGet("api/usage/history")]
    public async Task<IActionResult> GetHistory()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var logs = await _db.UsageLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .Take(50)
            .Select(l => new
            {
                l.Id,
                l.ModelUsed,
                l.TaskType,
                l.PromptTokens,
                l.CompletionTokens,
                l.CreatedAt
            })
            .ToListAsync();

        return Ok(logs);
    }
}
