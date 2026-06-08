# CẤU TRÚC DỰ ÁN — Health+
# E-Health Record & Vaccine Tracker
---
**Phiên bản:** 1.0.0 | **Ngày:** 2026-06-08  
**Backend:** ASP.NET Core 9 · Clean Architecture · CQRS  
**Frontend:** Angular 20 · Standalone Components · Signal Store  
**Database:** SQL Server 2022 · EF Core 9  

---

## TỔNG QUAN THƯ MỤC GỐC

```
HealthCare/                             ← Thư mục gốc dự án
│
├── backend/                            ← Toàn bộ backend ASP.NET Core
│   ├── HealthCare.sln
│   ├── src/
│   │   ├── HealthCare.Domain/
│   │   ├── HealthCare.Application/
│   │   ├── HealthCare.Infrastructure/
│   │   └── HealthCare.API/
│   └── tests/
│       ├── HealthCare.Domain.Tests/
│       ├── HealthCare.Application.Tests/
│       └── HealthCare.Integration.Tests/
│
├── frontend/                           ← Toàn bộ frontend Angular 20
│   └── src/
│
├── deploy/                             ← Docker, Nginx, CI/CD
│   ├── docker-compose.yml
│   ├── docker-compose.dev.yml
│   ├── nginx/
│   └── scripts/
│
├── docs/                               ← Tài liệu dự án
│   ├── SRS_HealthPlus.md
│   └── PROJECT_STRUCTURE.md
│
└── .github/
    └── workflows/
```

---

## PHẦN 1 — BACKEND (ASP.NET Core 9)

---

### 1.1 Solution Root

```
backend/
├── HealthCare.sln                      ← Solution file, liên kết tất cả projects
├── .editorconfig                       ← Code style rules (indentation, charset)
├── .gitignore
├── global.json                         ← Ghim phiên bản .NET SDK (9.0.x)
└── Directory.Build.props               ← NuGet properties dùng chung cho tất cả projects
```

---

### 1.2 Domain Layer — `HealthCare.Domain`

> Không phụ thuộc vào bất kỳ layer nào. Chứa toàn bộ business logic thuần túy.

```
src/HealthCare.Domain/
├── HealthCare.Domain.csproj

├── Common/
│   ├── BaseEntity.cs                   ← Id (Guid), CreatedAt, UpdatedAt
│   ├── AuditableEntity.cs              ← Kế thừa BaseEntity + DeletedAt (soft delete)
│   └── IDomainEvent.cs                 ← Interface cho Domain Events (MediatR INotification)

├── Entities/
│   │
│   ├── ─── AUTH ───
│   ├── User.cs                         ← Email, PasswordHash, IsActive, FailedLoginCount, LockedUntil
│   ├── Role.cs                         ← Name: 'user' | 'family_admin' | 'admin' | 'readonly'
│   ├── Permission.cs                   ← Resource + Action (e.g. "health_records" + "read")
│   ├── UserRole.cs                     ← Join table User ↔ Role
│   ├── RolePermission.cs               ← Join table Role ↔ Permission
│   ├── RefreshToken.cs                 ← TokenHash (SHA-256), ExpiresAt, IsRevoked, DeviceInfo
│   ├── EmailVerification.cs            ← OtpHash, Type, ExpiresAt, UsedAt
│   │
│   ├── ─── FAMILY ───
│   ├── FamilyGroup.cs                  ← Name, AdminId
│   ├── FamilyMember.cs                 ← UserId (nullable), FullName, DOB, Relationship, ManagedBy
│   │
│   ├── ─── HEALTH PROFILE ───
│   ├── HealthProfile.cs                ← BloodType, Allergies (JSON), ChronicConditions (JSON),
│   │                                       InsuranceNumber (VARBINARY encrypted), EmergencyContact
│   ├── HealthMeasurement.cs            ← WeightKg, HeightCm, Bmi (computed), HeartRate,
│   │                                       Temperature, BloodGlucose, Spo2, Source, MeasuredAt
│   ├── BloodPressureLog.cs             ← Systolic, Diastolic, Pulse, Arm, Position, MeasuredAt
│   ├── HealthAlert.cs                  ← AlertType, Severity, Message, IsAcknowledged
│   │
│   ├── ─── MEDICAL HISTORY ───
│   ├── MedicalVisit.cs                 ← VisitDate, FacilityName, Diagnosis, ICD10, FollowUpDate
│   ├── MedicalDocument.cs              ← FileName, StorageKey, MimeType, DocumentType,
│   │                                       OcrStatus, OcrText, ThumbnailKey
│   │
│   ├── ─── MEDICATIONS ───
│   ├── DrugCatalog.cs                  ← NameGeneric, NameBrand, DrugClass (master data)
│   ├── Medication.cs                   ← DrugName, Strength, Instructions, StartDate,
│   │                                       IsOngoing, ConfidenceScore (từ OCR)
│   ├── MedicationSchedule.cs           ← ScheduledTime, DosageAmount, ReminderEnabled
│   ├── MedicationLog.cs                ← ScheduledAt, TakenAt, Status (taken/skipped/pending)
│   │
│   ├── ─── VACCINES ───
│   ├── VaccineCatalog.cs               ← Name, TotalDoses, IsMandatory, AgeStartMonths (master data)
│   ├── VaccineScheduleRule.cs          ← DoseNumber, MinIntervalDays, MinAgeMonths (master data)
│   ├── VaccineRecord.cs                ← VaccineName, DoseNumber, InjectionDate, NextDueDate,
│   │                                       Facility, LotNumber, Reaction
│   │
│   ├── ─── NOTIFICATIONS ───
│   ├── NotificationPreference.cs       ← Timezone, QuietHours, PushEnabled, EmailEnabled,
│   │                                       VaccineReminder, MedicationReminder, HealthAlert
│   ├── PushSubscription.cs             ← FcmToken, DeviceType, IsActive
│   ├── Reminder.cs                     ← ReminderType, ReferenceId, Title, RemindAt, Status
│   │
│   ├── ─── OFFLINE SYNC ───
│   ├── SyncQueue.cs                    ← Operation, EntityType, EntityId, Payload (JSON),
│   │                                       ClientVersion (Lamport), Status, ConflictData
│   │
│   └── ─── AUDIT ───
│       └── AuditLog.cs                 ← EventType, UserId, TargetUserId, Resource, Action,
│                                           OldValueHash, NewValueHash, IpAddress (append-only)

├── ValueObjects/
│   ├── Email.cs                        ← Validate định dạng RFC 5322, immutable
│   ├── BloodPressureReading.cs         ← Systolic + Diastolic, phân loại Normal/Stage1/Stage2/Crisis
│   ├── BmiValue.cs                     ← Decimal value + phân loại theo ngưỡng châu Á WHO 2004
│   └── EncryptedField.cs               ← Wrapper cho VARBINARY(MAX) AES-256 encrypted data

├── Enums/
│   ├── Gender.cs                       ← Male, Female, Other
│   ├── BloodType.cs                    ← APositive, ANegative, BPositive, ... OPositive, ONegative
│   ├── DocumentType.cs                 ← Prescription, TestResult, Xray, Report, Other
│   ├── OcrStatus.cs                    ← Pending, Processing, Done, Failed
│   ├── ReminderType.cs                 ← Vaccine, Medication, Followup, Custom
│   ├── AlertType.cs                    ← HighBP, LowSpo2, HighBMI, HighGlucose, ...
│   ├── AlertSeverity.cs                ← Warning, Critical
│   ├── MedicationLogStatus.cs          ← Taken, Skipped, Pending
│   └── SyncOperation.cs                ← Create, Update, Delete

└── Events/
    ├── MeasurementRecordedEvent.cs     ← Kích hoạt khi có measurement mới → check ngưỡng cảnh báo
    ├── BloodPressureAlertEvent.cs      ← Kích hoạt khi BP vượt ngưỡng → gửi push notification
    ├── VaccineDueEvent.cs              ← Kích hoạt khi vaccine sắp đến hạn → tạo reminder
    └── MedicationDueEvent.cs           ← Kích hoạt khi thuốc đến giờ uống
```

