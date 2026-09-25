# KẾT QUẢ CODE REVIEW — Health+
> Ngày review: 2026-08-03/04 · Phạm vi: **Nhóm A** (Auth · ShareGrant · Doctor Portal · Persistence) + **Nhóm B** (Domain · Health Profiles · Analytics · Medications · OCR · Vaccines · Reminders · Family · Background Jobs) + **Nhóm C** (Frontend Angular — Core/Auth/Interceptors/Offline/Push) + **Nhóm D** (Config · Deploy · CI/CD · Tests)
> Trạng thái: **CHƯA SỬA** — file này chỉ ghi nhận phát hiện. Cả 4 nhóm đã review xong.
>
> ⚠️ **BUG-26 bên dưới là phát hiện nghiêm trọng nhất trong toàn bộ đợt review — đọc ngay.**
>
> Mức độ: 🔴 Nghiêm trọng · 🟠 Cao · 🟡 Trung bình · 🟢 Thấp
> CONFIRMED = đã xác minh bằng thực nghiệm/grep toàn codebase, không phải suy đoán.

---

## 🔴 BUG-01 — Module Family + 2 endpoint chết trên SQL Server thật (CONFIRMED bằng thực nghiệm)

**Vấn đề:** 9 chỗ dùng `!x.IsDeleted` trong LINQ query dịch xuống database. `IsDeleted` là computed property (`=> DeletedAt.HasValue` trong `AuditableEntity`) — **không phải cột DB** (không xuất hiện trong bất kỳ migration/model snapshot nào). EF Core không translate được → ném `InvalidOperationException` lúc runtime → API trả 500.

**Đã xác minh:** chạy `ToQueryString()` trên `ApplicationDbContext` với provider SqlServer — cả 4 query đại diện (FamilyGroups, Users, MedicalVisits, FamilyMembers) đều FAIL với `InvalidOperationException: The LINQ expression could not be translated`.

**Vị trí (9 chỗ):**

| File | Dòng | Endpoint bị ảnh hưởng |
|---|---|---|
| `Application/Family/Queries/GetFamily/GetFamilyQueryHandler.cs` | 18 | GET /api/Family |
| `Application/Family/Commands/CreateFamilyGroup/CreateFamilyGroupCommandHandler.cs` | 19 | POST tạo family group |
| `Application/Family/Commands/AddFamilyMember/AddFamilyMemberCommandHandler.cs` | 20 | POST thêm thành viên |
| `Application/Family/Commands/InviteMember/InviteFamilyMemberCommandHandler.cs` | 23 | POST mời thành viên |
| `Application/Family/Commands/RemoveFamilyMember/RemoveFamilyMemberCommandHandler.cs` | 17, 22 | DELETE thành viên |
| `Application/Admin/Queries/ExportUsers/ExportUsersQueryHandler.cs` | 14 | Admin export users |
| `Application/Analytics/Queries/GetVisitFrequency/GetVisitFrequencyQueryHandler.cs` | 28 | Analytics tần suất khám |

(`GetFamilyQueryHandler.cs:24` cũng dùng `!m.IsDeleted` nhưng chạy in-memory sau `Include` nên không lỗi — vẫn nên đổi cho nhất quán.)

**Tại sao 129/129 tests vẫn pass:** test dùng InMemory provider (xác nhận trong `Program.cs` — "skipped for non-relational providers e.g. InMemory in tests"). InMemory thực thi C# trực tiếp nên không bao giờ lộ lỗi translation. → Xem thêm BUG-13.

**Lưu ý:** đây là lỗi tái phạm — PROGRESS.md từng ghi nhận chính lỗi này với `HasQueryFilter(x => !x.IsDeleted)` và đã fix ở configurations bằng `DeletedAt == null`.

**Fix đề xuất:** đổi cả 9 chỗ thành `x.DeletedAt == null`. Riêng `MedicalVisits` (GetVisitFrequency) xóa hẳn điều kiện vì entity đã có global query filter.

---

## 🔴 BUG-02 — Token verify-email dùng được để đổi mật khẩu (token confusion)

**Vấn đề:** bảng `EmailVerification` không có cột phân loại mục đích (`Type`). Cả 2 handler chỉ query theo `Token`:
- `Application/Auth/Commands/ResetPassword/ResetPasswordCommandHandler.cs:16` — chấp nhận **bất kỳ** token EmailVerification hợp lệ, kể cả token xác minh email sinh lúc đăng ký (hạn 24h, nằm trong email chào mừng).
- `Application/Auth/Commands/VerifyEmail/VerifyEmailCommandHandler.cs:16` — ngược lại, token reset (2h) cũng verify email được.

**Kịch bản khai thác:** ai đọc được email verify (forward nhầm, log mail server, hộp thư cũ chưa bấm link) → gọi POST /api/Auth/reset-password với token đó → **chiếm tài khoản**.

**Ghi chú:** spec gốc (PROJECT_STRUCTURE dòng 86) có cột `Type` nhưng code bỏ qua.

**Fix đề xuất:** thêm enum `VerificationType { EmailVerify, PasswordReset }` vào entity + migration; filter theo type ở cả 2 handler.

---

## 🔴 BUG-03 — Đổi/reset mật khẩu không thu hồi refresh token

**Vấn đề:** `ResetPasswordCommandHandler` và `ChangePasswordCommandHandler` chỉ đổi password hash, không revoke session nào. Grep toàn Application xác nhận: `RefreshTokens` chỉ được đụng tới ở Login/Logout/RefreshToken — 0 chỗ trong 2 handler trên.

**Hậu quả:** nạn nhân biết tài khoản bị chiếm, đổi mật khẩu — kẻ tấn công **vẫn giữ quyền truy cập tới 7 ngày** qua refresh token đang có.

**Ghi chú:** spec (PROJECT_STRUCTURE dòng 215) ghi rõ ResetPassword phải "revoke all tokens".

**Fix đề xuất:** sau `UpdatePassword()`, revoke toàn bộ RefreshTokens active của user trong cả 2 handler.

---

## 🟠 BUG-04 — Refresh token + email token lưu plaintext trong DB

**Vấn đề:**
- `RefreshToken.Token` lưu nguyên văn, unique index trên token thô (`RefreshTokenConfiguration.cs:13-14`). `ReplacedByToken` cũng plaintext.
- `EmailVerification.Token` tương tự.

**Hậu quả:** dump DB (SQL injection, backup lộ, insider) → dùng được ngay mọi refresh token active (sống 7 ngày) và mọi token reset password đang hiệu lực.

**Ghi chú:** spec yêu cầu SHA-256 hash. `Application/Sharing/Common/ShareTokens.cs` đã có sẵn helper hash làm đúng chuẩn — chỉ cần áp dụng cùng pattern. Cần migration (hash token hiện có hoặc revoke hết).

---

