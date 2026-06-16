using HealthCare.Application.Ocr.DTOs;
using MediatR;

namespace HealthCare.Application.Ocr.Commands.ProcessOcr;

public record ProcessOcrCommand(Guid DocumentId) : IRequest<OcrResultDto>;
