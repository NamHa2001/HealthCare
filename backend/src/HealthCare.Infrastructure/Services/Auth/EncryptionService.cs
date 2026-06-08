using System.Security.Cryptography;
using System.Text;
using HealthCare.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HealthCare.Infrastructure.Services.Auth;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration config)
    {
        var raw = config["Encryption:Key"]
            ?? throw new InvalidOperationException("Encryption:Key chưa được cấu hình.");
        _key = Convert.FromBase64String(raw);
    }

    public string Encrypt(string plaintext)
    {
        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize];
        RandomNumberGenerator.Fill(nonce);

        var input = Encoding.UTF8.GetBytes(plaintext);
        var cipher = new byte[input.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize];

        aes.Encrypt(nonce, input, cipher, tag);

        var result = new byte[nonce.Length + cipher.Length + tag.Length];
        nonce.CopyTo(result, 0);
        cipher.CopyTo(result, nonce.Length);
        tag.CopyTo(result, nonce.Length + cipher.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string ciphertext)
    {
        var data = Convert.FromBase64String(ciphertext);
        var nonceSize = AesGcm.NonceByteSizes.MaxSize;
        var tagSize = AesGcm.TagByteSizes.MaxSize;
        var cipherSize = data.Length - nonceSize - tagSize;

        var nonce = data[..nonceSize];
        var cipher = data[nonceSize..(nonceSize + cipherSize)];
        var tag = data[(nonceSize + cipherSize)..];

        using var aes = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        var plain = new byte[cipherSize];
        aes.Decrypt(nonce, cipher, tag, plain);

        return Encoding.UTF8.GetString(plain);
    }
}
