namespace HealthCare.Application.Common.Interfaces;

// Bucket lưu trữ. DoctorLicenses tách riêng khỏi Documents vì ảnh CCHN chỉ admin
// được xem (signed URL 5 phút), không dùng chung ranh giới truy cập với tài liệu y tế bệnh nhân.
public enum StorageBucket
{
    Documents,
    DoctorLicenses
}

public interface IFileStorageService
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default, StorageBucket bucket = StorageBucket.Documents);
    Task<string> GetSignedUrlAsync(string storageKey, int expiryMinutes = 60, CancellationToken ct = default, StorageBucket bucket = StorageBucket.Documents);
    Task DeleteAsync(string storageKey, CancellationToken ct = default, StorageBucket bucket = StorageBucket.Documents);
}