## 🟠 BUG-05 — Auth endpoints không có rate limit (app-level lẫn proxy-level)

**Vấn đề:** rate limiter chỉ gắn policy `"shared"` (`Program.cs:101-113`). Kiểm tra thêm: thư mục `deploy/nginx/` **rỗng hoàn toàn** — không có cả tầng proxy che chắn như PROJECT_STRUCTURE mô tả.

**Hậu quả:**
1. `forgot-password` spam không giới hạn — mỗi call sinh 1 token reset hợp lệ 2h + 1 email thật.
2. `register` spam tạo tài khoản hàng loạt.
3. Lockout per-account là **DoS vector**: ai biết email nạn nhân có thể cố tình sai mật khẩu 10 lần → khóa tài khoản người khác 24h (`User.RecordFailedLogin`).

**Fix đề xuất:** thêm rate limit policy cho nhóm auth endpoints (theo IP + theo email); cân nhắc CAPTCHA cho forgot-password; lockout nên kết hợp yếu tố IP thay vì chỉ đếm trên account.

---

## 🟠 BUG-06 — Audit log gần như không tồn tại (yêu cầu pháp lý trong SRS)

**Vấn đề:** `AuditLogs.Add` xuất hiện **đúng 1 nơi** trong toàn codebase (`Application/Doctors/Dashboard/Common/DoctorAccess.cs:58` — bác sĩ xem hồ sơ).

Cụ thể các khoảng trống:
- `AuditBehaviour.cs` thực chất chỉ log slow-request (>500ms), **không ghi AuditLog** — trái spec "ghi AuditLog sau mỗi Command".
- Thư mục `Infrastructure/Persistence/Interceptors/` **rỗng** — `AuditSaveChangesInterceptor` và `SoftDeleteInterceptor` trong spec chưa từng được viết.
- Duyệt/từ chối/tạm ngưng bác sĩ (`VerifyDoctorCommands.cs`) không ghi vết "ai duyệt, lúc nào" — trái DOCTOR_PORTAL §4.1.
- Truy cập qua link chia sẻ **không có mặt trong AuditLogs** (chỉ tăng `AccessCount` trên grant) — trái DOCTOR_PORTAL §12 ("bệnh nhân xem được lịch sử ai đã xem hồ sơ tôi").
- Login/logout, thay đổi dữ liệu y tế, admin actions: không ghi gì.

**Fix đề xuất:** việc lớn, nên tách sprint riêng. Tối thiểu trước mắt: ghi AuditLog cho admin actions (duyệt bác sĩ) và shared access.

---

## 🟡 BUG-07 — Endpoint dữ liệu shared không ghi nhận lượt truy cập

**Vấn đề:** chỉ `GET /shared/{token}` (meta) gọi `grant.RecordAccess()` (`GetSharedMetaQuery.cs`). Ai có link gọi thẳng `/shared/{token}/measurements`, `/blood-pressure`... thì không tăng AccessCount, không lưu IP (`GetSharedSectionQueries.cs` — không section nào record).

**Hậu quả:** bệnh nhân nhìn "số lượt xem" thấy con số thấp hơn thực tế; kẻ có link có thể poll dữ liệu mà không để lại vết.

**Fix đề xuất:** ghi nhận access (hoặc ít nhất AuditLog) ở mọi section endpoint, không chỉ meta.

---

## 🟡 BUG-08 — Không phát hiện refresh-token reuse

**Vấn đề:** `RefreshTokenCommandHandler.cs:29-30` — token đã revoke bị dùng lại (dấu hiệu kinh điển token bị đánh cắp: cả kẻ trộm và nạn nhân cùng dùng) chỉ trả 403 đơn lẻ.

**Fix đề xuất:** khi gặp token revoked, revoke toàn bộ chuỗi token thay thế của user (dây chuyền `ReplacedByToken` đã lưu sẵn — chỉ thiếu logic) + gửi email cảnh báo.

---

## 🟡 BUG-09 — Password policy backend yếu hơn spec

**Vấn đề:** cả 3 validator chỉ check `MinimumLength(8)`:
- `RegisterCommandValidator.cs:10`
- `ResetPasswordCommandValidator.cs:10`
- `ChangePasswordCommandValidator.cs:10`

Spec + frontend (`password-strength.validator.ts`) yêu cầu hoa + thường + số. Gọi API trực tiếp đăng ký được mật khẩu `aaaaaaaa`.

**Fix đề xuất:** thêm rule regex (hoa + thường + số) vào cả 3 validator — dùng chung 1 extension method.

---

## 🟡 BUG-10 — Soft delete: 5 entity auditable thiếu query filter, không có interceptor an toàn

**Vấn đề:** chỉ 6 entity có `HasQueryFilter` (MedicalVisit, MedicalDocument, Medication, MedicationLog, MedicationSchedule, VaccineRecord). 5 entity auditable còn lại **không có**: `User`, `FamilyGroup`, `FamilyInvitation`, `FamilyMember`, `HealthProfile`. Dev đã bù bằng filter tay `!IsDeleted` — thứ gây ra BUG-01.

Ngoài ra không có `SoftDeleteInterceptor` → bất kỳ `Remove()` nào trong code tương lai trên entity auditable sẽ hard-delete thật, im lặng.

**Lưu ý phạm vi (đối chiếu SRS):** SRS chỉ khai báo `DeletedAt` trên 7 bảng (Users, FamilyGroups, FamilyMembers, MedicalVisits, MedicalDocuments, Medications, VaccineRecords). Việc `DeleteMeasurement`/`DeleteBloodPressure`/`DeleteMedicationSchedule` hard-delete là **đúng spec**, không phải lỗi.

**Fix đề xuất:** thêm `HasQueryFilter(x => x.DeletedAt == null)` cho 5 entity còn thiếu (fix chung với BUG-01, chú ý cảnh báo EF về navigation tới entity có filter); cân nhắc viết SoftDeleteInterceptor theo spec.

---

## 🟢 BUG-11 — Các vấn đề nhỏ

| # | Vấn đề | Vị trí |
|---|---|---|
| 11a | User enumeration qua timing: BCrypt.Verify (~100ms) chỉ chạy khi email tồn tại; message khóa tài khoản khác message sai mật khẩu → phân biệt được account tồn tại | `LoginCommandHandler.cs` |
| 11b | `TokenService` đọc file PEM từ disk **mỗi lần** sign/validate token — I/O thừa mỗi request | `TokenService.cs:19,58` |
| 11c | `ExpiresIn = 900` hardcode ở 2 handler trong khi thời hạn thật đọc từ config (`Jwt:AccessTokenExpiryMinutes`, hiện = 15 nên khớp) — sẽ lệch nếu đổi config | `LoginCommandHandler.cs`, `RefreshTokenCommandHandler.cs` |
| 11d | Login không yêu cầu `IsEmailVerified` — user chưa xác minh vẫn đăng nhập được. Nếu là chủ đích thì bỏ qua, nhưng làm giảm ý nghĩa luồng verify (liên quan BUG-02) | `LoginCommandHandler.cs` |
| 11e | `GetSharedVaccinesQuery` không có `Take()` giới hạn như các query shared khác | `GetSharedSectionQueries.cs` |
| 11f | `ForgotPassword` không invalidate token reset cũ khi tạo token mới — nhiều token hợp lệ song song trong 2h | `ForgotPasswordCommandHandler.cs` |

