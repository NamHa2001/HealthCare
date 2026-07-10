using HealthCare.Application.Doctors.Commands.RegisterDoctor;
using HealthCare.Application.Doctors.Dashboard.Commands;
using HealthCare.Application.Doctors.Dashboard.Queries;
using HealthCare.Application.Doctors.DTOs;
using HealthCare.Application.Doctors.Links.Commands;
using HealthCare.Application.Doctors.Links.Queries;
using HealthCare.Application.Doctors.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

public record InvitePatientRequest(string PatientEmail);

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

    /// <summary>Danh bạ bác sĩ đã xác minh — bệnh nhân tìm để mời.</summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? q, CancellationToken ct)
        => Ok(await Sender.Send(new SearchDoctorsQuery(q), ct));

    // ─── Lời mời của bác sĩ (yêu cầu role doctor) ─────────────────────────────

    [Authorize(Roles = "doctor")]
    [HttpPost("invitations")]
    public async Task<IActionResult> InvitePatient([FromBody] InvitePatientRequest body, CancellationToken ct)
        => Ok(await Sender.Send(new InvitePatientCommand(body.PatientEmail), ct));

    [Authorize(Roles = "doctor")]
    [HttpGet("invitations")]
    public async Task<IActionResult> GetInvitations([FromQuery] string? status, CancellationToken ct)
        => Ok(await Sender.Send(new GetDoctorInvitationsQuery(status), ct));

    [Authorize(Roles = "doctor")]
    [HttpDelete("invitations/{id:guid}")]
    public async Task<IActionResult> RevokeInvitation(Guid id, CancellationToken ct)
    {
        await Sender.Send(new RevokeDoctorLinkCommand(id), ct);
        return NoContent();
    }

    // ─── Doctor Dashboard (DOCTOR_PORTAL.md §6) — read-only, guard theo consent ──

    [Authorize(Roles = "doctor")]
    [HttpGet("patients")]
    public async Task<IActionResult> GetMyPatients(CancellationToken ct)
        => Ok(await Sender.Send(new GetMyPatientsQuery(), ct));

    [Authorize(Roles = "doctor")]
    [HttpGet("patients/{profileId:guid}/summary")]
    public async Task<IActionResult> GetPatientSummary(Guid profileId, CancellationToken ct)
        => Ok(await Sender.Send(new GetPatientSummaryQuery(profileId), ct));

    [Authorize(Roles = "doctor")]
    [HttpGet("patients/{profileId:guid}/measurements")]
    public async Task<IActionResult> GetPatientMeasurements(Guid profileId, CancellationToken ct)
        => Ok(await Sender.Send(new GetPatientMeasurementsQuery(profileId), ct));

    [Authorize(Roles = "doctor")]
    [HttpGet("patients/{profileId:guid}/blood-pressure")]
    public async Task<IActionResult> GetPatientBloodPressure(Guid profileId, CancellationToken ct)
        => Ok(await Sender.Send(new GetPatientBloodPressureQuery(profileId), ct));

    [Authorize(Roles = "doctor")]
    [HttpGet("patients/{profileId:guid}/medical-visits")]
    public async Task<IActionResult> GetPatientVisits(Guid profileId, CancellationToken ct)
        => Ok(await Sender.Send(new GetPatientVisitsQuery(profileId), ct));

    [Authorize(Roles = "doctor")]
    [HttpGet("patients/{profileId:guid}/medications")]
    public async Task<IActionResult> GetPatientMedications(Guid profileId, CancellationToken ct)
        => Ok(await Sender.Send(new GetPatientMedicationsQuery(profileId), ct));

    [Authorize(Roles = "doctor")]
    [HttpGet("patients/{profileId:guid}/vaccines")]
    public async Task<IActionResult> GetPatientVaccines(Guid profileId, CancellationToken ct)
        => Ok(await Sender.Send(new GetPatientVaccinesQuery(profileId), ct));

    [Authorize(Roles = "doctor")]
    [HttpPost("patients/{profileId:guid}/alerts/{alertId:guid}/acknowledge")]
    public async Task<IActionResult> AcknowledgeAlert(Guid profileId, Guid alertId, CancellationToken ct)
    {
        await Sender.Send(new AcknowledgeAlertAsDoctorCommand(profileId, alertId), ct);
        return NoContent();
    }
}
