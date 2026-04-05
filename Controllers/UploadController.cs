using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.API.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { message = "Không tìm thấy file tải lên." });
        }

        // Tạo thư mục nếu chưa có
        var uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "rooms");
        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        // Đổi tên file để tránh trùng lặp
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadFolder, fileName);

        // Lưu file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Trả về url có thể truy cập
        var url = $"/uploads/rooms/{fileName}";
        
        // Trong môi trường development trả về absolute url dễ xem hơn
        var fullUrl = $"{Request.Scheme}://{Request.Host}{url}";

        return Ok(new { url = fullUrl });
    }
}
