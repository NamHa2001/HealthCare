namespace HealthCare.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default);
    Task<string> GetSignedUrlAsync(string storageKey, int expiryMinutes = 60, CancellationToken ct = default);
    Task DeleteAsync(string storageKey, CancellationToken ct = default);
}