---

### 1.3 Application Layer — `HealthCare.Application`

> Chứa Use Cases (CQRS). Phụ thuộc Domain, KHÔNG phụ thuộc Infrastructure.

```
src/HealthCare.Application/
├── HealthCare.Application.csproj

├── Common/
│   ├── Interfaces/
│   │   ├── IApplicationDbContext.cs    ← DbSet<T> properties + SaveChangesAsync
│   │   ├── ICurrentUser.cs             ← UserId, Email, Roles (inject từ JWT middleware)
│   │   ├── IEmailService.cs            ← SendOtpAsync, SendResetPasswordAsync, SendReminderAsync
│   │   ├── IFileStorageService.cs      ← UploadAsync, GetSignedUrlAsync, DeleteAsync
│   │   ├── IOcrService.cs              ← ExtractTextAsync, ExtractPrescriptionAsync
│   │   ├── INotificationService.cs     ← SendPushAsync, SendEmailNotificationAsync
│   │   ├── ITokenService.cs            ← GenerateAccessToken, GenerateRefreshToken, ValidateRefreshToken
│   │   ├── IEncryptionService.cs       ← EncryptAsync(plaintext) → VARBINARY, DecryptAsync
│   │   └── IVaccineScheduleService.cs  ← CalculateNextDueDate(vaccineId, doseNumber, lastDate)
│   │
│   ├── Behaviours/
│   │   ├── ValidationBehaviour.cs      ← MediatR Pipeline: chạy FluentValidation trước handler
│   │   ├── LoggingBehaviour.cs         ← MediatR Pipeline: log tên command + thời gian xử lý
│   │   └── AuditBehaviour.cs           ← MediatR Pipeline: ghi AuditLog sau mỗi Command
│   │
│   ├── Mappings/
│   │   └── MappingProfile.cs           ← AutoMapper: Entity → DTO mappings cho tất cả modules
│   │
│   ├── Models/
│   │   ├── PagedResult.cs              ← Items, Page, PageSize, Total, TotalPages
│   │   ├── Result.cs                   ← Success/Failure wrapper (Railway-oriented pattern)
│   │   └── FileUploadResult.cs         ← StorageKey, SignedUrl, MimeType, SizeBytes
│   │
│   └── Exceptions/
│       ├── ValidationException.cs      ← HTTP 422 — FluentValidation failures
│       ├── NotFoundException.cs         ← HTTP 404 — Entity không tồn tại
│       ├── ForbiddenException.cs        ← HTTP 403 — Không có quyền truy cập
│       └── ConflictException.cs         ← HTTP 409 — Sync conflict, duplicate entity

├── Auth/
│   ├── Commands/
│   │   ├── Register/
│   │   │   ├── RegisterCommand.cs          ← Email, Password, FullName, DateOfBirth, Gender, Phone
│   │   │   ├── RegisterCommandHandler.cs   ← Hash password, tạo User, gửi OTP email
│   │   │   └── RegisterCommandValidator.cs ← Validate email unique, password strength, phone VN
│   │   ├── Login/
│   │   │   ├── LoginCommand.cs
│   │   │   ├── LoginCommandHandler.cs      ← Verify password, check brute force, tạo JWT pair
│   │   │   └── LoginCommandValidator.cs
│   │   ├── RefreshToken/
│   │   │   ├── RefreshTokenCommand.cs
│   │   │   └── RefreshTokenCommandHandler.cs ← Validate hash, rotate token, revoke cũ
│   │   ├── Logout/
│   │   │   └── LogoutCommandHandler.cs     ← Revoke refresh token
│   │   ├── ForgotPassword/
│   │   │   └── ForgotPasswordCommandHandler.cs ← Gửi reset link (JWT one-time)
│   │   ├── ResetPassword/
│   │   │   └── ResetPasswordCommandHandler.cs  ← Validate token, hash new password, revoke all tokens
│   │   ├── ChangePassword/
│   │   │   └── ChangePasswordCommandHandler.cs
│   │   └── VerifyEmail/
│   │       └── VerifyEmailCommandHandler.cs ← Validate OTP hash, kích hoạt tài khoản
│   ├── Queries/
│   │   └── GetCurrentUser/
│   │       └── GetCurrentUserQueryHandler.cs ← Trả UserDto từ ICurrentUser.UserId
│   └── DTOs/
│       ├── AuthResponseDto.cs           ← AccessToken, RefreshToken, ExpiresIn, UserInfo
│       ├── UserInfoDto.cs               ← Id, Email, FullName, Roles
│       └── RegisterDto.cs / LoginDto.cs

├── HealthProfiles/
│   ├── Commands/
│   │   ├── UpdateHealthProfile/         ← Cập nhật BloodType, Allergies, EmergencyContact
│   │   ├── AddMeasurement/              ← Tạo HealthMeasurement mới, publish MeasurementRecordedEvent
│   │   ├── AddBloodPressure/            ← Tạo BloodPressureLog, check ngưỡng → publish Alert
│   │   └── AcknowledgeAlert/            ← Set IsAcknowledged = true
│   ├── Queries/
│   │   ├── GetHealthProfile/            ← Trả profile + latest measurements
│   │   ├── GetMeasurements/             ← Có filter: from/to/type, phân trang
│   │   └── GetBloodPressureLogs/        ← Có filter: from/to
│   └── DTOs/
│       ├── HealthProfileDto.cs
│       ├── HealthMeasurementDto.cs
│       ├── BloodPressureLogDto.cs
│       └── HealthAlertDto.cs

├── MedicalHistory/
│   ├── Commands/
│   │   ├── CreateMedicalVisit/          ← Tạo visit, liên kết documents nếu có
│   │   ├── UpdateMedicalVisit/
│   │   ├── DeleteMedicalVisit/          ← Soft delete (set DeletedAt)
│   │   └── UploadDocument/              ← Upload file → MinIO, tạo MedicalDocument record
│   ├── Queries/
│   │   ├── GetMedicalVisits/            ← Phân trang, filter theo ngày
│   │   ├── GetMedicalVisitById/         ← Kèm theo danh sách documents
│   │   └── GetDocumentDownloadUrl/      ← Gọi MinIO signed URL (1 giờ)
│   └── DTOs/
│       ├── MedicalVisitDto.cs
│       ├── MedicalVisitListDto.cs       ← Compact version cho list
│       └── MedicalDocumentDto.cs

├── Ocr/
│   ├── Commands/
│   │   └── ProcessOcr/
│   │       ├── ProcessOcrCommand.cs         ← DocumentId
│   │       └── ProcessOcrCommandHandler.cs  ← Download từ MinIO → preprocess → OCR → extract → lưu
│   ├── Queries/
│   │   └── GetOcrResult/
│   │       └── GetOcrResultQueryHandler.cs  ← Trả PrescriptionData + confidence scores
│   └── DTOs/
│       ├── OcrResultDto.cs              ← RawText, Status, ConfidenceScore
│       └── PrescriptionDataDto.cs       ← List<ExtractedDrugDto>

├── Medications/
│   ├── Commands/
│   │   ├── CreateMedication/            ← Có thể từ OCR (OcrSourceDocId) hoặc manual
│   │   ├── UpdateMedication/
│   │   ├── DeleteMedication/            ← Soft delete
│   │   ├── AddMedicationSchedule/       ← Tạo schedule + tự tạo reminders
│   │   ├── DeleteMedicationSchedule/
│   │   ├── LogMedicationTaken/          ← Set Status = Taken, TakenAt = now
│   │   └── LogMedicationSkipped/        ← Set Status = Skipped, SkipReason
│   ├── Queries/
│   │   ├── GetMedications/              ← Danh sách medications (active/all)
│   │   ├── GetMedicationSchedules/      ← Lịch uống thuốc hôm nay
│   │   └── GetMedicationCompliance/     ← % tuân thủ theo tuần/tháng
│   └── DTOs/
│       ├── MedicationDto.cs
│       ├── MedicationScheduleDto.cs
│       ├── MedicationLogDto.cs
│       └── ComplianceReportDto.cs

├── Vaccines/
│   ├── Commands/
│   │   ├── CreateVaccineRecord/         ← Tạo record + tự tính NextDueDate + tạo reminder
│   │   ├── UpdateVaccineRecord/
│   │   └── DeleteVaccineRecord/         ← Soft delete
│   ├── Queries/
│   │   ├── GetVaccineRecords/           ← Danh sách đã tiêm
│   │   ├── GetVaccineCatalog/           ← Master list tất cả vaccines
│   │   ├── GetVaccineProgress/          ← % hoàn thành từng loại vaccine
│   │   └── GetVaccinePassportData/      ← Data cho PDF export
│   └── DTOs/
│       ├── VaccineRecordDto.cs
│       ├── VaccineCatalogDto.cs
│       └── VaccineProgressDto.cs

├── Reminders/
│   ├── Commands/
│   │   ├── CreateReminder/
│   │   ├── UpdateReminder/
│   │   └── CancelReminder/             ← Set Status = Cancelled
│   ├── Queries/
│   │   └── GetReminders/               ← Filter: status, type, date range
│   └── DTOs/
│       └── ReminderDto.cs

├── Family/
│   ├── Commands/
│   │   ├── CreateFamilyGroup/
│   │   ├── InviteFamilyMember/         ← Gửi email invite + tạo FamilyMember record
│   │   └── RemoveFamilyMember/         ← Soft delete member
│   ├── Queries/
│   │   └── GetFamilyGroup/             ← Group + tất cả members
│   └── DTOs/
│       ├── FamilyGroupDto.cs
│       └── FamilyMemberDto.cs

├── Notifications/
│   ├── Commands/
│   │   ├── UpdateNotificationPreferences/
│   │   ├── SubscribePush/              ← Lưu FCM token
│   │   └── UnsubscribePush/            ← Set IsActive = false
│   ├── Queries/
│   │   └── GetNotificationPreferences/
│   └── DTOs/
│       └── NotificationPreferencesDto.cs

├── Analytics/
│   └── Queries/
│       ├── GetHealthScore/             ← Tính điểm 0-100 từ BMI, BP, vaccine, medication
│       ├── GetBmiTrend/                ← List{date, bmi} trong khoảng thời gian
│       ├── GetBpTrend/                 ← List{date, systolic, diastolic} 
│       ├── GetVaccineProgress/         ← Từng vaccine: số mũi đã tiêm / tổng
│       └── GetMedicationCompliance/    ← Từng tuần: % đã uống / tổng
│           └── DTOs/
│               ├── HealthScoreDto.cs
│               ├── TrendDataPointDto.cs
│               └── VaccineProgressDto.cs

├── Sync/
│   ├── Commands/
│   │   └── PushSync/
│   │       ├── PushSyncCommand.cs          ← List<SyncOperationDto>
│   │       └── PushSyncCommandHandler.cs   ← Apply operations theo Lamport clock, handle conflicts
│   ├── Queries/
│   │   └── PullSync/
│   │       └── PullSyncQueryHandler.cs     ← Trả changes kể từ since timestamp
│   └── DTOs/
│       ├── SyncOperationDto.cs         ← Operation, EntityType, EntityId, Payload, ClientVersion
│       ├── SyncResultDto.cs            ← Results per operation (success/conflict)
│       └── SyncPullResponseDto.cs

└── Admin/
    ├── Commands/
    │   └── UpdateUserStatus/           ← IsActive = true/false
    └── Queries/
        ├── GetUsers/                   ← Phân trang, search, filter
        ├── GetAuditLogs/               ← Filter: userId, from, to
        └── GetSystemStats/             ← TotalUsers, ActiveUsers, TotalDocuments, DBSize
```

