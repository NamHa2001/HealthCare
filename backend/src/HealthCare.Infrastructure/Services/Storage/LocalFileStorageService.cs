using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using HealthCare.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace HealthCare.Infrastructure.Services.Storage;

/// <summary>
/// Lưu file trên ổ đĩa của máy chủ — dùng cho host không có MinIO/S3 (vd. MonsterASP gói free).
/// Bật bằng <c>Storage:Provider = Local</c>. File nằm ở <c>{Storage:LocalPath}/{bucket}/yyyy/MM/{guid}{ext}</c>
/// (mặc định <c>uploads</c> dưới content root — trên host phải khai báo thư mục này là Protected
/// để deploy không xóa mất).
///
/// Thay cho presigned URL của MinIO: link tải có dạng <c>/api/Files/{bucket}/{key}?exp=&amp;sig=</c>,
/// chữ ký HMAC-SHA256 trên (bucket, key, exp) — hết hạn hoặc sai chữ ký đều trả 404 (FilesController).
/// </summary>
public partial class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private readonly string? _publicBaseUrl;
    private readonly byte[] _signingKey;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly FileExtensionContentTypeProvider ContentTypes = new();

    public LocalFileStorageService(IConfiguration config, IHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        var configuredPath = config["Storage:LocalPath"] ?? "uploads";
        _rootPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(env.ContentRootPath, configuredPath);

        _publicBaseUrl = config["Storage:PublicBaseUrl"]?.TrimEnd('/');
        _httpContextAccessor = httpContextAccessor;

        // Không bắt buộc thêm secret mới: nếu thiếu Storage:SigningKey thì dẫn xuất từ Encryption:Key
        // (vốn đã bắt buộc có). Nhãn "file-url|" để khóa ký link khác hẳn khóa mã hóa dữ liệu.
        var secret = config["Storage:SigningKey"]
            ?? config["Encryption:Key"]
            ?? throw new InvalidOperationException("Thiếu Storage:SigningKey hoặc Encryption:Key để ký link tải file.");
        _signingKey = SHA256.HashData(Encoding.UTF8.GetBytes("file-url|" + secret));
    }

    public async Task<string> UploadAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken ct = default, StorageBucket bucket = StorageBucket.Documents)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (!SafeExtension().IsMatch(ext)) ext = string.Empty;

        var storageKey = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}{ext}";
        var fullPath = GetFullPath(bucket, storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, useAsync: true);
        await fileStream.CopyToAsync(output, ct);

        return storageKey;
    }

    public Task<string> GetSignedUrlAsync(
        string storageKey, int expiryMinutes = 60, CancellationToken ct = default, StorageBucket bucket = StorageBucket.Documents)
    {
        var exp = DateTimeOffset.UtcNow.AddMinutes(expiryMinutes).ToUnixTimeSeconds();
        var sig = Sign(bucket, storageKey, exp);
        var url = $"{GetBaseUrl()}/api/Files/{BucketSegment(bucket)}/{storageKey}?exp={exp}&sig={sig}";
        return Task.FromResult(url);
    }

    public Task DeleteAsync(string storageKey, CancellationToken ct = default, StorageBucket bucket = StorageBucket.Documents)
    {
        if (IsValidKey(storageKey))
        {
            var fullPath = GetFullPath(bucket, storageKey);
            if (File.Exists(fullPath)) File.Delete(fullPath);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// Kiểm tra link tải (dùng bởi FilesController). Trả null nếu bucket/key sai định dạng, hết hạn,
    /// sai chữ ký hoặc file không tồn tại — controller trả 404 đồng nhất cho mọi trường hợp.
    /// </summary>
    public (string FullPath, string ContentType)? ResolveSignedFile(string bucketSegment, string storageKey, long exp, string sig)
    {
        if (!TryParseBucket(bucketSegment, out var bucket)) return null;
        if (!IsValidKey(storageKey)) return null;
        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > exp) return null;

        var expected = Encoding.ASCII.GetBytes(Sign(bucket, storageKey, exp));
        var actual = Encoding.ASCII.GetBytes(sig ?? string.Empty);
        if (!CryptographicOperations.FixedTimeEquals(expected, actual)) return null;

        var fullPath = GetFullPath(bucket, storageKey);
        if (!File.Exists(fullPath)) return null;

        if (!ContentTypes.TryGetContentType(fullPath, out var contentType))
            contentType = "application/octet-stream";

        return (fullPath, contentType);
    }

    private string Sign(StorageBucket bucket, string storageKey, long exp)
    {
        var payload = Encoding.UTF8.GetBytes($"{BucketSegment(bucket)}|{storageKey}|{exp}");
        return Base64UrlEncode(HMACSHA256.HashData(_signingKey, payload));
    }

    private string GetBaseUrl()
    {
        if (!string.IsNullOrEmpty(_publicBaseUrl)) return _publicBaseUrl;

        var request = _httpContextAccessor.HttpContext?.Request
            ?? throw new InvalidOperationException("Thiếu Storage:PublicBaseUrl và không có HTTP request để suy ra địa chỉ API.");
        return $"{request.Scheme}://{request.Host}{request.PathBase}";
    }

    private string GetFullPath(StorageBucket bucket, string storageKey) =>
        Path.Combine(_rootPath, BucketSegment(bucket), storageKey.Replace('/', Path.DirectorySeparatorChar));

    private static string BucketSegment(StorageBucket bucket) => bucket switch
    {
        StorageBucket.DoctorLicenses => "doctor-licenses",
        _ => "documents",
    };

    private static bool TryParseBucket(string segment, out StorageBucket bucket)
    {
        switch (segment)
        {
            case "documents": bucket = StorageBucket.Documents; return true;
            case "doctor-licenses": bucket = StorageBucket.DoctorLicenses; return true;
            default: bucket = default; return false;
        }
    }

    // Chỉ chấp nhận đúng định dạng key do UploadAsync sinh ra — chặn path traversal (../) từ URL.
    private static bool IsValidKey(string storageKey) => StorageKeyPattern().IsMatch(storageKey);

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    [GeneratedRegex(@"^\d{4}/\d{2}/[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}(\.[a-z0-9]{1,10})?$")]
    private static partial Regex StorageKeyPattern();

    [GeneratedRegex(@"^\.[a-z0-9]{1,10}$")]
    private static partial Regex SafeExtension();
}
