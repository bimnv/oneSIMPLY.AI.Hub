using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace oneSIMPLY.AIHub.Api.Controllers;

[ApiController]
[Route("api/media")]
[Authorize]
public class MediaController : BaseController
{
    private readonly IWebHostEnvironment _env;

    public MediaController(IWebHostEnvironment env) => _env = env;

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        if (!Request.HasFormContentType)
            return BadRequest("Request phải là multipart/form-data.");

        var form = await Request.ReadFormAsync();
        var file = form.Files.GetFile("image");
        if (file == null || file.Length == 0)
            return BadRequest("Vui lòng chọn ảnh sản phẩm.");

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest("Ảnh tối đa 5MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not ".jpg" and not ".jpeg" and not ".png" and not ".webp")
            return BadRequest("Chỉ hỗ trợ JPG, PNG, WEBP.");

        var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads", userId.ToString()!);
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadsDir, fileName);
        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var imageUrl = $"/uploads/{userId}/{fileName}";
        return Ok(new { ImageUrl = imageUrl, FileName = fileName });
    }
}
