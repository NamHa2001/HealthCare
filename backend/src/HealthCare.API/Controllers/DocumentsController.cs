using HealthCare.Application.MedicalHistory.Commands.DeleteDocument;
using HealthCare.Application.MedicalHistory.Commands.UploadDocument;
using HealthCare.Application.MedicalHistory.Queries.GetDocumentDownloadUrl;
using HealthCare.Application.Ocr.Commands.ProcessOcr;
using HealthCare.Application.Ocr.Queries.GetOcrResult;
using HealthCare.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCare.API.Controllers;

[Authorize]
public class DocumentsController : BaseController
{
    /// <summary>
    /// Upload tài liệu y tế (PDF/JPG/PNG/HEIC, max 10MB per SRS §4.2).
    /// Form fields: file (required), medicalVisitId (optional), documentType (optional).
    /// </summary>
    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<IActionResult> Upload(
        IFormFile file,
        [FromForm] Guid? medicalVisitId,
        [FromForm] string? documentType,
        CancellationToken ct)
    {
        DocumentType? docType = null;
        if (!string.IsNullOrEmpty(documentType) && Enum.TryParse<DocumentType>(documentType, ignoreCase: true, out var parsed))
            docType = parsed;

        var command = new UploadDocumentCommand
        {
            FileStream = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            MedicalVisitId = medicalVisitId,
            DocumentType = docType
        };

        var result = await Sender.Send(command, ct);
        return CreatedAtAction(nameof(GetDownloadUrl), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> GetDownloadUrl(Guid id, CancellationToken ct)
        => Ok(await Sender.Send(new GetDocumentDownloadUrlQuery(id), ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDocument(Guid id, CancellationToken ct)
    {
        await Sender.Send(new DeleteDocumentCommand(id), ct);
        return NoContent();
    }

    /// <summary>
    /// Kích hoạt OCR cho tài liệu (SRS §12). Trả ngay kết quả sau khi xử lý.
    /// </summary>
    [HttpPost("{id:guid}/ocr")]
    public async Task<IActionResult> ProcessOcr(Guid id, CancellationToken ct)
        => Ok(await Sender.Send(new ProcessOcrCommand(id), ct));

    /// <summary>
    /// Lấy kết quả OCR đã xử lý trước đó.
    /// </summary>
    [HttpGet("{id:guid}/ocr")]
    public async Task<IActionResult> GetOcrResult(Guid id, CancellationToken ct)
        => Ok(await Sender.Send(new GetOcrResultQuery(id), ct));
}