---

### 1.4 Infrastructure Layer — `HealthCare.Infrastructure`

> Implement các interfaces từ Application. Chứa EF Core, MinIO, Email, FCM, OCR.

```
src/HealthCare.Infrastructure/
├── HealthCare.Infrastructure.csproj

├── Persistence/
│   ├── ApplicationDbContext.cs         ← DbContext: DbSet cho tất cả 24 entities,
│   │                                       cấu hình Interceptors, tắt cascade delete không mong muốn
│   │
│   ├── Configurations/                 ← EF Core Fluent API — 1 file/entity
│   │   ├── UserConfiguration.cs        ← Index Email, MaxLength, HasQueryFilter(DeletedAt==null)
│   │   ├── RoleConfiguration.cs
│   │   ├── HealthProfileConfiguration.cs   ← CheckConstraint Owner, VARBINARY InsuranceNumber
│   │   ├── HealthMeasurementConfiguration.cs ← ComputedColumn BMI, Index ProfileId+MeasuredAt
│   │   ├── BloodPressureLogConfiguration.cs
│   │   ├── MedicalVisitConfiguration.cs    ← HasQueryFilter soft delete
│   │   ├── MedicalDocumentConfiguration.cs ← FK SET NULL khi visit bị xóa
│   │   ├── DrugCatalogConfiguration.cs     ← FullTextIndex (requires manual migration)
│   │   ├── MedicationConfiguration.cs
│   │   ├── MedicationScheduleConfiguration.cs
│   │   ├── MedicationLogConfiguration.cs
│   │   ├── VaccineCatalogConfiguration.cs
│   │   ├── VaccineScheduleRuleConfiguration.cs ← UniqueConstraint (CatalogId, DoseNumber)
│   │   ├── VaccineRecordConfiguration.cs
│   │   ├── NotificationPreferenceConfiguration.cs ← Unique UserId
│   │   ├── PushSubscriptionConfiguration.cs
│   │   ├── ReminderConfiguration.cs
│   │   ├── SyncQueueConfiguration.cs
│   │   ├── AuditLogConfiguration.cs    ← Identity column, KHÔNG có FK, KHÔNG có query filter
│   │   ├── FamilyGroupConfiguration.cs
│   │   └── FamilyMemberConfiguration.cs
│   │
│   ├── Interceptors/
│   │   ├── AuditSaveChangesInterceptor.cs  ← Tự động hash old/new values + ghi AuditLog
│   │   └── SoftDeleteInterceptor.cs        ← Chặn DELETE thật, tự set DeletedAt = GETUTCDATE()
│   │
│   ├── Migrations/
│   │   └── (EF Core tự sinh — không chỉnh sửa tay)
│   │
│   └── Seeds/
│       ├── RoleSeeder.cs               ← Seed: user, family_admin, admin, readonly
│       ├── PermissionSeeder.cs         ← Seed tất cả resource+action combinations
│       ├── VaccineCatalogSeeder.cs     ← Seed lịch tiêm TCMR Việt Nam (8 loại vaccine)
│       └── DrugCatalogSeeder.cs        ← Seed ~500 tên thuốc phổ biến tại Việt Nam

├── Services/
│   │
│   ├── Auth/
│   │   ├── TokenService.cs             ← RS256 JWT sign (private key) / verify (public key),
│   │   │                                   GenerateRefreshToken (64-byte random → SHA-256 hash)
│   │   └── EncryptionService.cs        ← AES-256-GCM cho InsuranceNumber field
│   │
│   ├── Storage/
│   │   └── MinioFileStorageService.cs  ← Upload, GetPresignedUrl (1h), Delete, CreateBucketIfNotExists
│   │
│   ├── Email/
│   │   ├── SmtpEmailService.cs         ← MailKit SMTP client, load HTML template, fill placeholders
│   │   └── Templates/
│   │       ├── WelcomeEmail.html       ← Gửi sau khi đăng ký thành công
│   │       ├── OtpEmail.html           ← Gửi OTP 6 số xác nhận email
│   │       ├── ResetPasswordEmail.html ← Gửi link đặt lại mật khẩu
│   │       ├── ReminderEmail.html      ← Gửi nhắc lịch (vaccine/thuốc/tái khám)
│   │       └── SecurityAlertEmail.html ← Gửi khi phát hiện đăng nhập bất thường
│   │
│   ├── Notifications/
│   │   └── FcmNotificationService.cs   ← Gửi FCM push qua Firebase Admin SDK
│   │                                       Retry 3 lần nếu fail, ghi status vào Reminder
│   │
│   ├── Ocr/
│   │   ├── GoogleVisionOcrService.cs   ← Gọi Google Vision API, parse response, trả confidence
│   │   ├── EasyOcrService.cs           ← Gọi self-hosted EasyOCR REST API (fallback)
│   │   ├── OcrServiceProxy.cs          ← Circuit breaker: thử Google trước, fallback EasyOCR
│   │   ├── ImagePreprocessor.cs        ← SixLabors.ImageSharp: deskew, denoise, resize 300 DPI
│   │   └── PrescriptionExtractor.cs    ← Regex patterns tiếng Việt → trích xuất drug/dose/freq
│   │
│   └── Vaccines/
│       └── VaccineScheduleService.cs   ← CalculateNextDueDate: tra VaccineScheduleRules,
│                                           tính catch-up nếu quá hạn

└── BackgroundJobs/
    ├── ReminderProcessorJob.cs         ← Hangfire Recurring (*/15 * * * *):
    │                                       Query pending reminders → check quiet hours
    │                                       → gửi FCM → fallback Email → update status
    ├── MedicationLogGeneratorJob.cs    ← Hangfire Daily (0 0 * * *):
    │                                       Tạo MedicationLogs và Reminders cho ngày N+30
    ├── VaccineReminderJob.cs           ← Hangfire Daily (0 1 * * *):
    │                                       Tìm vaccines sắp đến hạn 7 ngày → tạo Reminders
    └── SyncQueueProcessorJob.cs        ← Hangfire Recurring (*/5 * * * *):
                                            Retry failed sync operations (retryCount < 3)
```

