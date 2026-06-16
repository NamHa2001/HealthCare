using HealthCare.Application.Ocr.DTOs;
using MediatR;

namespace HealthCare.Application.Ocr.Queries.GetOcrResult;

public record GetOcrResultQuery(Guid DocumentId) : IRequest<OcrResultDto>;
