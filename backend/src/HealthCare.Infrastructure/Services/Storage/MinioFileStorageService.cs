using HealthCare.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace HealthCare.Infrastructure.Services.Storage;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minio;
    private readonly string _documentsBucket;
    private readonly string _doctorLicensesBucket;

    public MinioFileStorageService(IMinioClient minio, IConfiguration config)
    {
        _minio = minio;
        _documentsBucket = config["Storage:BucketDocuments"] ?? config["Storage:BucketName"] ?? "health-documents";
        _doctorLicensesBucket = config["Storage:BucketDoctorLicenses"] ?? "doctor-licenses";
    }

    private string ResolveBucket(StorageBucket bucket) => bucket switch
    {
        StorageBucket.DoctorLicenses => _doctorLicensesBucket,
        _ => _documentsBucket,
    };

    public async Task<string> UploadAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken ct = default, StorageBucket bucket = StorageBucket.Documents)
    {
        var bucketName = ResolveBucket(bucket);
        await EnsureBucketExistsAsync(bucketName, ct);

        var ext = Path.GetExtension(fileName);
        var storageKey = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}{ext}";

        var size = fileStream.CanSeek ? fileStream.Length : -1;

        var args = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(storageKey)
            .WithStreamData(fileStream)
            .WithObjectSize(size)
            .WithContentType(contentType);

        await _minio.PutObjectAsync(args, ct);
        return storageKey;
    }

    public async Task<string> GetSignedUrlAsync(
        string storageKey, int expiryMinutes = 60, CancellationToken ct = default, StorageBucket bucket = StorageBucket.Documents)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(ResolveBucket(bucket))
            .WithObject(storageKey)
            .WithExpiry(expiryMinutes * 60);

        return await _minio.PresignedGetObjectAsync(args);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken ct = default, StorageBucket bucket = StorageBucket.Documents)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(ResolveBucket(bucket))
            .WithObject(storageKey);

        await _minio.RemoveObjectAsync(args, ct);
    }

    private async Task EnsureBucketExistsAsync(string bucketName, CancellationToken ct)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(bucketName);
        if (!await _minio.BucketExistsAsync(existsArgs, ct))
        {
            var makeArgs = new MakeBucketArgs().WithBucket(bucketName);
            await _minio.MakeBucketAsync(makeArgs, ct);
        }
    }
}
