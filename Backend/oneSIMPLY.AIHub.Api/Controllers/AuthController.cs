using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oneSIMPLY.AIHub.Api.Data;
using oneSIMPLY.AIHub.Api.Models;
using oneSIMPLY.AIHub.Api.Services;

namespace oneSIMPLY.AIHub.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AppDbContext _db;

    public AuthController(IAuthService authService, AppDbContext db)
    {
        _authService = authService;
        _db = db;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return BadRequest("Email này đã được sử dụng trên hệ thống!");

        var user = await _authService.RegisterAsync(request.Email, request.Password);

        var freeSub = await _db.Subscriptions.FirstOrDefaultAsync(s => s.Name == "Free Trial");
        if (freeSub == null)
        {
            freeSub = new Subscription { Name = "Free Trial", Price = 0, MaxRequests = 10, DurationDays = 30 };
            _db.Subscriptions.Add(freeSub);
            await _db.SaveChangesAsync();
        }

        _db.UserSubscriptions.Add(new UserSubscription
        {
            UserId = user.Id,
            SubscriptionId = freeSub.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(freeSub.DurationDays),
            IsActive = true,
            RemainingRequests = freeSub.MaxRequests
        });
        await _db.SaveChangesAsync();

        return Ok(new { Message = "Đăng ký tài khoản thành công!", UserId = user.Id });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var token = await _authService.LoginAsync(request.Email, request.Password);
        if (token == null)
            return Unauthorized();

        return Ok(new { Token = token, Email = request.Email });
    }
}