---

## 🟡 BUG-12 — (Hạ tầng test) Integration tests dùng InMemory provider

**Vấn đề:** `Program.cs` xác nhận tests chạy InMemory thay vì SQL Server (spec ghi Testcontainers). Hậu quả trực tiếp: cả lớp lỗi translation như BUG-01 vô hình với CI — 129/129 pass trong khi Family module chết trên DB thật.

**Fix đề xuất:** chuyển Integration tests sang SQL Server thật (Testcontainers hoặc LocalDB) — ít nhất cho smoke test mỗi endpoint.

---

---

# NHÓM B — Domain · Health Profiles · Analytics · Medications · OCR · Vaccines · Reminders · Family · Background Jobs

## 🔴 BUG-13 — VaccineReminderJob: lỗi độ ưu tiên toán tử khiến nhắc lịch tiêm chủng luôn đến TRỄ (sau ngày hẹn, không phải trước) — CONFIRMED bằng chạy thử thực tế

**Vấn đề:** `VaccineReminderJob.cs:39`:
```csharp
var remindAt = record.NextDueDate!.Value.ToDateTime(new TimeOnly(8, 0))
    .AddDays(-daysUntil == 1 ? 1 : 7);
```
Toán tử `-` (unary minus) có độ ưu tiên cao hơn `==`, nên biểu thức được parse là `(-daysUntil) == 1 ? 1 : 7`, **không phải** `-(daysUntil == 1 ? 1 : 7)` như người viết code có vẻ định làm. Vì `daysUntil` luôn nằm trong khoảng [0,7] (do query chỉ lấy vaccine đến hạn trong 7 ngày tới), `-daysUntil` không bao giờ bằng 1 → điều kiện luôn `false` → giá trị luôn là `7` (số dương) → `.AddDays(7)` cộng thêm 7 ngày, **bất kể `daysUntil` thực tế là bao nhiêu**.

**Đã xác minh bằng chạy thử độc lập** (tái tạo đúng biểu thức gốc, không suy đoán):
```
daysUntil= 0 | remindAt = ngày đến hạn + 7 ngày   (đúng ra phải trước hạn 1 hoặc 7 ngày)
daysUntil= 1 | remindAt = ngày đến hạn + 7 ngày
...
daysUntil= 7 | remindAt = ngày đến hạn + 7 ngày
```
Kết quả này khác với suy đoán ban đầu của lần review trước (từng ghi nhầm là "remindAt luôn nằm trong quá khứ → bị bỏ qua, không nhắc bao giờ") — đã sửa lại cho đúng ở đây.

**Hậu quả thực tế:** `remindAt` luôn là **(ngày đến hạn + 7 ngày, 08:00)**, tức luôn nằm trong **tương lai** so với ngày chạy job → không bị chặn bởi `if (remindAt <= DateTime.UtcNow) continue;` → reminder **vẫn được tạo và vẫn được gửi**, nhưng luôn **trễ 7 ngày sau khi vaccine đã đến/quá hạn** — đúng lúc phụ huynh cần nhắc trước thì im lặng, đến khi đã trễ cả tuần mới báo. Message hiển thị vẫn ghi "ngày mai"/"7 ngày nữa" (biến `suffix` tính riêng, không bị bug) dù thực chất tin nhắn đến sau khi hạn tiêm đã qua — khiến lỗi rất khó phát hiện khi test thủ công qua UI (nội dung tin nhắn trông hợp lý, chỉ sai thời điểm gửi).

**Đối chiếu:** `CreateVaccineRecordCommandHandler.cs` (tạo reminder ngay lúc thêm vaccine mới) làm đúng — 2 dòng `.AddDays(-7)` và `.AddDays(-1)` tách riêng, không dính lỗi này. Bug chỉ nằm ở job nền quét các vaccine đã có sẵn.

**Fix đề xuất:** `.AddDays(daysUntil == 1 ? -1 : -7);` (đưa dấu trừ vào trong từng nhánh, đã kiểm chứng cho kết quả đúng ý định — nhắc trước hạn, không phải sau hạn).

---

## 🔴 BUG-14 — Tính năng "Nhật ký uống thuốc" không hoạt động — không có nơi nào tự sinh MedicationLog (CONFIRMED bằng grep toàn codebase)

**Vấn đề:** grep `MedicationLog.Create` / `MedicationLogs.Add` trên toàn bộ `Application` + `Infrastructure`: **0 lời gọi** ngoài định nghĩa trong chính entity. Cụ thể:
- `AddMedicationScheduleCommandHandler.cs` chỉ tạo `MedicationSchedule`, không tạo log nào kèm theo.
- Thư mục `Infrastructure/BackgroundJobs/` (đã liệt kê đủ 6 file) **không có** `MedicationLogGeneratorJob` — job mà PROJECT_STRUCTURE.md §1.4 mô tả ("Hangfire Daily: Tạo MedicationLogs và Reminders cho ngày N+30") chưa từng được viết.
- `JobSchedulerHostedService` (thay Hangfire) cũng không lịch job nào cho việc này — chỉ có Reminder/Vaccine/DoctorDigest/ShareGrantCleanup.

**Hậu quả:** `GetMedicationSchedulesQueryHandler` (endpoint "lịch uống thuốc hôm nay") query bảng `MedicationLogs` theo ngày — luôn trả về **rỗng** dù người dùng có bao nhiêu schedule active. `LogMedicationTaken`/`LogMedicationSkipped` không có log nào để đánh dấu. Toàn bộ luồng "Đã uống / Bỏ qua" trong UI hiện không dùng được với dữ liệu thật.

**Fix đề xuất:** viết `MedicationLogGeneratorJob` (đã có tên sẵn trong spec) chạy hàng ngày, sinh `MedicationLog` cho mỗi `MedicationSchedule` active của ngày N+1 (hoặc N+30 theo spec); thêm vào `JobSchedulerHostedService`.

---

## 🟠 BUG-15 — Health Score tính tỷ lệ tuân thủ thuốc từ MedicationLogs của **mọi** người dùng, không lọc theo hồ sơ

