using HealthCare.API.Models;
using HealthCare.Application.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HealthCare.API.Filters;

/// <summary>
/// Bọc tất cả response thành công vào envelope chuẩn SRS §8.2: { success, data, meta }.
/// - Tự động unwrap <see cref="Result{T}"/> (lấy Data, đọc Succeeded).
/// - Tự động unwrap <see cref="PagedResult{T}"/> (data = Items, meta = page/pageSize/total).
/// - Response trống 200 → { success: true, data: null }.
/// - 204 NoContent và các response lỗi (4xx/5xx) giữ nguyên (đã được format ở nơi khác).
/// </summary>
public class ApiResponseFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Có exception → để ExceptionHandlingMiddleware xử lý.
        if (context.Exception != null || context.Canceled) return;

        switch (context.Result)
        {
            case ObjectResult obj when IsSuccessStatus(obj.StatusCode):
                context.Result = WrapObjectResult(obj);
                break;

            // Ok() / StatusCode(200) không có body.
            case StatusCodeResult sc when sc.StatusCode == StatusCodes.Status200OK:
                context.Result = new ObjectResult(new ApiResponse<object?> { Success = true, Data = null })
                {
                    StatusCode = StatusCodes.Status200OK
                };
                break;

            // EmptyResult (action trả về void/Task) với mặc định 200.
            case EmptyResult:
                context.Result = new ObjectResult(new ApiResponse<object?> { Success = true, Data = null })
                {
                    StatusCode = StatusCodes.Status200OK
                };
                break;
        }
    }

    private static IActionResult WrapObjectResult(ObjectResult obj)
    {
        var statusCode = obj.StatusCode ?? StatusCodes.Status200OK;
        var value = obj.Value;

        // Đã là envelope rồi thì không bọc lại.
        if (value is ApiResponse<object?> || value is ApiErrorResponse)
            return obj;

        object? data = value;
        ApiMeta? meta = null;

        // Result / Result<T>
        if (value is Result result)
        {
            if (!result.Succeeded)
            {
                return new ObjectResult(new ApiErrorResponse
                {
                    Success = false,
                    Error = new ApiError
                    {
                        Code = "ERROR",
                        Message = result.Errors.FirstOrDefault() ?? "Đã xảy ra lỗi.",
                        Details = result.Errors.Length > 0
                            ? result.Errors.Select(e => new ApiErrorDetail { Field = string.Empty, Message = e }).ToList()
                            : null
                    }
                })
                {
                    StatusCode = IsSuccessStatus(statusCode) ? StatusCodes.Status400BadRequest : statusCode
                };
            }

            // Lấy Data từ Result<T> qua reflection (T không biết trước).
            data = value.GetType().GetProperty(nameof(Result<object>.Data))?.GetValue(value);
        }
        else if (value != null && TryUnwrapPagedResult(value, out var items, out var pagedMeta))
        {
            data = items;
            meta = pagedMeta;
        }

        return new ObjectResult(new ApiResponse<object?>
        {
            Success = true,
            Data = data,
            Meta = meta
        })
        {
            StatusCode = statusCode
        };
    }

    private static bool TryUnwrapPagedResult(object value, out object? items, out ApiMeta? meta)
    {
        items = null;
        meta = null;

        var type = value.GetType();
        if (!type.IsGenericType || type.GetGenericTypeDefinition() != typeof(PagedResult<>))
            return false;

        items = type.GetProperty(nameof(PagedResult<object>.Items))!.GetValue(value);
        meta = new ApiMeta
        {
            Page = (int)type.GetProperty(nameof(PagedResult<object>.Page))!.GetValue(value)!,
            PageSize = (int)type.GetProperty(nameof(PagedResult<object>.PageSize))!.GetValue(value)!,
            Total = (int)type.GetProperty(nameof(PagedResult<object>.TotalCount))!.GetValue(value)!
        };
        return true;
    }

    private static bool IsSuccessStatus(int? statusCode)
        => statusCode is null or (>= 200 and < 300);
}
