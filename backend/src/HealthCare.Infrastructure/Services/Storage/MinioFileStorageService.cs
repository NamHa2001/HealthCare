using HealthCare.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Minio;
using Minio.DataModel.Args;

namespace HealthCare.Infrastructure.Services.Storage;

public class MinioFileStorageService : IFileStorageService
{
    private readonly IMinioClient _minio;
    private readonly string _bucket;

    public MinioFileStorageService(IMinioClient minio, IConfiguration config)
    {
        _minio = minio;
        _bucket = config["Storage:BucketDocuments"] ?? config["Storage:BucketName"] ?? "health-documents";
    }

    public async Task<string> UploadAsync(
        Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
    {
        await EnsureBucketExistsAsync(ct);

        var ext = Path.GetExtension(fileName);
        var storageKey = $"{DateTime.UtcNow:yyyy/MM}/{Guid.NewGuid()}{ext}";

        var size = fileStream.CanSeek ? fileStream.Length : -1;

        var args = new PutObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storageKey)
            .WithStreamData(fileStream)
            .WithObjectSize(size)
            .WithContentType(contentType);

        await _minio.PutObjectAsync(args, ct);
        return storageKey;
    }

    public async Task<string> GetSignedUrlAsync(
        string storageKey, int expiryMinutes = 60, CancellationToken ct = default)
    {
        var args = new PresignedGetObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storageKey)
            .WithExpiry(expiryMinutes * 60);

        return await _minio.PresignedGetObjectAsync(args);
    }

    public async Task DeleteAsync(string storageKey, CancellationToken ct = default)
    {
        var args = new RemoveObjectArgs()
            .WithBucket(_bucket)
            .WithObject(storageKey);

        await _minio.RemoveObjectAsync(args, ct);
    }

    private async Task EnsureBucketExistsAsync(CancellationToken ct)
    {
        var existsArgs = new BucketExistsArgs().WithBucket(_bucket);
        if (!await _minio.BucketExistsAsync(existsArgs, ct))
        {
            var makeArgs = new MakeBucketArgs().WithBucket(_bucket);
            await _minio.MakeBucketAsync(makeArgs, ct);
        }
    }
}