**Vấn đề:** `GetHealthScoreQueryHandler.cs`:
```csharp
var logs = await db.MedicationLogs
    .AsNoTracking()
    .Where(l => l.ScheduledAt >= since
        && (l.Status == MedicationLogStatus.Taken || l.Status == MedicationLogStatus.Skipped))
    .ToListAsync(ct);
```
Thiếu điều kiện lọc theo `HealthProfileId` của user hiện tại (so với `GetMedicationComplianceQueryHandler` trong `Analytics/` — handler tương tự nhưng đúng — có filter `l.MedicationSchedule.Medication.HealthProfileId == profile.Id`).

**Hậu quả:** điểm "tuân thủ thuốc" trong Health Score của **mọi** người dùng được tính từ log thuốc của **toàn hệ thống** trộn lẫn, không phải của riêng họ — điểm số sai và vô nghĩa ngay khi có ≥2 user dùng thuốc trong hệ thống. (Lưu ý: bug này đang bị BUG-14 che khuất — vì hiện tại không có `MedicationLog` nào được tạo tự động nên bảng gần như rỗng; bug sẽ lộ ra ngay khi BUG-14 được sửa.)

**Fix đề xuất:** thêm `l.MedicationSchedule.Medication.HealthProfileId == profile.Id` vào điều kiện `Where`.

---

## 🟠 BUG-16 — "Quiet hours" thông báo dùng giờ UTC thay vì giờ địa phương — lệch múi giờ 7 tiếng

**Vấn đề:** `ReminderProcessorJob.cs`:
```csharp
var userLocalTime = TimeOnly.FromDateTime(DateTime.UtcNow);
if (prefs.IsInQuietHours(userLocalTime))
```
Biến tên `userLocalTime` nhưng gán trực tiếp từ `DateTime.UtcNow` — không hề dùng `prefs.Timezone` (mặc định `"Asia/Ho_Chi_Minh"`, UTC+7) để quy đổi.

**Hậu quả:** tính năng "giờ yên tĩnh" (không gửi thông báo ban đêm — SRS nêu rõ) hoạt động sai lệch 7 tiếng đồng hồ. Ví dụ: user đặt quiet hours 22:00–06:00 giờ VN, hệ thống sẽ áp dụng khoảng đó theo giờ UTC, tức thực tế là 05:00–13:00 giờ VN — gửi thông báo ban đêm thật (giờ VN) và im lặng vào ban ngày.

**Fix đề xuất:** convert `DateTime.UtcNow` sang giờ theo `prefs.Timezone` bằng `TimeZoneInfo.ConvertTimeFromUtc` trước khi lấy `TimeOnly`.

---

## 🟡 BUG-17 — Ngưỡng kích hoạt cảnh báo huyết áp cao không khớp ngưỡng phân loại hiển thị (lệch tại đúng biên 140/90)

**Vấn đề:** `BloodPressureLog.Create()` raise domain event (tạo `HealthAlert`) khi `systolic > 140 || diastolic > 90` (strict, không có `=`). Nhưng `HealthCalculations.IsBpAlert()` và `GetBpLabel()` (dùng cho hiển thị/health score) phân loại `>= 140 || >= 90` là "Cao độ 2". `BloodPressureReading` value object cũng dùng `>=`.

**Hậu quả:** một chỉ số đúng 140/90 (biên điển hình trong y khoa) được **hiển thị** là "Cao độ 2" / "Tăng huyết áp độ II" nhưng **không** tạo `HealthAlert`, không thông báo bác sĩ liên kết — trái với chính SRS §3.2 mà comment trong code trích dẫn ("SRS: >140 systolic hoặc >90 diastolic" — SRS gốc dùng `≥`).

**Fix đề xuất:** đổi `BloodPressureLog.Create()` sang `>=` cho nhất quán với `HealthCalculations`/`BloodPressureReading`.

---

## 🟡 BUG-18 — Điều kiện chọn mức độ cảnh báo BMI luôn cho cùng một kết quả (nhánh chết)

**Vấn đề:** `MeasurementRecordedEventHandler.cs`:
```csharp
var severity = notification.Bmi >= 25.0m ? AlertSeverity.Warning : AlertSeverity.Warning;
```
Cả hai nhánh của biểu thức ba ngôi đều là `AlertSeverity.Warning` — biến `severity` luôn nhận cùng giá trị bất kể điều kiện, dấu hiệu rõ ràng của việc quên điền nhánh còn lại (nhiều khả năng ý định ban đầu là `Critical` cho BMI ≥25, "béo phì").

**Fix đề xuất:** xác nhận lại ý định thiết kế rồi sửa 1 trong 2 nhánh (gợi ý: `Critical` khi ≥25 theo cùng logic mà `BloodPressureAlertEventHandler` đang áp dụng cho BP — Critical ở ngưỡng nặng hơn).

---

## 🟡 BUG-19 — 3 nơi tính/phân loại BMI khác nhau, không đồng bộ — 1 trong 3 là dead code

**Vấn đề:** có 3 cách tính BMI song song trong codebase, không dùng chung 1 nguồn:
1. `Domain/ValueObjects/BmiValue.cs` — phân loại 5 mức (18.5/23/25/30), nhưng **grep xác nhận: không được khởi tạo/sử dụng ở bất kỳ đâu khác trong toàn bộ codebase** → dead code hoàn toàn.
2. `Domain/Services/HealthCalculations.GetBmiLabel()` — phân loại 4 mức (18.5/23/25), dùng cho Health Score + Analytics.
3. `HealthMeasurement.Create()` — tự tính tay `Math.Round(weightKg / (heightM*heightM), 2)` khi lưu đo lường, không gọi đến 2 class trên.

**Hậu quả:** không phải bug hiện tại (3 công thức tính BMI giống nhau về giá trị số), nhưng là rủi ro bảo trì — sửa ngưỡng BMI châu Á ở 1 chỗ (ví dụ theo hướng dẫn y tế mới) rất dễ quên 2 chỗ còn lại, gây lệch dữ liệu ngầm giữa các module.

**Fix đề xuất:** xóa `BmiValue` (dead code) hoặc hợp nhất về dùng chung `HealthCalculations` làm nguồn duy nhất.

---

## 🟢 BUG-20 — `ProcessOcrCommandHandler` tạo `new HttpClient()` trực tiếp mỗi request

**Vấn đề:** `ProcessOcrCommandHandler.cs` — `using var http = new HttpClient();` mỗi lần xử lý OCR thay vì inject qua `IHttpClientFactory`. Đây là anti-pattern .NET đã biết (rủi ro cạn kiệt socket/DNS caching sai dưới tải cao).

**Fix đề xuất:** đăng ký `IHttpClientFactory`, inject vào handler.

---

## 🟢 BUG-21 — VaccineScheduleService: logic catch-up đơn giản hóa so với thực tế y khoa

