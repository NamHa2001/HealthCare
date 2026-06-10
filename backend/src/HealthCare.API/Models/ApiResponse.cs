using System.Text.Json.Serialization;

namespace HealthCare.API.Models;

/// <summary>
/// Envelope thành công theo SRS §8.2: { success, data, meta }
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; init; } = true;

    public T? Data { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ApiMeta? Meta { get; init; }
}

/// <summary>
/// Thông tin phân trang. Chỉ xuất hiện với response danh sách có phân trang.
/// </summary>
public class ApiMeta
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
}

/// <summary>
/// Envelope lỗi theo SRS §8.2: { success: false, error: { code, message, details } }
/// </summary>
public class ApiErrorResponse
{
    public bool Success { get; init; }
    public ApiError Error { get; init; } = default!;
}

public class ApiError
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public List<ApiErrorDetail>? Details { get; init; }
}

public class ApiErrorDetail
{
    public string Field { get; init; } = default!;
    public string Message { get; init; } = default!;
}
