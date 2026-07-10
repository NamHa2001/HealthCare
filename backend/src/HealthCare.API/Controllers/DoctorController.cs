using HealthCare.Application.Doctors.Commands.RegisterDoctor;
using HealthCare.Application.Doctors.DTOs;
using HealthCare.Application.Doctors.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class DoctorController : BaseController
{
    /// <summary>Nộp hồ sơ đăng ký bác sĩ (multipart: thông tin + ảnh CCHN). Nộp lại được nếu bị từ chối.</summary>
    [HttpPost("register")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> Register(
        [FromForm] string licenseNumber,
        [FromForm] string specialty,
        [FromForm] string workplace,
        [FromForm] List<IFormFile> licenseFiles,
        CancellationToken ct)
    {
        var files = licenseFiles
            .Select(f => new UploadedFile(f.OpenReadStream(), f.FileName, f.ContentType, f.Length))
            .ToList();

        var command = new RegisterDoctorCommand(licenseNumber, specialty, workplace, files);
        return Ok(await Sender.Send(command, ct));
    }

    /// <summary>Trạng thái hồ sơ bác sĩ của tôi (null nếu chưa đăng ký).</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
        => Ok(await Sender.Send(new GetMyDoctorProfileQuery(), ct));
}