---

### 1.5 API Layer — `HealthCare.API`

> Entry point. Chứa Controllers, Middleware, DI configuration, Swagger.

```
src/HealthCare.API/
├── HealthCare.API.csproj
├── Program.cs                          ← Đăng ký tất cả services, middleware pipeline,
│                                           Hangfire, Serilog, Swagger, EF migrations on startup

├── appsettings.json                    ← Cấu hình chung (Logging, AllowedHosts)
├── appsettings.Development.json        ← Dev: SQL Server local, SMTP test, OCR disabled
├── appsettings.Production.json         ← Prod: chỉ chứa keys không nhạy cảm (secrets qua env vars)

├── Controllers/
│   ├── BaseController.cs               ← [ApiController], [Route("api/v1/[controller]")],
│   │                                       helper methods: OkResult, PagedResult, CreatedResult
│   ├── AuthController.cs               ← POST register, login, refresh, logout,
│   │                                       forgot-password, reset-password, verify-email, PUT change-password
│   ├── HealthProfilesController.cs     ← GET/PUT /health-profiles/me
│   ├── MeasurementsController.cs       ← GET /measurements, POST, DELETE /{id}
│   ├── BloodPressureController.cs      ← GET /blood-pressure, POST, DELETE /{id}
│   ├── MedicalVisitsController.cs      ← CRUD /medical-visits
│   ├── DocumentsController.cs          ← POST /upload, GET /{id}/download, DELETE, POST /{id}/ocr
│   ├── MedicationsController.cs        ← CRUD /medications + schedules + logs
│   ├── VaccinesController.cs           ← CRUD /vaccines + catalog + passport PDF
│   ├── RemindersController.cs          ← CRUD /reminders
│   ├── AnalyticsController.cs          ← GET health-score, bmi-trend, bp-trend, ...
│   ├── FamilyController.cs             ← GET/POST /family, invite, remove member
│   ├── NotificationsController.cs      ← preferences + push-subscribe
│   ├── SyncController.cs               ← POST /sync/push, GET /sync/pull
│   ├── AdminController.cs              ← [Authorize(Roles = "admin")] users, audit-logs, stats
│   └── HealthController.cs             ← GET /health (overall), /health/db, /health/storage, ...

├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs  ← Catch tất cả exceptions → trả JSON error format chuẩn
│   │                                       Map: ValidationException→422, NotFound→404, Forbidden→403
│   ├── AuditMiddleware.cs              ← Ghi AuditLog cho mỗi request thay đổi dữ liệu
│   │                                       Extract: UserId (từ JWT), IpAddress, UserAgent
│   └── CurrentUserMiddleware.cs        ← Parse JWT claims → set ICurrentUser vào DI scope

├── Filters/
│   └── ApiResponseFilter.cs            ← Wrap tất cả response trong { success, data, meta } format

└── Extensions/
    ├── ServiceCollectionExtensions.cs  ← AddApplication(), AddInfrastructure(), AddSwagger(),
    │                                       AddJwtAuthentication(), AddRateLimit(), AddCors()
    ├── ApplicationBuilderExtensions.cs ← UseExceptionHandling(), UseAudit(), UseSecurity()
    └── SwaggerExtensions.cs            ← Cấu hình Swagger JWT bearer, group by tag, versioning
```

