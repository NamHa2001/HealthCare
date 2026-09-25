namespace HealthCare.Infrastructure.Services.Auth;

/// <summary>
/// Đọc file PEM của JWT. Đường dẫn tuyệt đối dùng nguyên; đường dẫn tương đối thử theo thư mục
/// làm việc hiện tại (dev: <c>dotnet run</c> trong thư mục project) rồi tới thư mục chứa app
/// (host IIS: thư mục đã publish) — để cùng một cấu hình <c>keys/jwt_private.pem</c> chạy được cả hai nơi.
/// </summary>
public static class PemKeyLoader
{
    public static string ReadPem(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
            return File.ReadAllText(configuredPath);

        var candidates = new[]
        {
            Path.GetFullPath(configuredPath),
            Path.Combine(AppContext.BaseDirectory, configuredPath),
        };

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return File.ReadAllText(path);
        }

        throw new FileNotFoundException(
            $"Không tìm thấy file key JWT '{configuredPath}'. Đã thử: {string.Join(", ", candidates)}");
    }
}
