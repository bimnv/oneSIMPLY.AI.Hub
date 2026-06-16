using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using oneSIMPLY.AIHub.Api.Data;

namespace oneSIMPLY.AIHub.Api.Controllers;

[ApiController]
[Route("api/subscriptions")]
public class SubscriptionController : ControllerBase
{
    private readonly AppDbContext _db;

    public SubscriptionController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetPlans()
    {
        var plans = await _db.Subscriptions
            .OrderBy(s => s.Price)
            .Select(s => new { s.Id, s.Name, s.Price, s.MaxRequests, s.DurationDays })
            .ToListAsync();

        return Ok(plans);
    }
}