---

### 1.6 Test Projects

```
tests/
│
├── HealthCare.Domain.Tests/
│   ├── HealthCare.Domain.Tests.csproj  ← xUnit + FluentAssertions
│   ├── ValueObjects/
│   │   ├── BmiValueTests.cs            ← Test BMI classification ngưỡng châu Á
│   │   ├── BloodPressureReadingTests.cs ← Test BP classification stages
│   │   └── EmailTests.cs               ← Test email format validation
│   ├── Entities/
│   │   ├── UserTests.cs                ← Test brute force lockout logic
│   │   └── VaccineRecordTests.cs       ← Test overdue detection
│   └── Services/
│       └── VaccineScheduleServiceTests.cs ← Test next due date calculation (8 vaccines)
│
├── HealthCare.Application.Tests/
│   ├── HealthCare.Application.Tests.csproj ← xUnit + Moq + FluentAssertions
│   ├── Auth/
│   │   ├── RegisterCommandTests.cs
│   │   ├── LoginCommandTests.cs        ← Test brute force, wrong password
│   │   └── RefreshTokenTests.cs        ← Test rotation, revoked token
│   ├── HealthProfiles/
│   │   ├── AddMeasurementTests.cs      ← Test BMI auto-calc, alert generation
│   │   └── AddBloodPressureTests.cs    ← Test alert threshold
│   ├── Medications/
│   │   └── MedicationComplianceTests.cs
│   └── Vaccines/
│       └── VaccineReminderTests.cs     ← Test reminder creation on new record
│
└── HealthCare.Integration.Tests/
    ├── HealthCare.Integration.Tests.csproj ← xUnit + WebApplicationFactory + SQL Server (Testcontainers)
    ├── Setup/
    │   ├── IntegrationTestBase.cs      ← WebApplicationFactory, seeded DB, cleanup per test
    │   └── TestDatabaseHelper.cs
    ├── Auth/
    │   └── AuthFlowTests.cs            ← Register→VerifyEmail→Login→Refresh→Logout end-to-end
    ├── HealthRecords/
    │   └── MeasurementFlowTests.cs
    ├── MedicalHistory/
    │   └── VisitAndDocumentTests.cs
    ├── Vaccines/
    │   └── VaccineRecordTests.cs
    └── Sync/
        └── SyncProtocolTests.cs        ← Test Lamport clock, conflict resolution
```

---

## PHẦN 2 — FRONTEND (Angular 20)

---

### 2.1 Project Root

```
frontend/
├── package.json                        ← Dependencies + scripts (start, build, test, lint)
├── package-lock.json
├── angular.json                        ← Build config, assets, styles, budgets
├── tsconfig.json                       ← Base TS config
├── tsconfig.app.json                   ← App-specific TS config
├── tsconfig.spec.json                  ← Test-specific TS config
├── tailwind.config.js                  ← TailwindCSS: content paths, theme extend, plugins
├── .eslintrc.json                      ← ESLint + Angular plugin rules
├── .prettierrc                         ← Code formatting rules
├── karma.conf.js                       ← Karma test runner config
├── Dockerfile                          ← Multi-stage: node:22 build → nginx:alpine serve
└── nginx/
    └── default.conf                    ← Nginx config: try_files / index.html (SPA routing),
                                            gzip, cache headers cho assets
```

---

### 2.2 Source Root (`src/`)

```
src/
├── index.html                          ← Root HTML, chứa <app-root>, PWA meta tags
├── main.ts                             ← bootstrapApplication(AppComponent, appConfig)
├── styles.scss                         ← Global styles: @tailwind directives, Angular Material theme
├── manifest.webmanifest                ← PWA: name, icons, theme_color, display: standalone
├── sw.js                               ← Service Worker: Workbox precache + runtime caching

├── assets/
│   ├── icons/
│   │   ├── icon-72x72.png              ← PWA icons (nhiều kích thước)
│   │   ├── icon-192x192.png
│   │   └── icon-512x512.png
│   └── images/
│       └── logo.svg

└── environments/
    ├── environment.ts                  ← { apiUrl: 'http://localhost:5000/api/v1', production: false }
    └── environment.prod.ts             ← { apiUrl: 'https://api.healthplus.vn/api/v1', production: true }
```