**Vấn đề:** khi ngày đến hạn tính ra rơi vào quá khứ, `VaccineScheduleService.CalculateNextDueDateAsync()` luôn đẩy về "hôm nay + 7 ngày" thay vì áp dụng quy tắc catch-up schedule thực tế (thường phụ thuộc loại vaccine, tuổi, số mũi đã tiêm). Ghi nhận để biết — không chắc là bug nếu đây là chủ đích đơn giản hóa MVP, nhưng nên xác nhận với yêu cầu nghiệp vụ.

---

---

# NHÓM C — Frontend Angular (Core · Auth · Interceptors · Offline · Push)

## 🔴 BUG-22 — Push notification (FCM) không bao giờ hoạt động — frontend gửi token giả, không có tích hợp Firebase/Web Push thật (CONFIRMED)

**Vấn đề:** `core/services/push-subscription.service.ts` được thiết kế để đăng ký FCM token, nhưng:
1. **Không có Firebase SDK nào trong dự án** — grep `firebase` trên `package.json` + toàn bộ `src/`: 0 kết quả.
2. **Không nơi nào gọi `pushManager.subscribe(...)`** với VAPID key (bước bắt buộc để trình duyệt tạo Web Push subscription) — grep `pushManager.subscribe|VAPID|applicationServerKey`: 0 kết quả trong toàn bộ `src/`.
3. Vì (2), `registration.pushManager.getSubscription()` trong `getDeviceToken()` **luôn trả về `null`** (không có gì để lấy) — nhánh `if (sub) return btoa(sub.endpoint)...` trên thực tế **không bao giờ chạy**.
4. Code rơi vào nhánh dự phòng: sinh một `crypto.randomUUID()` ngẫu nhiên, lưu vào `localStorage['hp_device_token']`, rồi gửi chuỗi UUID này lên backend dưới tên trường `fcmToken`.

**Hậu quả:** backend (`FcmNotificationService`, dùng Firebase Admin SDK thật) nhận một chuỗi UUID ngẫu nhiên hoàn toàn không phải FCM registration token hợp lệ — mọi lần gọi `SendPushAsync` chắc chắn thất bại (Firebase từ chối registration token không hợp lệ). Toàn bộ kênh "push" của Smart Reminder Engine (SRS: "Notification: Firebase Cloud Messaging") **không hoạt động cho bất kỳ người dùng nào, từ trước đến giờ**.

**Vì sao không bị phát hiện khi test thủ công:** `ReminderProcessorJob.cs` (đã review ở Nhóm B) có logic fallback — push thất bại thì gửi email — nên người dùng vẫn nhận được thông báo qua email, tạo cảm giác "thông báo hoạt động bình thường" dù kênh push đã chết hoàn toàn từ lớp client.

**Fix đề xuất:** tích hợp Firebase SDK (`firebase/messaging`) thật ở frontend, lấy VAPID key từ Firebase Console, gọi `getToken()` của Firebase đúng chuẩn thay vì tự chế token từ Web Push endpoint/UUID ngẫu nhiên.

---

## 🟠 BUG-23 — "Offline First" chỉ triển khai một phần: không đọc được dữ liệu khi offline, giao thức đồng bộ 2 chiều thật không bao giờ được gọi (CONFIRMED bằng grep)

**Vấn đề:** có 2 lớp cơ chế offline song song trong code, nhưng chỉ 1 lớp thực sự chạy:

1. **Hàng đợi mutation khi offline** (`offlineQueueInterceptor` + `SyncService.processSyncQueue()` + `IndexedDbService.syncQueue`) — **có hoạt động**: request POST/PUT/PATCH/DELETE khi offline được lưu vào IndexedDB, replay lại khi có mạng.
2. **Cache đọc dữ liệu offline** (`IndexedDbService.measurements`, `.bpLogs` — bảng Dexie khai báo sẵn `CachedMeasurement`/`CachedBpLog`) — **hoàn toàn không được dùng**: grep toàn bộ `src/app` cho các bảng này ngoài chính file định nghĩa: 0 kết quả. Không component/store nào ghi hay đọc từ 2 bảng này.
3. **Giao thức sync 2 chiều thật** (`SyncService.pushToServer()`, `.pullFromServer()` — gọi `/sync/push`, `/sync/pull`, có field `clientVersion`/`clientTimestamp` gợi ý Lamport clock theo đúng thiết kế SRS) — **dead code**: grep toàn bộ `src/app`, 2 hàm này không được gọi ở bất kỳ đâu ngoài định nghĩa của chính chúng.

**Hậu quả:** người dùng offline chỉ có thể "gửi thao tác trễ" (tạo/sửa/xóa mà không thấy phản hồi thật, optimistic theo kiểu unqueue-and-hope) — **không thể xem dữ liệu y tế đã lưu trước đó khi mất mạng** (vì cache đọc rỗng), và cơ chế conflict-resolution theo Lamport clock mà backend đã dựng sẵn (`SyncController`, `PushSyncCommand`, `PullSyncQueryHandler` — theo cấu trúc thư mục Nhóm A liệt kê, chưa review chi tiết logic) **không bao giờ được frontend gọi tới**.

**Ghi chú đối chiếu tài liệu:** `PROGRESS.md` liệt kê "Sprint 7 — Offline First: ⏳ Chưa bắt đầu" — nhưng code cho thấy một phần hạ tầng (interceptor, IndexedDB schema, SyncService) đã được viết from trước, chỉ dừng ở mức khung sườn/dở dang, không khớp với trạng thái "chưa bắt đầu" ghi trong tài liệu theo dõi tiến độ.

**Fix đề xuất:** nếu Sprint 7 thực sự chưa làm, cân nhắc xóa phần khung sườn dở dang (dead code dễ gây nhầm lẫn cho người đọc sau) hoặc hoàn thiện nốt: (a) đọc/ghi cache Dexie khi hiển thị dữ liệu, fallback sang cache khi offline; (b) gọi `pushToServer`/`pullFromServer` đúng lúc thay vì chỉ replay HTTP thô.

---

## 🟢 BUG-24 — Access token + refresh token lưu plaintext trong `localStorage`, trái với mô tả tài liệu ("lưu tokens vào localStorage (encrypted)")

**Vấn đề:** `auth.service.ts` — `localStorage.setItem(ACCESS_TOKEN_KEY, res.accessToken)` / `REFRESH_TOKEN_KEY` lưu nguyên văn, không mã hóa. `PROJECT_STRUCTURE.md` dòng mô tả `auth.service.ts` ghi rõ: "lưu tokens vào localStorage (**encrypted**)".

**Hậu quả:** bất kỳ XSS nào trên trang (kể cả từ 1 thư viện phụ thuộc bên thứ 3 bị compromise) đọc được `localStorage` sẽ lấy được cả access token lẫn refresh token — chiếm toàn bộ phiên đăng nhập trong tối đa 7 ngày (thời hạn refresh token). Đây là rủi ro tương ứng phía client của BUG-04 (Nhóm A — refresh token lưu plaintext trong DB phía backend).

