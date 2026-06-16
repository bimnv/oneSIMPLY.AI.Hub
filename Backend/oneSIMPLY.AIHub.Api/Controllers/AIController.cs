using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oneSIMPLY.AIHub.Api.Data;
using oneSIMPLY.AIHub.Api.Models;
using oneSIMPLY.AIHub.Api.Services;

namespace oneSIMPLY.AIHub.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AIController : BaseController
{
    private readonly AppDbContext _db;
    private readonly ILLMService _llmService;
    private readonly IConfiguration _config;

    public AIController(AppDbContext db, ILLMService llmService, IConfiguration config)
    {
        _db = db;
        _llmService = llmService;
        _config = config;
    }

    [HttpPost("copywriting")]
    public async Task<IActionResult> Copywriting([FromBody] CopywritingRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.ProductName))
            return BadRequest("Vui lòng nhập tên sản phẩm.");

        var activeSub = await _db.UserSubscriptions
            .Include(us => us.Subscription)
            .FirstOrDefaultAsync(us => us.UserId == userId && us.IsActive && us.EndDate > DateTime.UtcNow);

        if (activeSub == null || activeSub.RemainingRequests <= 0)
            return StatusCode(403, new { Error = "Hạn mức tài khoản đã hết hoặc chưa mua gói cước. Vui lòng nạp tiền!" });

        var wrappedPrompt = PromptTemplateService.BuildCopywritingPrompt(
            request.ProductName, request.Features, request.Tone, request.Formula, request.ImageUrl);

        string targetModel = "deepseek-chat";
        string? aiResult;

        if (string.IsNullOrWhiteSpace(_config["AI:DeepSeekKey"]))
        {
            aiResult = PromptTemplateService.BuildDemoCopywritingPost(request);
            targetModel = "demo-local";
        }
        else
        {
            aiResult = await _llmService.CallLLMAsync(wrappedPrompt, targetModel);
        }

        if (string.IsNullOrEmpty(aiResult))
            return Problem("Có lỗi xảy ra khi kết nối máy chủ AI đối tác.");

        activeSub.RemainingRequests -= 1;
        _db.UsageLogs.Add(new UsageLog
        {
            UserId = userId.Value,
            ModelUsed = targetModel,
            PromptTokens = wrappedPrompt.Length / 4,
            CompletionTokens = aiResult.Length / 4,
            CreatedAt = DateTime.UtcNow,
            TaskType = "copywriting"
        });
        await _db.SaveChangesAsync();

        return Ok(new { Result = aiResult, ModelUsed = targetModel, RemainingRequests = activeSub.RemainingRequests, ImageUrl = request.ImageUrl });
    }

    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] AIGenerateRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var activeSub = await _db.UserSubscriptions
            .Include(us => us.Subscription)
            .FirstOrDefaultAsync(us => us.UserId == userId && us.IsActive && us.EndDate > DateTime.UtcNow);

        if (activeSub == null || activeSub.RemainingRequests <= 0)
            return StatusCode(403, new { Error = "Hạn mức tài khoản đã hết hoặc chưa mua gói cước. Vui lòng nạp tiền!" });

        string targetModel = "deepseek-chat";
        if (request.TaskType.Equals("strategy", StringComparison.OrdinalIgnoreCase)
            || request.TaskType.Equals("analytics", StringComparison.OrdinalIgnoreCase))
        {
            targetModel = "gpt-4o-mini";
        }

        var aiResult = await _llmService.CallLLMAsync(request.Prompt, targetModel);
        if (string.IsNullOrEmpty(aiResult))
            return Problem("Có lỗi xảy ra khi kết nối máy chủ AI đối tác.");

        activeSub.RemainingRequests -= 1;
        _db.UsageLogs.Add(new UsageLog
        {
            UserId = userId.Value,
            ModelUsed = targetModel,
            PromptTokens = request.Prompt.Length / 4,
            CompletionTokens = aiResult.Length / 4,
            CreatedAt = DateTime.UtcNow,
            TaskType = request.TaskType
        });
        await _db.SaveChangesAsync();

        return Ok(new { Result = aiResult, ModelUsed = targetModel, RemainingRequests = activeSub.RemainingRequests });
    }
}