---

### 2.3 App Root (`src/app/`)

```
src/app/
├── app.component.ts                    ← Root component: chứa <router-outlet>
├── app.component.html
├── app.config.ts                       ← provideRouter, provideHttpClient, provideAnimations,
│                                           provideServiceWorker, provideMaterial, ...
└── app.routes.ts                       ← Route definitions:
                                            / → redirect → /dashboard
                                            /auth → AuthLayout (lazy)
                                            /dashboard → MainLayout (lazy, authGuard)
                                            /health-records → MainLayout (lazy, authGuard)
                                            /admin → MainLayout (lazy, adminGuard)
```

---

### 2.4 Core Module (`src/app/core/`)

> Singleton services, guards, interceptors. Import 1 lần ở AppConfig.

```
src/app/core/
│
├── auth/
│   ├── auth.service.ts                 ← login(), register(), logout(), refreshToken()
│   │                                       Gọi API, lưu tokens vào localStorage (encrypted)
│   ├── auth.store.ts                   ← SignalStore: currentUser signal, isLoggedIn computed,
│   │                                       isAdmin computed, roles signal
│   ├── auth.guard.ts                   ← CanActivateFn: kiểm tra isLoggedIn, redirect /auth/login
│   ├── admin.guard.ts                  ← CanActivateFn: kiểm tra role === 'admin'
│   └── models/
│       ├── auth-response.model.ts      ← { accessToken, refreshToken, expiresIn, user }
│       ├── login-request.model.ts
│       └── register-request.model.ts
│
├── interceptors/
│   ├── auth.interceptor.ts             ← Thêm Authorization header, auto-refresh khi 401,
│   │                                       queue requests trong khi đang refresh
│   ├── error.interceptor.ts            ← Catch HTTP errors, hiển thị snackbar thông báo,
│   │                                       redirect /auth/login khi 401 không thể refresh
│   └── offline-queue.interceptor.ts    ← Nếu offline: lưu mutation requests vào SyncQueue (IndexedDB),
│                                           trả về optimistic response
│
├── services/
│   ├── api.service.ts                  ← Wrapper cho HttpClient: get<T>(), post<T>(), put<T>(), delete<T>()
│   │                                       Tự động xây dựng URL từ environment.apiUrl
│   ├── offline.service.ts              ← isOnline signal (navigator.onLine + fetch probe every 30s)
│   ├── sync.service.ts                 ← syncNow(): đọc SyncQueue từ IndexedDB → POST /sync/push
│   │                                       pullSync(): GET /sync/pull → merge vào IndexedDB
│   ├── notification.service.ts         ← requestPushPermission(), subscribeFCM(), showLocalNotification()
│   └── storage/
│       ├── indexeddb.service.ts        ← Wrapper cho idb library:
│       │                                   openDB(), getAll(), put(), delete(), clear()
│       │                                   Schema versioning với upgrade callbacks
│       └── db.schema.ts                ← IDB Schema v1:
│                                           stores: healthProfiles, measurements, bloodPressure,
│                                           medicalVisits, vaccines, medications, reminders, syncQueue
│
└── models/
    ├── api-response.model.ts           ← { success, data, meta }
    ├── paged-result.model.ts           ← { items, page, pageSize, total, totalPages }
    └── sync-operation.model.ts         ← { operation, entityType, entityId, payload, clientVersion }
```

---

### 2.5 Shared Module (`src/app/shared/`)

> Reusable components, pipes, directives. Import vào từng feature khi cần.

```
src/app/shared/
│
├── components/
│   ├── loading-spinner/
│   │   └── loading-spinner.component.ts    ← Overlay spinner, nhận [isLoading] input
│   ├── alert-banner/
│   │   └── alert-banner.component.ts       ← Success/Error/Warning/Info banner, auto-dismiss
│   ├── offline-indicator/
│   │   └── offline-indicator.component.ts  ← Banner "Đang offline — X thay đổi chờ đồng bộ"
│   │                                           Đọc từ offline.service.ts isOnline signal
│   ├── confirm-dialog/
│   │   └── confirm-dialog.component.ts     ← Angular Material Dialog xác nhận xóa/hành động nguy hiểm
│   ├── file-upload/
│   │   └── file-upload.component.ts        ← Drag-and-drop + click upload, validate MIME/size,
│   │                                           preview thumbnail cho ảnh
│   ├── chart-wrapper/
│   │   └── chart-wrapper.component.ts      ← Wrapper cho ApexCharts: loading state, empty state,
│   │                                           responsive, dark mode support
│   ├── health-score-widget/
│   │   └── health-score-widget.component.ts ← Hiển thị điểm 0-100 với màu sắc và icon
│   ├── pagination/
│   │   └── pagination.component.ts         ← Phân trang chuẩn, emit pageChange event
│   └── empty-state/
│       └── empty-state.component.ts        ← Hiển thị khi không có dữ liệu (icon + message + CTA)
│
├── pipes/
│   ├── bmi-class.pipe.ts               ← 18.5→'Thiếu cân', 23→'Bình thường', ... (ngưỡng châu Á)
│   ├── bp-class.pipe.ts                ← 120/80→'Bình thường', 140/90→'Cao', ...
│   ├── relative-date.pipe.ts           ← "2 ngày trước", "hôm qua", "vừa xong" (tiếng Việt)
│   ├── file-size.pipe.ts               ← 1048576 → "1 MB"
│   └── vaccine-status.pipe.ts          ← 'overdue'→'Quá hạn', 'upcoming'→'Sắp tới', ...
│
├── directives/
│   └── has-permission.directive.ts     ← *appHasPermission="'health_records:write'"
│                                           Ẩn/hiện element dựa trên RBAC
│
└── validators/
    ├── password-strength.validator.ts  ← Kiểm tra uppercase + lowercase + digit + min 8 chars
    └── vietnam-phone.validator.ts      ← Regex ^(0|\+84)[3|5|7|8|9][0-9]{8}$
```

---

### 2.6 Layouts (`src/app/layouts/`)