**Fix đề xuất:** mức tối thiểu — chấp nhận rủi ro này là đánh đổi phổ biến của SPA (nhiều app thực tế cũng làm vậy) nhưng cần cập nhật lại tài liệu cho khớp thực tế, hoặc nếu muốn giữ đúng cam kết "encrypted" thì cần mã hóa trước khi lưu (lưu ý: key mã hóa phía client luôn có thể bị trích xuất từ chính JS bundle, nên đây chỉ là lớp phòng thủ yếu, không thay thế được httpOnly cookie).

---

## 🟢 BUG-25 — `authToken` snapshot trong hàng đợi offline gần như vô dụng, dễ gây hiểu nhầm

**Vấn đề:** `offlineQueueInterceptor` lưu `authToken` hiện tại vào từng item trong hàng đợi lúc queue. Nhưng khi replay (`SyncService.replayRequest`), request đi qua lại `authInterceptor` — interceptor này **luôn ghi đè** header `Authorization` bằng token mới nhất đọc từ `localStorage` tại thời điểm replay (`addToken()` dùng `req.clone({ setHeaders: {...} })`). Do đó `item.authToken` chỉ thực sự được dùng khi `authService.getAccessToken()` trả về `null` lúc replay (tức người dùng đã bị đăng xuất) — trường hợp mà token cũ trong hàng đợi gần như chắc chắn cũng đã hết hạn, vô dụng theo cách khác.

**Hậu quả:** không gây lỗi chức năng rõ ràng (vì `authInterceptor` "vô tình" tự sửa đúng), nhưng là code gây hiểu nhầm cho người đọc sau — trông như một cơ chế bảo toàn token cố ý trong khi thực chất không có tác dụng trong đường đi chính.

**Fix đề xuất:** bỏ field `authToken` khỏi `SyncQueueItem` nếu xác nhận không cần thiết, hoặc thêm comment giải thích rõ vai trò thực tế (dự phòng khi logged-out).

---

---

# NHÓM D — Config · Deploy · CI/CD · Tests

## 🔴 BUG-26 — RSA PRIVATE KEY dùng để ký JWT bị commit vào git, repo có remote GitHub thật (CONFIRMED — nghiêm trọng nhất toàn bộ đợt review)

**Vấn đề:** `git ls-files` xác nhận các file sau **đang được git track**:
```
backend/src/HealthCare.API/keys/jwt_private.pem
backend/src/HealthCare.API/keys/jwt_public.pem
backend/src/HealthCare.API/appsettings.Development.json
```
`.gitignore` là template Visual Studio mặc định — **không có bất kỳ rule nào** cho `keys/`, `*.pem`, `appsettings.Development.json`, hay `.env`. Đã xác minh `jwt_private.pem` là **RSA private key thật** (26 dòng, `-----BEGIN RSA PRIVATE KEY-----` hợp lệ), không phải placeholder. Repo có remote thật: `git remote -v` → `origin  https://github.com/NamHa2001/HealthCare.git`.

**Cập nhật khi rà soát lại (đã xác minh thêm, không phải suy đoán):** commit thêm private key (`871ac30`) **đã có mặt trên `origin/master`** — `git branch -r --contains 871ac30` liệt kê `origin/master`, và `git rev-list --left-right --count master...origin/master` trả về `0  0` (local và remote-tracking branch đang khớp nhau tuyệt đối, không có commit nào chưa đẩy lên). Đây là bằng chứng cục bộ mạnh cho thấy **key đã được push lên GitHub thật**, không còn là rủi ro giả định "nếu lỡ push" — cần coi khóa này là đã lộ và xử lý theo hướng đó (rotate ngay, không chỉ dọn dẹp .gitignore). *(Lưu ý: kết luận này dựa trên trạng thái remote-tracking ref cục bộ tại thời điểm review, không phải gọi trực tiếp tới GitHub để xác minh real-time — nhưng đủ mạnh để không nên trì hoãn hành động.)*

**Hậu quả:** đây là khóa **ký (sign)** JWT — không phải khóa xác minh. Bất kỳ ai lấy được file này (đã ở trong lịch sử git, dù xóa khỏi commit mới nhất vẫn còn trong history trừ khi rewrite history + revoke key) đều có thể **tự ký JWT hợp lệ cho bất kỳ user nào, kể cả admin** — bỏ qua hoàn toàn hệ thống xác thực, hiệu lực cho đến khi khóa được xoay vòng. Đây là mức độ nghiêm trọng cao nhất có thể có với một hệ thống auth dựa trên JWT asymmetric.

**Trớ trêu:** `deploy/.env.example` đã viết đúng hướng dẫn — *"NEVER commit .env to version control"* và chỉ mô tả cách tự sinh key bằng `openssl` — nhưng vì `.gitignore` thiếu rule nên key sinh ra ở local đã lọt vào git.

**Bằng chứng phụ:** `appsettings.Development.json` cũng bị track, chứa `Encryption:Key` = giá trị AES-256 thật (đã ẩn — key cũ, coi như đã lộ) dùng để mã hóa `InsuranceNumber` — cùng một lớp vấn đề, mức độ nhẹ hơn (chỉ ảnh hưởng field encryption, không chiếm được toàn hệ thống như JWT key).

**Fix đề xuất (khẩn cấp, theo thứ tự):**
1. **Xoay vòng (rotate) cặp khóa JWT RS256 ngay lập tức** — sinh cặp key mới, deploy, khóa cũ coi như đã lộ vĩnh viễn.
2. Đổi luôn `Encryption:Key` (đồng nghĩa cần migration decrypt-by-old-key/re-encrypt-by-new-key cho các `InsuranceNumber` đã lưu).
3. Thêm vào `.gitignore`: `keys/`, `*.pem`, `appsettings.Development.json`, `appsettings.*.local.json`, `.env`.
4. Cân nhắc `git filter-repo` / BFG Repo-Cleaner để xóa khỏi lịch sử git nếu repo từng được push lên remote công khai hoặc chia sẻ — xóa file ở commit mới KHÔNG đủ, lịch sử vẫn còn nguyên.
5. Audit xem đã có ai clone/pull được các commit chứa key này chưa.

---

## 🟠 BUG-27 — Docker Compose "production" thiếu cấu hình JWT key + Encryption key → deploy production hiện tại sẽ crash/không đăng nhập được (CONFIRMED)

**Vấn đề:** `docker-compose.yml` (root) định nghĩa service `api` chạy `ASPNETCORE_ENVIRONMENT: Production`, nhưng:
- Không set biến môi trường `Jwt__PrivateKeyPath` / `Jwt__PublicKeyPath`.
- Không set `Encryption__Key`.
- `appsettings.Production.json` cũng **không có** 2 mục cấu hình này (chỉ Dev có, trỏ tới file cục bộ `keys/jwt_private.pem`).
- `Dockerfile` của API (`backend/src/HealthCare.API/Dockerfile`) **không copy thư mục `keys/`** vào image, không có volume mount nào cho key trong `docker-compose.yml`.

