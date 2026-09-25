namespace HealthCare.Infrastructure.BackgroundJobs;

/// <summary>
/// Khóa phân tán cho background jobs khi chạy nhiều instance API (WORKLOG 2026-07-10, mục 3).
/// Một hàng/job. Claim bằng UPDATE có điều kiện — SQL Server tự serialize UPDATE trên cùng 1 hàng
/// nên chỉ đúng 1 instance thắng race, không cần sp_getapplock.
/// </summary>
public class JobLock
{
    public string JobName { get; set; } = null!;
    public DateTime LockedUntil { get; set; }
    public string? LockedBy { get; set; }
}
