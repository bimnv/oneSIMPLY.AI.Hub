using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oneSIMPLY.AIHub.Api.Data;
using oneSIMPLY.AIHub.Api.Models;
using oneSIMPLY.AIHub.Api.Services;

namespace oneSIMPLY.AIHub.Api.Controllers;

[ApiController]
[Route("api/payment")]
public class PaymentController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;

    public PaymentController(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    [HttpPost("generate-qr")]
    [Authorize]
    public async Task<IActionResult> GenerateQr([FromBody] PurchaseRequest request)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var subscription = await _db.Subscriptions.FindAsync(request.SubscriptionId);
        if (subscription == null)
            return BadRequest("Gói cước không tồn tại!");

        var nganHang = _config["VietQR:NganHang"] ?? "MB";
        var soTaiKhoan = _config["VietQR:SoTaiKhoan"] ?? "";
        var tenChuThe = _config["VietQR:TenChuThe"] ?? "";

        string transactionCode = $"OSAI{userId}S{subscription.Id}T{DateTime.UtcNow.Ticks % 100000}";

        _db.Transactions.Add(new Transaction
        {
            UserId = userId.Value,
            Amount = subscription.Price,
            Status = "Pending",
            Code = transactionCode,
            CreatedAt = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        string qrUrl = $"https://img.vietqr.io/image/{nganHang}-{soTaiKhoan}-compact2.png?amount={subscription.Price}&addInfo={transactionCode}&accountName={tenChuThe}";

        return Ok(new { QRUrl = qrUrl, TransactionCode = transactionCode, Amount = subscription.Price, SubscriptionName = subscription.Name });
    }

    [HttpGet("status/{transactionCode}")]
    [Authorize]
    public async Task<IActionResult> GetStatus(string transactionCode)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Code == transactionCode && t.UserId == userId);

        if (transaction == null)
            return NotFound(new { Error = "Không tìm thấy giao dịch." });

        return Ok(new { transaction.Status, transaction.Code, transaction.Amount });
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook([FromBody] BankNotification webhookData)
    {
        var transaction = await _db.Transactions
            .FirstOrDefaultAsync(t => t.Code == webhookData.Description && t.Status == "Pending");

        if (transaction == null)
            return BadRequest("Giao dịch không hợp lệ hoặc đã được xử lý.");

        transaction.Status = "Success";

        if (!PaymentHelper.TryParseTransactionCode(transaction.Code, out int userId, out int subId))
            return BadRequest("Mã giao dịch không hợp lệ.");

        var sub = await _db.Subscriptions.FindAsync(subId);
        if (sub != null)
        {
            var userSub = await _db.UserSubscriptions.FirstOrDefaultAsync(us => us.UserId == userId && us.IsActive);
            if (userSub != null)
            {
                userSub.EndDate = DateTime.UtcNow.AddDays(sub.DurationDays);
                userSub.RemainingRequests += sub.MaxRequests;
            }
            else
            {
                _db.UserSubscriptions.Add(new UserSubscription
                {
                    UserId = userId,
                    SubscriptionId = sub.Id,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(sub.DurationDays),
                    IsActive = true,
                    RemainingRequests = sub.MaxRequests
                });
            }
            await _db.SaveChangesAsync();
        }

        return Ok(new { Status = "Success", Message = "Đã kích hoạt gói cước tự động thành công!" });
    }
}