**Hậu quả:** `ServiceCollectionExtensions.AddApiAuthentication` ném `InvalidOperationException("Jwt:PublicKeyPath configuration is missing.")` khi resolve JWT bearer options (lazy, xảy ra ở request xác thực đầu tiên) — và `TokenService.GenerateAccessToken`/`EncryptionService` constructor cũng sẽ lỗi tương tự khi được gọi. Nói cách khác: **deploy đúng theo `docker-compose.yml` hiện tại, tính năng đăng nhập sẽ không hoạt động** (không phải rủi ro tiềm ẩn — chắc chắn lỗi ngay khi thử).

**Fix đề xuất:** thêm biến môi trường trỏ tới key (hoặc mount volume `./backend/keys:/app/keys:ro`), và `Encryption__Key` lấy từ secret manager/vault thay vì hardcode trong compose file.

---

## 🟠 BUG-28 — "Production" docker-compose dùng tài khoản `sa` (superuser) cho kết nối ứng dụng, SQL Server Developer Edition không có giấy phép sản xuất

**Vấn đề:**
- `docker-compose.yml`: `ConnectionStrings__Default: "...User Id=sa;Password=${SQL_SA_PASSWORD};..."` — ứng dụng kết nối SQL Server bằng chính tài khoản quản trị cao nhất, vi phạm nguyên tắc least-privilege. Nếu API bị compromise (ví dụ qua 1 trong các lỗ hổng ở Nhóm A/B), kẻ tấn công có toàn quyền trên cả server DB, không chỉ database ứng dụng.
- `MSSQL_PID: Developer` ở cả `docker-compose.yml` (root) lẫn `docker-compose.dev.yml` — Developer Edition theo EULA của Microsoft **chỉ được dùng cho môi trường phát triển/test, không được cấp phép chạy production**.
- `deploy/scripts/` (thư mục để chứa `init-db.sql` tạo login `healthplus_app` least-privilege, theo đúng comment trong `PROJECT_STRUCTURE.md`) **rỗng hoàn toàn** — script này chưa từng được viết.

**Fix đề xuất:** viết `init-db.sql` tạo login riêng cho ứng dụng với quyền hạn tối thiểu cần thiết; đổi `MSSQL_PID` sang `Standard`/`Enterprise` (có phí) hoặc `Express` (miễn phí, giới hạn 10GB) cho production thật.

---

## 🟡 BUG-29 — Không có tầng Nginx/reverse-proxy trong "production" deploy dù tài liệu mô tả có

**Vấn đề:** `docker-compose.yml` (root) expose thẳng container `api` ra `5000:8080` và `frontend` ra `4200:80` — không có service `nginx` nào. `deploy/nginx/` tồn tại nhưng **rỗng hoàn toàn** (đã xác nhận khi review Nhóm A, nhắc lại ở đây vì thuộc phạm vi Nhóm D). `PROJECT_STRUCTURE.md` §3.1 mô tả `nginx.conf`, `default.conf`, SSL cert — không file nào tồn tại.

**Hậu quả:** không có nơi nào để terminate SSL, không rate-limit ở tầng edge (liên quan BUG-05 Nhóm A — API cũng không tự rate-limit các endpoint auth), không cấu hình security headers (HSTS, CSP...) tập trung.

**Fix đề xuất:** viết `nginx.conf`/`default.conf` theo đúng mô tả trong PROJECT_STRUCTURE.md, hoặc cập nhật lại tài liệu nếu quyết định dùng giải pháp khác (Cloudflare, managed load balancer...).

---

## 🟡 BUG-30 — CI không chạy test frontend, không chạy `HealthCare.Domain.Tests`

**Vấn đề:** `.github/workflows/ci.yml` job `frontend` chỉ chạy `npx tsc --noEmit` + `ng build` — **không có bước `npm test`** dù `package.json` có sẵn script `"test": "ng test"` và ít nhất 1 spec file tồn tại (`app.spec.ts`, theo PROGRESS.md). Một lỗi runtime lọt qua type-check + build vẫn có thể merge mà không ai biết.

Về phía backend, CI chỉ chạy `HealthCare.Application.Tests` + `HealthCare.Integration.Tests`; project `HealthCare.Domain.Tests` không được gọi tới — tuy nhiên đã xác minh project này chỉ chứa 1 test rỗng (`UnitTest1.cs`, `[Fact] public void Test1() {}` — không có assertion, luôn pass), nên việc bỏ qua không mất coverage thực tế, nhưng project rỗng này là dead weight nên dọn hoặc lấp đầy bằng test thật (PROJECT_STRUCTURE.md từng dự định các test như `BmiValueTests`, `BloodPressureReadingTests` cho project này nhưng cuối cùng lại viết dưới `Application.Tests/Domain/` — không sai, chỉ lệch so với kế hoạch ban đầu).

**Fix đề xuất:** thêm bước `npm test -- --watch=false --browsers=ChromeHeadless` (hoặc runner tương đương) vào CI; xóa hoặc bỏ hẳn `HealthCare.Domain.Tests` khỏi solution nếu không có kế hoạch dùng.

---

## 🟡 BUG-31 — Integration test chỉ phủ 4/20 controller, và dùng InMemory provider nên không bắt được lỗi như BUG-01

**Vấn đề:** đếm số `[Fact]`/`[Theory]` theo file: `AuthControllerTests` (4), `HealthControllerTests` (1), `HealthRecordFlowTests` (8), `SyncControllerTests` (7) — tổng cộng chỉ test 4 khu vực trong khi API có **20 controller**. Các controller sau **không có integration test nào**: MedicalVisits, Documents (OCR), Medications, Vaccines, Reminders, Analytics, Family, Notifications, Admin, ShareGrants, Shared, Doctor, DoctorLinks.

Kết hợp với BUG-12 (đã ghi ở Nhóm A): `HealthPlusWebAppFactory.cs` xác nhận dùng `UseInMemoryDatabase` — nên dù có viết thêm test cho các controller còn thiếu, vẫn cần chạy trên SQL Server thật (Testcontainers/LocalDB) mới bắt được các lỗi kiểu BUG-01 (`!x.IsDeleted` không dịch được sang SQL). Đây chính là nguyên nhân trực tiếp khiến BUG-01 (module Family chết trên production) không bị phát hiện qua 129 test đang có.

**Ghi chú tích cực:** `SyncControllerTests.cs` có 7 test thực sự cho `/sync/push`/`/sync/pull` — nghĩa là tính năng sync 2 chiều phía **backend đã được kiểm thử và hoạt động**, chỉ là **frontend không bao giờ gọi tới** (BUG-23, Nhóm C) — xác nhận chéo, củng cố nhận định BUG-23 là vấn đề tích hợp chứ không phải backend hỏng.

