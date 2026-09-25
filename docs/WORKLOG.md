# NHẬT KÝ PHÁT TRIỂN — Health+
---

## Phiên 2026-07-09 → 2026-07-10 — Doctor Portal MVP hoàn thành

### Đã làm

| Sprint | Nội dung | Commit |
|---|---|---|
| Sửa bug nền tảng | Link email thiếu domain (`App:FrontendUrl`), gán role `user` khi đăng ký + backfill 10 tài khoản cũ, EF query filter warning, nâng AutoMapper 13.0.1 → 15.1.1 (vá DoS) | `d7e1b64` |
| A — ShareGrant | Chia sẻ hồ sơ qua link/QR có thời hạn (1h/24h/7d), token hash SHA-256, rate limit 30 req/phút/IP, 404 đồng nhất chống dò token, max 3 link active/hồ sơ. Trang `/sharing` + trang public `/shared/:token` | `968b47e` |
| B1 — Xác minh bác sĩ | `DoctorProfile` + role `doctor`, upload ảnh CCHN lên MinIO (signed URL 5 phút cho admin), admin duyệt/từ chối/tạm ngưng tại `/admin/doctor-verifications`, trang `/doctor-registration` | `a35606a` |
| B2 — Liên kết + consent | `PatientDoctorLink` mời 2 chiều. **Consent record bất biến**: snapshot nguyên văn + IP + user-agent + người bấm (NĐ 13/2023). Bác sĩ mời → bệnh nhân accept kèm chọn scope; bệnh nhân mời → consent ghi lúc mời, bác sĩ accept chỉ kích hoạt. Suspend bác sĩ tự thu hồi mọi liên kết | `0ceedb0` |
| B3 — Doctor dashboard | `DoctorAccess` guard 3 lớp (Approved → link Active → đúng scope) + AuditLog mọi lượt xem (TargetUserId). Trang `/doctor/patients` (critical lên đầu) + chi tiết tabs theo scope. Respect consent đến từng trường (BMI hiện, BP ẩn nếu ngoài scope) | `b6fa496` |
| B4 — Thông báo bác sĩ | Critical → email + push ngay; Warning → gom daily digest 1 email/ngày (0:00 UTC). Chống trùng bằng unique (doctor, alert, channel). **Kích hoạt 3 background jobs vốn chưa từng chạy** qua `JobSchedulerHostedService` | `623c5e7` |

**Tests: 129/129 pass** (1 Domain + 107 Application + 21 Integration).

### Lưu ý kỹ thuật quan trọng (đọc trước khi code tiếp)

1. **Dừng API trước khi build/migration**: API đang chạy sẽ khóa DLL → build lỗi MSB3027.
   `Stop-Process -Name HealthCare.API -Force` trước, và **build lại sau khi** `dotnet ef migrations add`
   (nếu tạo migration với `--no-build` trên assembly cũ sẽ ra migration RỖNG — đã dính 1 lần).
2. **Role mới chỉ vào JWT ở lần đăng nhập kế tiếp** (access token sống 15 phút) — duyệt bác sĩ xong
   phải logout/login lại mới thấy menu "Bệnh nhân của tôi".
3. **Background jobs KHÔNG dùng Hangfire** (chưa cài) — chạy qua `JobSchedulerHostedService`
   (Infrastructure/BackgroundJobs): reminder 15'/lần, vaccine + digest hàng ngày. Deploy nhiều
   instance API sẽ chạy trùng job → cần lock hoặc chuyển Hangfire thật khi scale.
4. **PowerShell 5.1 hiển thị sai UTF-8** (mojibake tiếng Việt) khi đọc JSON API — không phải bug app.
   Đã kiểm chứng email/subject lưu đúng UTF-8. Test bằng `curl.exe -o file` rồi đọc file với UTF-8.
5. **Ảnh CCHN** đang lưu chung bucket `health-documents` (spec ghi bucket riêng) — key ngẫu nhiên,
   chỉ truy cập qua signed URL nên tạm chấp nhận; tách bucket khi làm hardening.
6. **NotificationPreferences chưa có cột opt-out** cho doctor alert/digest (spec §7.3) — hiện luôn bật.
   Bổ sung khi làm settings cho bác sĩ.
7. Dữ liệu **consent scope quyết định cả thông báo**: bác sĩ không được chia sẻ
   measurements/blood_pressure thì không nhận cảnh báo về chỉ số.

### Môi trường dev

- Bật tất cả: `powershell -File E:\HealthCare\tools\start-dev.ps1`
  (MinIO :9000/:9001, Mailpit SMTP :1025 UI :8025, API :5199, Angular :4200)
- SQL Server local dùng **Windows auth**, DB `HealthPlusDev`
- Tài khoản test:
  - `testuser@healthplus.vn` / `TestPass123` — bệnh nhân, có role **admin**
  - `testuser2@healthplus.vn` / `TestPass123` — bác sĩ đã xác minh (CCHN 001234/BYT-CCHN, Tim mạch)
  - Hai tài khoản đang có liên kết active, scope [measurements, visits]

### Việc tiếp theo (chọn 1 khi quay lại)

1. **Deploy lên internet** — đáng làm nhất, sản phẩm đã đủ demo. Chưa chốt hosting
   (tunnel 0đ / VPS ~150-250k/tháng / MonsterASP free).
2. **Hardening bảo mật** trước khi mời người dùng thật: tách bucket CCHN, rà soát secrets,
   backup tự động, CORS đang mở toàn bộ origin (`SetIsOriginAllowed(_ => true)` — PHẢI siết khi lên prod).
3. **Giai đoạn C**: ghi chú của bác sĩ trên hồ sơ, đề xuất lịch tái khám, opt-out notification.

---
*Health+ Project | cập nhật 2026-07-10*
