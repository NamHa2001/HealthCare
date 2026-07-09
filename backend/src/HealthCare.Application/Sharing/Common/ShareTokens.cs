using System.Security.Cryptography;
using System.Text;

namespace HealthCare.Application.Sharing.Common;

public static class ShareTokens
{
    /// <summary>Token 32-byte random (256-bit) dạng hex — chỉ trả về client 1 lần, DB lưu hash.</summary>
    public static string Generate() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();

    public static string Hash(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();
}