**Fix đề xuất:** ưu tiên viết integration test cho Family (trực tiếp bắt được BUG-01 nếu tồn tại từ đầu), Medications, Vaccines; chuyển sang SQL Server thật cho ít nhất 1 pipeline CI riêng (có thể tách job "integration-sqlserver" chạy chậm hơn, không chặn PR).

---

## ✅ ĐIỂM LÀM TỐT (không cần đổi) — cả 4 nhóm A/B/C/D

**Nhóm A:**
- AES-256-GCM đúng chuẩn, nonce random (`EncryptionService.cs`)
- JWT RS256 với `ClockSkew = Zero`, key resolve lazy cho test override
- ShareGrant: token 256-bit, SHA-256 hash, 404 đồng nhất chống dò token, rate limit 30 req/phút/IP, giới hạn 3 link active/hồ sơ, revoke check mọi request
- `DoctorAccess` guard đúng 3 lớp spec (Approved → link Active → đúng scope) + AuditLog mỗi lượt đọc với TargetUserId
- Consent record bất biến đầy đủ (scope + snapshot nguyên văn + IP + UserAgent + người bấm)
- Suspend bác sĩ thu hồi role + toàn bộ link ngay lập tức
- UTC ValueConverter toàn cục trong `ApplicationDbContext` xử lý triệt để bug DatePipe cũ
- 2 ghi chú nợ kỹ thuật trong WORKLOG đã được giải quyết mà chưa cập nhật doc: bucket CCHN đã tách riêng (`doctor-licenses`), CORS đã config-driven (`Cors:AllowedOrigins`)

**Nhóm B:**
- Ownership check (`HealthProfileId` khớp user hiện tại) nhất quán và đúng ở toàn bộ handler đã đọc trong HealthProfiles/Medications/Vaccines/OCR — không phát hiện IDOR nào ở nhóm này
- `JobSchedulerHostedService` dùng khóa phân tán qua UPDATE có điều kiện trên SQL Server — giải quyết đúng vấn đề "chạy nhiều instance" mà WORKLOG từng ghi là nợ kỹ thuật
- `DoctorAlertNotifier` tôn trọng đúng consent scope trước khi báo bác sĩ (measurements/BP), có chống gửi trùng qua `DoctorAlertDeliveries` unique

**Nhóm C:**
- `authInterceptor`: cơ chế gom nhiều request 401 đồng thời chờ chung 1 lần refresh (`isRefreshing` + `BehaviorSubject`) đúng chuẩn, tránh gọi refresh-token trùng lặp
- Guard phía frontend (`authGuard`/`adminGuard`/`doctorGuard`) chỉ đóng vai trò UX — quyền thật luôn được backend enforce lại (đã xác nhận ở Nhóm A/B), không dựa dẫm vào client-side check cho bảo mật thật
- `password-strength.validator.ts` đúng yêu cầu spec (hoa+thường+số+8 ký tự) — chặt hơn backend hiện tại (xem BUG-09, Nhóm A)
- Bug pattern "signal cục bộ không đồng bộ lại sau mutation" (từng gặp trước đây) — kiểm tra lại các component còn dùng `.find()` trên dữ liệu store (`medication-schedule`, `medication-form`, `family`): không phát hiện tái phát, `medication-schedule.component.ts` đã đồng bộ lại đúng ở cả `addSchedule` và `removeSchedule`
- `document-viewer.component.ts` dùng `bypassSecurityTrustResourceUrl` nhưng nguồn URL luôn đến từ chính API backend (signed URL đã qua kiểm tra quyền sở hữu tài liệu ở Nhóm B), không phải input người dùng — không phải lỗ hổng XSS thực tế

**Nhóm D:**
- CI có 3 job tách biệt (backend/frontend/docker) chạy song song hợp lý, `docker` job chỉ chạy khi push vào `master` — tránh build image thừa cho mỗi PR
- `deploy/.env.example` viết đúng hướng dẫn bảo mật (không commit `.env`, hướng dẫn tự sinh JWT key bằng openssl) — vấn đề nằm ở `.gitignore` thiếu rule, không phải do thiếu ý thức của người viết
- `docker-compose.dev.yml` có healthcheck đầy đủ cho SQL Server/MinIO, đúng chuẩn compose hiện đại
- Test cho Doctor Portal khá đầy đủ ở tầng Application (`DoctorAlertNotifierTests`, `DoctorDashboardTests`, `DoctorLinkTests`, `DoctorRegistrationTests`) — phản ánh đúng độ phức tạp nghiệp vụ cao nhất trong hệ thống
- `SyncControllerTests.cs` cho thấy backend sync protocol được test thật, không phải tính năng bỏ hoang hoàn toàn — chỉ là chưa tích hợp frontend

---

## THỨ TỰ SỬA ĐỀ XUẤT

1. **BUG-01** — đang làm chết chức năng ngay bây giờ, sửa nhanh (9 dòng)
2. **BUG-27, BUG-28** — deploy production hiện tại đăng nhập sẽ crash + dùng tài khoản `sa`, phải chặn trước khi deploy thật
3. **BUG-02, BUG-03** — lỗ hổng chiếm tài khoản, vài chục dòng code
4. **BUG-01** — đang làm chết chức năng ngay bây giờ, sửa nhanh (9 dòng)
5. **BUG-13, BUG-14** — nhắc lịch tiêm chủng gửi trễ (sau hạn, mất tác dụng phòng ngừa) và nhật ký uống thuốc không hoạt động, sửa không lớn nhưng ảnh hưởng trải nghiệm cốt lõi
6. **BUG-22** — push notification chưa từng hoạt động (cần tích hợp Firebase SDK thật ở frontend — việc lớn hơn các bug khác trong nhóm này, nhưng là tính năng lõi của Smart Reminder Engine)
7. **BUG-16, BUG-17** — sai lệch ngưỡng cảnh báo y tế, ảnh hưởng trực tiếp tính năng an toàn
8. **BUG-04** — cần migration
9. **BUG-05, BUG-09** — chống abuse
10. **BUG-31** — mở rộng integration test lên SQL Server thật + phủ các controller còn thiếu, để không lặp lại kiểu lỗi BUG-01
11. **BUG-15, BUG-18, BUG-19, BUG-30** — sửa nhanh, rủi ro thấp hơn nhưng dễ làm luôn khi đụng vào file liên quan
12. **BUG-23, BUG-29** — quyết định rõ hướng đi (hoàn thiện offline-first / viết nginx config hay dọn dead code, cập nhật lại tài liệu) trước khi làm tiếp
13. **BUG-06** — thiết kế lại audit, tách sprint riêng
14. Còn lại theo mức độ

---
*Nhóm A + B + C + D hoàn thành 2026-08-03/04 — cả 4 nhóm đã review*