```
src/app/layouts/
│
├── main-layout/
│   ├── main-layout.component.ts        ← Shell chính: sidebar + topbar + <router-outlet>
│   ├── main-layout.component.html
│   ├── sidebar/
│   │   └── sidebar.component.ts        ← Navigation menu: Dashboard, Hồ sơ, Lịch sử khám,
│   │                                       Vắc-xin, Thuốc, Nhắc lịch, Gia đình
│   │                                       Collapse trên mobile
│   └── topbar/
│       └── topbar.component.ts         ← User avatar, offline indicator, notification bell,
│                                           logout button
│
└── auth-layout/
    ├── auth-layout.component.ts        ← Shell trang auth: logo + <router-outlet> (không có sidebar)
    └── auth-layout.component.html
```

---

### 2.7 Features (`src/app/features/`)

> Mỗi feature là một lazy-loaded module độc lập.

```
src/app/features/
│
├── auth/
│   ├── auth.routes.ts                  ← /auth/login, /auth/register, /auth/verify-email,
│   │                                       /auth/forgot-password, /auth/reset-password
│   ├── login/
│   │   ├── login.component.ts          ← Reactive Form, gọi auth.service.login(), redirect sau login
│   │   └── login.component.html
│   ├── register/
│   │   ├── register.component.ts       ← Multi-step form: bước 1 thông tin, bước 2 xác nhận OTP
│   │   └── register.component.html
│   ├── verify-email/
│   │   └── verify-email.component.ts   ← OTP input 6 ô, countdown resend (rate limit)
│   ├── forgot-password/
│   │   └── forgot-password.component.ts
│   └── reset-password/
│       └── reset-password.component.ts ← Nhận token từ URL params, form đặt mật khẩu mới
│
├── dashboard/
│   ├── dashboard.routes.ts             ← /dashboard
│   ├── dashboard.component.ts          ← Orchestrate tất cả widgets
│   ├── dashboard.component.html
│   ├── dashboard.store.ts              ← SignalStore: load health score, alerts, upcoming reminders
│   └── widgets/
│       ├── bmi-chart/
│       │   └── bmi-chart.component.ts  ← ApexCharts line chart, ngưỡng tham chiếu (23, 18.5)
│       ├── bp-chart/
│       │   └── bp-chart.component.ts   ← ApexCharts multi-line: systolic + diastolic
│       ├── glucose-chart/
│       │   └── glucose-chart.component.ts
│       ├── weight-chart/
│       │   └── weight-chart.component.ts
│       ├── vaccine-progress/
│       │   └── vaccine-progress.component.ts ← Progress bars cho từng loại vaccine
│       ├── medication-compliance/
│       │   └── medication-compliance.component.ts ← Bar chart % tuân thủ theo tuần
│       ├── upcoming-reminders/
│       │   └── upcoming-reminders.component.ts ← List 7 ngày tới
│       └── health-alerts/
│           └── health-alerts.component.ts  ← Active alerts, acknowledge button
│
├── health-records/
│   ├── health-records.routes.ts        ← /health-records (profile, measurements, blood-pressure)
│   ├── health-records.store.ts         ← SignalStore: profile, measurements list, bp list, alerts
│   ├── profile/
│   │   ├── health-profile.component.ts     ← View profile (blood type, allergies, emergency contact)
│   │   └── health-profile-form.component.ts ← Edit profile form
│   ├── measurements/
│   │   ├── measurements.component.ts       ← List measurements với chart toggle
│   │   ├── measurement-form.component.ts   ← Nhập weight, height, heart rate, etc.
│   │   └── measurement-chart.component.ts  ← ApexCharts cho từng loại measurement
│   └── blood-pressure/
│       ├── blood-pressure.component.ts
│       ├── bp-form.component.ts             ← Nhập systolic, diastolic, pulse, arm, position
│       └── bp-chart.component.ts
│
├── medical-history/
│   ├── medical-history.routes.ts       ← /medical-history
│   ├── medical-history.store.ts
│   ├── visit-list/
│   │   └── visit-list.component.ts     ← List visits phân trang, filter theo năm/tháng
│   ├── visit-detail/
│   │   └── visit-detail.component.ts   ← Chi tiết visit + danh sách documents
│   ├── visit-form/
│   │   └── visit-form.component.ts     ← Create/Edit visit, upload documents inline
│   └── document-viewer/
│       └── document-viewer.component.ts ← Preview PDF/image trong dialog, download button
│
├── ocr/
│   ├── ocr.routes.ts                   ← /ocr/upload, /ocr/review/:docId
│   ├── ocr-upload/
│   │   └── ocr-upload.component.ts     ← Camera capture (mobile) + file upload,
│   │                                       trigger OCR, navigate to review
│   └── ocr-review/
│       └── ocr-review.component.ts     ← Side-by-side: ảnh gốc + form chỉnh sửa kết quả OCR
│                                           Highlight fields có confidence thấp (màu vàng)
│                                           Save → tạo Medications
│
├── medications/
│   ├── medications.routes.ts           ← /medications
│   ├── medications.store.ts
│   ├── medication-list/
│   │   └── medication-list.component.ts ← Chia tabs: Đang dùng / Đã hoàn thành
│   ├── medication-form/
│   │   └── medication-form.component.ts ← Thêm thuốc manual hoặc từ kết quả OCR
│   ├── medication-schedule/
│   │   └── medication-schedule.component.ts ← Cài đặt giờ uống, nhắc nhở
│   └── medication-log/
│       └── medication-log.component.ts  ← Danh sách logs hôm nay, nút "Đã uống" / "Bỏ qua"
│
├── vaccines/
│   ├── vaccines.routes.ts              ← /vaccines
│   ├── vaccines.store.ts
│   ├── vaccine-list/
│   │   └── vaccine-list.component.ts   ← List vaccine records + status badge (đúng hạn/sắp tới/quá hạn)
│   ├── vaccine-form/
│   │   └── vaccine-form.component.ts   ← Thêm/sửa vaccine record, chọn từ catalog
│   ├── vaccine-schedule/
│   │   └── vaccine-schedule.component.ts ← Timeline view lịch tiêm tất cả vaccines
│   └── vaccine-passport/
│       └── vaccine-passport.component.ts ← Preview + nút download PDF
│
├── reminders/
│   ├── reminders.routes.ts             ← /reminders
│   ├── reminder-list/
│   │   └── reminder-list.component.ts  ← Calendar view + list view, filter theo loại
│   └── reminder-form/
│       └── reminder-form.component.ts  ← Tạo/sửa custom reminder
│
├── family/
│   ├── family.routes.ts                ← /family
│   ├── family.store.ts
│   ├── family-group/
│   │   ├── family-group.component.ts   ← Hiển thị group, danh sách thành viên, invite button
│   │   └── invite-member.component.ts  ← Form mời thành viên qua email
│   └── member-profile/
│       └── member-profile.component.ts ← Xem/chỉnh sửa hồ sơ thành viên gia đình
│
└── admin/
    ├── admin.routes.ts                 ← /admin (chỉ role 'admin')
    ├── dashboard/
    │   └── admin-dashboard.component.ts ← Stats: tổng users, active, documents, DB size
    ├── user-management/
    │   ├── user-list.component.ts      ← Phân trang, search, filter active/inactive
    │   └── user-detail.component.ts    ← Xem thông tin, enable/disable account
    ├── audit-logs/
    │   └── audit-log-list.component.ts ← Filter: userId, resource, action, date range
    └── system-settings/
        ├── vaccine-catalog/
        │   ├── vaccine-catalog-list.component.ts
        │   └── vaccine-catalog-form.component.ts ← Thêm/sửa vaccine + schedule rules
        └── drug-catalog/
            ├── drug-catalog-list.component.ts
            └── drug-catalog-form.component.ts
```

