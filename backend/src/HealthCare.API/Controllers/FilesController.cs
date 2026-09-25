using HealthCare.Infrastructure.Services.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

/// <summary>
/// Phục vụ link tải có chữ ký của <see cref="LocalFileStorageService"/> (chỉ khi Storage:Provider = Local).
/// Không cần JWT — quyền truy cập đã được kiểm lúc cấp link (GetDocumentDownloadUrl, OCR, admin xem CCHN);
/// link tự hết hạn. Mọi lỗi trả 404 đồng nhất, không tiết lộ file có tồn tại hay không.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/Files")]
public class FilesController(IConfiguration config, LocalFileStorageService storage) : ControllerBase
{
    [HttpGet("{bucket}/{**key}")]
    public IActionResult Get(string bucket, string key, [FromQuery] long exp, [FromQuery] string? sig)
    {
        if (!string.Equals(config["Storage:Provider"], "Local", StringComparison.OrdinalIgnoreCase))
            return NotFound();

        var file = storage.ResolveSignedFile(bucket, key, exp, sig ?? string.Empty);
        if (file is null) return NotFound();

        Response.Headers.CacheControl = "private, no-store";
        return PhysicalFile(file.Value.FullPath, file.Value.ContentType, enableRangeProcessing: true);
    }
}