---

## PHẦN 3 — DEVOPS & DEPLOYMENT

---

### 3.1 Deploy Directory

```
deploy/
│
├── docker-compose.yml                  ← Production: nginx, frontend, api, sqlserver, minio, ocr-service
├── docker-compose.dev.yml              ← Development override: hot-reload, ports exposed, no SSL
├── docker-compose.test.yml             ← CI/CD: sqlserver + api + test runner
├── .env.example                        ← Template env vars (không commit .env thật)
│
├── nginx/
│   ├── nginx.conf                      ← Main nginx config: upstream, rate limit zones
│   ├── default.conf                    ← Server block: SSL, SPA routing, proxy to API
│   └── ssl/
│       ├── cert.pem                    ← SSL certificate (gitignored)
│       └── key.pem                     ← SSL private key (gitignored)
│
└── scripts/
    ├── init-db.sql                     ← Tạo login healthplus_app, grant permissions, enable TDE
    ├── seed-data.sql                   ← Chạy seeders (vaccine catalog, drug catalog, roles)
    ├── backup.sh                       ← Script backup SQL Server + MinIO
    └── deploy.sh                       ← Pull images, docker-compose up -d, health check, rollback
```

---

### 3.2 GitHub Actions

```
.github/
└── workflows/
    ├── ci.yml                          ← Chạy khi push/PR: test backend + test frontend
    │                                       Service: SQL Server container cho integration tests
    ├── cd.yml                          ← Chạy khi push main: build images + deploy staging + prod
    └── security-scan.yml               ← Weekly: dependabot + OWASP dependency check
```

---

## PHẦN 4 — TÓM TẮT FILE QUAN TRỌNG NHẤT

| File | Vai trò |
|---|---|
| `backend/src/HealthCare.Domain/Entities/*.cs` | Business rules, không phụ thuộc gì |
| `backend/src/HealthCare.Application/Common/Interfaces/` | Contracts giữa Application và Infrastructure |
| `backend/src/HealthCare.Infrastructure/Persistence/ApplicationDbContext.cs` | EF Core DbContext duy nhất |
| `backend/src/HealthCare.Infrastructure/Persistence/Configurations/` | SQL Server schema mapping |
| `backend/src/HealthCare.Infrastructure/Persistence/Seeds/` | Data mẫu bắt buộc (roles, vaccines) |
| `backend/src/HealthCare.Infrastructure/BackgroundJobs/ReminderProcessorJob.cs` | Core của Reminder Engine |
| `backend/src/HealthCare.API/Program.cs` | DI registration tất cả services |
| `frontend/src/app/core/services/storage/indexeddb.service.ts` | Offline storage layer |
| `frontend/src/app/core/services/sync.service.ts` | Offline sync protocol |
| `frontend/src/app/core/interceptors/offline-queue.interceptor.ts` | Tự động queue requests khi offline |
| `frontend/src/app/core/interceptors/auth.interceptor.ts` | Auto-refresh JWT token |
| `frontend/src/app/features/dashboard/widgets/` | Tất cả ApexCharts components |
| `frontend/src/app/features/ocr/ocr-review/` | UI review kết quả OCR |
| `deploy/docker-compose.yml` | Định nghĩa toàn bộ infrastructure |

---

## PHẦN 5 — THỨ TỰ TẠO FILE (Theo Sprint)

```
Sprint 0 — Foundation:
  1. backend/HealthCare.sln + 4 .csproj files
  2. Domain/Common/BaseEntity.cs, AuditableEntity.cs
  3. Infrastructure/Persistence/ApplicationDbContext.cs
  4. API/Program.cs (skeleton)
  5. deploy/docker-compose.dev.yml
  6. frontend/ Angular project (ng new)
  7. frontend/src/app/core/ (skeleton)
  8. .github/workflows/ci.yml

Sprint 1 — Auth:
  1. Domain/Entities: User, Role, Permission, RefreshToken, EmailVerification
  2. Application/Auth/ (tất cả Commands + DTOs)
  3. Infrastructure/Services/Auth/TokenService.cs, EncryptionService.cs
  4. Infrastructure/Persistence/Configurations: User, Role, Permission
  5. Infrastructure/Persistence/Seeds/RoleSeeder.cs
  6. API/Controllers/AuthController.cs
  7. API/Middleware/ExceptionHandlingMiddleware.cs, CurrentUserMiddleware.cs
  8. Frontend: auth.service.ts, auth.store.ts, auth.guard.ts
  9. Frontend: features/auth/ (login, register, verify-email, forgot/reset password)
  10. Frontend: core/interceptors/auth.interceptor.ts, error.interceptor.ts

Sprint 2 — Health Profile:
  1. Domain/Entities: HealthProfile, HealthMeasurement, BloodPressureLog, HealthAlert
  2. Domain/ValueObjects: BmiValue, BloodPressureReading
  3. Domain/Events: MeasurementRecordedEvent, BloodPressureAlertEvent
  4. Application/HealthProfiles/ (Commands + Queries + DTOs)
  5. Infrastructure/Configurations: HealthProfile, HealthMeasurement, BloodPressureLog
  6. API/Controllers: HealthProfilesController, MeasurementsController, BloodPressureController
  7. Frontend: features/health-records/ (tất cả components)
  8. Frontend: shared/pipes/bmi-class.pipe.ts, bp-class.pipe.ts

... (và tiếp tục theo từng Sprint)
```

---

*Cấu trúc này tương ứng 1:1 với SRS_HealthPlus.md v1.1.0*  
*Tổng file ước tính: ~180 files backend + ~150 files frontend + 20 files devops*  
*Health+ Project | 2026-06-08*
