# SRS — Đặc Tả Yêu Cầu Phần Mềm
# Health+ | E-Health Record & Vaccine Tracker
---
**Phiên bản:** 1.1.0  
**Ngày:** 2026-06-08  
**Tác giả:** System Architect / Healthcare System Administrator  
**Trạng thái:** Draft — Chờ phê duyệt  

---

## MỤC LỤC

1. [Giới thiệu tổng quan](#1-giới-thiệu-tổng-quan)
2. [Phân tích bên liên quan](#2-phân-tích-bên-liên-quan)
3. [Phạm vi hệ thống](#3-phạm-vi-hệ-thống)
4. [Yêu cầu chức năng](#4-yêu-cầu-chức-năng)
5. [Yêu cầu phi chức năng](#5-yêu-cầu-phi-chức-năng)
6. [Thiết kế cơ sở dữ liệu](#6-thiết-kế-cơ-sở-dữ-liệu)
7. [Kiến trúc hệ thống](#7-kiến-trúc-hệ-thống)
8. [Đặc tả API](#8-đặc-tả-api)
9. [Bảo mật & Tuân thủ](#9-bảo-mật--tuân-thủ)
10. [Offline First & Đồng bộ dữ liệu](#10-offline-first--đồng-bộ-dữ-liệu)
11. [Thông báo thông minh](#11-thông-báo-thông-minh)
12. [OCR Pipeline](#12-ocr-pipeline)
13. [DevOps & Triển khai](#13-devops--triển-khai)
14. [Chiến lược kiểm thử](#14-chiến-lược-kiểm-thử)
15. [Kế hoạch Sprint](#15-kế-hoạch-sprint)
16. [Rủi ro & Giảm thiểu](#16-rủi-ro--giảm-thiểu)

---

## 1. GIỚI THIỆU TỔNG QUAN

### 1.1 Mục đích tài liệu

Tài liệu này là bản đặc tả yêu cầu phần mềm (SRS) đầy đủ cho hệ thống **Health+**. Nó mô tả toàn bộ yêu cầu chức năng, phi chức năng, kiến trúc kỹ thuật, thiết kế cơ sở dữ liệu, và kế hoạch triển khai. Tài liệu phục vụ làm căn cứ cho toàn bộ quá trình phát triển, kiểm thử và bàn giao sản phẩm.

### 1.2 Bối cảnh & Vấn đề cần giải quyết

Tại Việt Nam hiện nay:
- **85%** người dân quản lý hồ sơ y tế bằng giấy tờ rời rạc, dễ thất lạc
- **Không có** hệ thống tập trung cho phép cá nhân theo dõi lịch sử khám bệnh liên tục
- **Nhắc lịch tiêm chủng** phụ thuộc hoàn toàn vào trí nhớ hoặc sổ tay thủ công
- **Mạng internet không ổn định** ở vùng nông thôn — ứng dụng cần hoạt động offline
- **Toa thuốc viết tay** khó lưu trữ, dễ nhầm lẫn khi không có bác sĩ giải thích

### 1.3 Mô tả hệ thống

**Health+** là ứng dụng web (PWA) quản lý hồ sơ sức khỏe điện tử cá nhân và gia đình, cho phép:

- Lưu trữ toàn bộ hồ sơ sức khỏe theo chuẩn điện tử
- Theo dõi chỉ số sức khỏe theo thời gian (time-series)
- Quản lý lịch tiêm chủng và tự tính lịch tiêm tiếp theo
- Nhận diện toa thuốc qua OCR
- Nhắc lịch thông minh qua Push Notification / Email
- Hoạt động offline, đồng bộ khi có internet
- Dashboard phân tích xu hướng sức khỏe cá nhân

### 1.4 Định nghĩa & Từ viết tắt

| Thuật ngữ | Định nghĩa |
|---|---|
| EHR | Electronic Health Record — Hồ sơ sức khỏe điện tử |
| PWA | Progressive Web App — Ứng dụng web tiến bộ |
| OCR | Optical Character Recognition — Nhận dạng ký tự quang học |
| RBAC | Role-Based Access Control — Kiểm soát truy cập theo vai trò |
| CQRS | Command Query Responsibility Segregation |
| JWT | JSON Web Token |
| BMI | Body Mass Index — Chỉ số khối cơ thể |
| FCM | Firebase Cloud Messaging |
| CDN | Content Delivery Network |
| SRS | Software Requirements Specification |
| EF Core | Entity Framework Core — ORM cho .NET |

---

## 2. PHÂN TÍCH BÊN LIÊN QUAN

### 2.1 Danh sách bên liên quan

| Bên liên quan | Vai trò | Mức độ ảnh hưởng |
|---|---|---|
| Người dùng cuối (cá nhân) | Tự quản lý hồ sơ sức khỏe | Cao |
| Trưởng gia đình | Quản lý hồ sơ cho các thành viên | Cao |
| Người cao tuổi (được hỗ trợ) | Xem, không tự quản lý | Trung bình |
| Quản trị viên hệ thống | Quản lý user, hệ thống | Cao |
| Bác sĩ / Y tá (tương lai) | Xem hồ sơ khi được chia sẻ | Trung bình |

### 2.2 Persona người dùng chính

**Persona 1 — Nguyễn Thị Lan, 35 tuổi, mẹ bỉm sữa**
- Quản lý sức khỏe cho 2 con nhỏ + bản thân + bố mẹ chồng
- Hay quên lịch tiêm nhắc nhở cho con
- Sống ở ngoại thành, mạng 4G không ổn định
- Cần: Offline hoạt động được, nhắc lịch rõ ràng, dễ dùng

**Persona 2 — Trần Văn Minh, 28 tuổi, nhân viên văn phòng**
- Tự theo dõi chế độ ăn, tập gym, huyết áp
- Muốn xem biểu đồ xu hướng BMI, huyết áp theo thời gian
- Cần: Dashboard trực quan, upload nhanh toa thuốc

**Persona 3 — Admin hệ thống**
- Quản lý toàn hệ thống, xem audit logs
- Cần: Dashboard quản trị, báo cáo thống kê

---

## 3. PHẠM VI HỆ THỐNG

### 3.1 Trong phạm vi (In-scope)

- Quản lý hồ sơ sức khỏe cá nhân và gia đình
- Theo dõi chỉ số sức khỏe time-series
- Lịch sử khám bệnh + upload tài liệu y tế
- OCR nhận dạng toa thuốc (ảnh chụp)
- Quản lý vaccine + tự tính lịch tiêm
- Smart Reminder: Push, Email
- Offline First + Background Sync
- Dashboard phân tích sức khỏe
- Quản trị: User, Role, Audit Log

### 3.2 Ngoài phạm vi (Out-of-scope) — Phiên bản 1.0

- Kết nối trực tiếp với HIS bệnh viện
- Telemedicine / video call bác sĩ
- Thanh toán viện phí
- Kê đơn thuốc điện tử có chữ ký số
- SMS notification
- AI chẩn đoán bệnh

### 3.3 Ràng buộc hệ thống

- Tuân thủ Luật An toàn thông tin mạng Việt Nam (Luật 86/2015/QH13)
- Nghị định 13/2023/NĐ-CP về bảo vệ dữ liệu cá nhân
- Logs không được chứa thông tin y tế dạng cleartext
- Không lưu số CMND/CCCD dạng plaintext

---

## 4. YÊU CẦU CHỨC NĂNG

---

### MODULE 1: AUTHENTICATION & SECURITY

#### 1.1 Đăng ký tài khoản

**Actor:** Guest  
**Luồng chính:**
1. User nhập: email, password, họ tên, ngày sinh, giới tính, số điện thoại
2. Hệ thống validate: email hợp lệ, password đủ mạnh, số điện thoại Việt Nam
3. Gửi email xác nhận (OTP 6 số, hết hạn 10 phút)
4. User nhập OTP → tài khoản kích hoạt
5. Tự động tạo hồ sơ sức khỏe rỗng cho user

**Validation rules:**
- Email: RFC 5322 format, unique trong hệ thống
- Password: min 8 ký tự, phải có uppercase, lowercase, digit
- Phone: regex `^(0|\+84)[3|5|7|8|9][0-9]{8}$`
- Ngày sinh: không được là tương lai, không quá 120 năm trước

**Luồng ngoại lệ:**
- Email đã tồn tại → báo lỗi, gợi ý đăng nhập
- OTP hết hạn → cho phép gửi lại (rate limit: 3 lần/giờ)

---

#### 1.2 Đăng nhập

**Luồng chính:**
1. Nhập email + password
2. Verify credentials
3. Trả về: `access_token` (15 phút), `refresh_token` (7 ngày), `user_info`
4. Ghi audit log: thời gian, IP, user agent

**Bảo vệ Brute Force:**
- Sau 5 lần sai: khóa 15 phút
- Sau 10 lần sai: khóa 24 giờ, gửi email cảnh báo
- Rate limit: 10 requests/phút/IP

---

#### 1.3 Refresh Token

**Luồng:**
1. Client gửi `refresh_token` còn hạn
2. Hệ thống validate token chưa bị revoke
3. Phát `access_token` mới + rotate `refresh_token` mới (token rotation)
4. Invalidate `refresh_token` cũ ngay lập tức

---

#### 1.4 Quên mật khẩu / Đặt lại mật khẩu

1. Nhập email → gửi link reset (JWT one-time token, hết hạn 1 giờ)
2. User click link → nhập password mới
3. Invalidate tất cả refresh_token hiện tại của user
4. Gửi email xác nhận đã đổi mật khẩu

---

#### 1.5 Phân quyền RBAC

| Role | Quyền |
|---|---|
| `user` | CRUD hồ sơ của bản thân và thành viên gia đình được ủy quyền |
| `family_admin` | Quản lý hồ sơ tất cả thành viên gia đình |
| `admin` | Toàn quyền hệ thống, xem audit logs |
| `readonly` | Chỉ xem (dùng khi chia sẻ với bác sĩ) |

---

### MODULE 2: QUẢN LÝ GIA ĐÌNH

#### 2.1 Tạo nhóm gia đình

- User tạo "Family Group", trở thành `family_admin`
- Mời thành viên qua email hoặc QR code
- Mỗi thành viên có profile sức khỏe riêng
- `family_admin` có thể tạo hồ sơ cho trẻ em / người cao tuổi không dùng điện thoại

#### 2.2 Ủy quyền truy cập

- Ủy quyền xem / chỉnh sửa hồ sơ cho thành viên khác
- Time-limited access: ví dụ cho bác sĩ xem trong 24 giờ
- Revoke quyền bất kỳ lúc nào

---

### MODULE 3: HỒ SƠ SỨC KHỎE (E-Health Record)

#### 3.1 Thông tin cơ bản (Profile tĩnh)

| Trường | Kiểu | Ghi chú |
|---|---|---|
| full_name | nvarchar | |
| date_of_birth | date | |
| gender | nvarchar | male/female/other |
| blood_type | nvarchar | A+/A-/B+/B-/AB+/AB-/O+/O- |
| allergies | nvarchar(max) | JSON array |
| chronic_conditions | nvarchar(max) | JSON array |
| emergency_contact | object | Tên, SĐT người liên hệ khẩn |
| insurance_number | varbinary | AES-256 encrypted |
| primary_doctor | nvarchar | Tên bác sĩ chính |

#### 3.2 Chỉ số sức khỏe (Time-series)

Mỗi lần đo tạo 1 bản ghi mới, **không ghi đè**:

| Chỉ số | Đơn vị | Ngưỡng cảnh báo |
|---|---|---|
| Cân nặng | kg | Thay đổi >5% trong 1 tháng |
| Chiều cao | cm | — |
| BMI | kg/m² | Tự tính; >23 thừa cân (ngưỡng châu Á) |
| Huyết áp tâm thu | mmHg | >140 cảnh báo cao |
| Huyết áp tâm trương | mmHg | >90 cảnh báo cao |
| Nhịp tim | bpm | <60 hoặc >100 |
| Đường huyết | mmol/L | >7.0 (lúc đói) |
| SpO2 | % | <95% |
| Nhiệt độ | °C | >37.5 |

**BMI Classification (ngưỡng châu Á — WHO 2004):**
- < 18.5: Thiếu cân
- 18.5 – 22.9: Bình thường
- 23.0 – 24.9: Thừa cân
- ≥ 25.0: Béo phì

---

### MODULE 4: LỊCH SỬ KHÁM BỆNH (Medical History)

#### 4.1 Tạo hồ sơ khám bệnh

| Trường | Bắt buộc | Ghi chú |
|---|---|---|
| visit_date | Có | |
| facility_name | Có | |
| doctor_name | Không | |
| chief_complaint | Có | Lý do khám |
| diagnosis | Có | |
| icd10_code | Không | Mã ICD-10 |
| treatment | Không | |
| follow_up_date | Không | |
| documents | Không | PDF/JPG/PNG/HEIC, max 10MB/file |

#### 4.2 Upload tài liệu y tế

- Hỗ trợ: PDF, JPG, PNG, HEIC
- Giới hạn: 10MB/file, 50MB/lần upload
- Lưu trữ: MinIO/S3 với signed URL (hết hạn 1 giờ)
- Thumbnail tự động cho ảnh

---

### MODULE 5: OCR TỜ THUỐC

#### 5.1 Luồng xử lý

```
[User upload ảnh toa thuốc]
        ↓
[Image Preprocessing]
  - Deskew / Denoise / Contrast enhancement
  - Resize về 300 DPI
        ↓
[OCR Engine]
  - Primary: Google Vision API (tiếng Việt tốt nhất)
  - Fallback: EasyOCR (self-hosted Docker)
  - Confidence score per word/line
        ↓
[Structured Extraction]
  - Regex + pattern matching
  - Trích xuất: tên thuốc, hàm lượng, liều dùng, số lần/ngày, số ngày
        ↓
[Drug Name Validation]
  - Match với drug_catalog trong SQL Server (Full-Text Search)
  - Suggest closest match nếu không chính xác
        ↓
[User Review UI]
  - Hiển thị kết quả OCR + ảnh gốc side-by-side
  - Confidence < 75%: highlight màu vàng để user chú ý
        ↓
[Lưu vào Medication List]
```

---

### MODULE 6: QUẢN LÝ VẮC-XIN

#### 6.1 Lịch tiêm chuẩn Việt Nam (TCMR)

| Vắc-xin | Số mũi | Lịch chuẩn |
|---|---|---|
| Viêm gan B | 3 | Sơ sinh, 1 tháng, 6 tháng |
| Bại liệt (OPV/IPV) | 4 | 2, 3, 4, 18 tháng |
| DTP (Bạch hầu - Ho gà - Uốn ván) | 5 | 2, 3, 4, 18 tháng, 4-6 tuổi |
| MMR (Sởi - Quai bị - Rubella) | 2 | 9-12 tháng, 18 tháng |
| Thủy đậu | 2 | 12-18 tháng, 4-6 tuổi |
| Cúm | Hàng năm | Từ 6 tháng tuổi |
| HPV | 2-3 | 9-26 tuổi |
| COVID-19 | Theo phác đồ | Cập nhật theo Bộ Y tế |

#### 6.2 Tự tính lịch tiêm tiếp theo

```
1. Lấy ngày tiêm gần nhất của mũi hiện tại
2. Tra vaccine_schedule_rules: khoảng cách tối thiểu giữa các mũi
3. next_due_date = last_injection_date + min_interval_days
4. Nếu next_due_date đã qua → trạng thái "Quá hạn", tính ngày catch-up
5. Tạo Reminder tự động 7 ngày và 1 ngày trước next_due_date
```

#### 6.3 Vaccine Passport

Xuất PDF tóm tắt toàn bộ lịch sử tiêm chủng, có mã QR.

---

### MODULE 7: SMART REMINDER ENGINE

#### 7.1 Loại nhắc nhở

| Loại | Trigger | Kênh |
|---|---|---|
| Nhắc tiêm vắc-xin | 7 ngày và 1 ngày trước | Push + Email |
| Nhắc uống thuốc | Giờ do user cài đặt | Push |
| Nhắc tái khám | N ngày trước follow_up_date | Push + Email |
| Cảnh báo chỉ số | Khi vượt ngưỡng | Push (ngay lập tức) |
| Vắc-xin quá hạn | Khi next_due_date đã qua | Push + Email |

#### 7.2 Cài đặt thông báo

- Timezone của user (mặc định: Asia/Ho_Chi_Minh)
- Quiet hours: không nhắc trong khung giờ user chọn (ví dụ: 22:00 – 7:00)
- Kênh ưu tiên: Push > Email
- Opt-out từng loại thông báo riêng biệt

---

### MODULE 8: HEALTH ANALYTICS DASHBOARD

#### 8.1 Biểu đồ & Metrics

| Biểu đồ | Loại Chart | Dữ liệu |
|---|---|---|
| BMI Trend | Line Chart | BMI theo thời gian + ngưỡng bình thường |
| Cân nặng | Line Chart | Weight theo thời gian |
| Huyết áp | Multi-line | Systolic + Diastolic |
| Vaccine Progress | Progress Bar | Số mũi đã tiêm / tổng |
| Medication Compliance | Bar Chart | % tuân thủ theo tuần |
| Blood Glucose | Line Chart | Đường huyết theo thời gian |
| Tần suất khám | Bar Chart | Số lần khám theo tháng/năm |

#### 8.2 Health Score

- Điểm tổng hợp 0–100 dựa trên: BMI, huyết áp, tuân thủ vaccine, tuân thủ thuốc
- Cảnh báo đang hoạt động: danh sách chỉ số vượt ngưỡng
- Upcoming reminders: 7 ngày tới

---

### MODULE 9: OFFLINE FIRST

*(Xem chi tiết tại Mục 10)*

---

### MODULE 10: QUẢN TRỊ HỆ THỐNG

#### 10.1 Quản lý người dùng

- Xem danh sách user (phân trang, tìm kiếm, lọc)
- Kích hoạt / Vô hiệu hóa tài khoản
- Reset mật khẩu thủ công
- Export danh sách user (CSV)

#### 10.2 Audit Log

Mọi hành động quan trọng đều được ghi log:

| Trường | Mô tả |
|---|---|
| event_type | login, logout, data_access, data_modify, data_delete |
| user_id | Ai thực hiện |
| target_user_id | Dữ liệu của ai bị truy cập |
| resource | Tên bảng / endpoint |
| action | create/read/update/delete |
| ip_address | IP người thực hiện |
| user_agent | Browser/device |
| timestamp | Thời gian chính xác (UTC) |
| old_value_hash | Hash dữ liệu trước khi sửa |
| new_value_hash | Hash dữ liệu sau khi sửa |

> Audit log **không được xóa**, chỉ có thể archive sau 2 năm.

#### 10.3 System Settings

- Cấu hình: giới hạn upload, thời gian token, quota per user
- Vaccine master data: thêm/sửa loại vắc-xin, lịch chuẩn
- Drug master database: thêm/sửa danh mục thuốc
- Email templates

---

## 5. YÊU CẦU PHI CHỨC NĂNG

### 5.1 Hiệu năng

| Chỉ số | Yêu cầu |
|---|---|
| API response time (P95) | < 500ms |
| API response time (P99) | < 2000ms |
| Page load time (FCP) | < 2 giây |
| OCR processing time | < 30 giây/ảnh |
| Dashboard render time | < 3 giây (1000 data points) |
| Concurrent users | Tối thiểu 1000 CCU |

### 5.2 Độ tin cậy

- **Uptime:** 99.5%
- **RTO:** < 4 giờ
- **RPO:** < 1 giờ (backup mỗi giờ)

### 5.3 Khả năng mở rộng

- API server stateless → horizontal scaling
- SQL Server với read replicas khi cần
- Hỗ trợ tăng từ 1.000 lên 100.000 users không cần refactor

### 5.4 Khả năng sử dụng

- Mobile-first responsive design, hỗ trợ màn hình từ 320px
- Accessibility: WCAG 2.1 Level AA
- Hỗ trợ ngôn ngữ: Tiếng Việt (mặc định), Tiếng Anh
- Offline indicator rõ ràng khi không có mạng

### 5.5 Khả năng bảo trì

- Code coverage unit test: tối thiểu 80%
- API documentation: Swagger tự động cập nhật
- Health check endpoint `/health` cho mỗi service
- Feature flags: tắt/bật tính năng không cần deploy

---

## 6. THIẾT KẾ CƠ SỞ DỮ LIỆU

### 6.1 Công nghệ

**Database:** Microsoft SQL Server 2022  
**ORM:** Entity Framework Core 9 với provider `Microsoft.EntityFrameworkCore.SqlServer`  
**Migrations:** EF Core Migrations  
**Full-Text Search:** SQL Server Full-Text Search (dùng cho drug_catalog)

### 6.2 ERD Tổng quan

```
Users ──────────────────────────────────────────┐
  │                                              │
  ├─── FamilyMembers (1:N)                       │
  │       └── HealthProfiles (1:1)               │
  │                                              │
  ├─── HealthProfiles (1:1)                      │
  │       ├─── HealthMeasurements (1:N)          │
  │       ├─── BloodPressureLogs (1:N)           │
  │       └─── HealthAlerts (1:N)                │
  │                                              │
  ├─── MedicalVisits (1:N)                       │
  │       └─── MedicalDocuments (1:N)            │
  │                                              │
  ├─── Medications (1:N)                         │
  │       └─── MedicationSchedules (1:N)         │
  │                 └─── MedicationLogs (1:N)    │
  │                                              │
  ├─── VaccineRecords (1:N)                      │
  │       └── [ref] VaccineCatalog (N:1)         │
  │                                              │
  ├─── Reminders (1:N)                           │
  ├─── NotificationPreferences (1:1)             │
  ├─── SyncQueue (1:N)                           │
  └─── RefreshTokens (1:N)

Roles ─── UserRoles (N:N) ─── Users
Roles ─── RolePermissions (N:N) ─── Permissions

AuditLogs (standalone, append-only)
VaccineCatalog (master data)
VaccineScheduleRules (master data)
DrugCatalog (master data)
```

### 6.3 SQL Schema Chi Tiết (SQL Server 2022)

```sql
-- ============================================
-- USERS & AUTH
-- ============================================

CREATE TABLE Users (
    Id                 UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    Email              NVARCHAR(255)    NOT NULL,
    PasswordHash       NVARCHAR(255)    NOT NULL,
    Phone              NVARCHAR(20)     NULL,
    IsActive           BIT              NOT NULL DEFAULT 0,
    IsEmailVerified    BIT              NOT NULL DEFAULT 0,
    LastLoginAt        DATETIME2        NULL,
    FailedLoginCount   INT              NOT NULL DEFAULT 0,
    LockedUntil        DATETIME2        NULL,
    CreatedAt          DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt          DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DeletedAt          DATETIME2        NULL,
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

CREATE TABLE RefreshTokens (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    UserId      UNIQUEIDENTIFIER NOT NULL,
    TokenHash   NVARCHAR(255)    NOT NULL,
    ExpiresAt   DATETIME2        NOT NULL,
    IsRevoked   BIT              NOT NULL DEFAULT 0,
    DeviceInfo  NVARCHAR(MAX)    NULL,
    IpAddress   NVARCHAR(45)     NULL,
    CreatedAt   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_RefreshTokens_TokenHash UNIQUE (TokenHash),
    CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE Roles (
    Id   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    Name NVARCHAR(50)     NOT NULL,
    CONSTRAINT UQ_Roles_Name UNIQUE (Name)
    -- Values: 'user', 'family_admin', 'admin', 'readonly'
);

CREATE TABLE Permissions (
    Id       UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    Resource NVARCHAR(100)    NOT NULL,
    Action   NVARCHAR(50)     NOT NULL,
    CONSTRAINT UQ_Permissions_ResourceAction UNIQUE (Resource, Action)
);

CREATE TABLE UserRoles (
    UserId UNIQUEIDENTIFIER NOT NULL,
    RoleId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId)
        REFERENCES Roles(Id) ON DELETE CASCADE
);

CREATE TABLE RolePermissions (
    RoleId       UNIQUEIDENTIFIER NOT NULL,
    PermissionId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT PK_RolePermissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId)
        REFERENCES Roles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId)
        REFERENCES Permissions(Id) ON DELETE CASCADE
);

CREATE TABLE EmailVerifications (
    Id        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    UserId    UNIQUEIDENTIFIER NOT NULL,
    OtpHash   NVARCHAR(255)    NOT NULL,
    Type      NVARCHAR(50)     NOT NULL, -- 'register', 'reset_password', 'change_email'
    ExpiresAt DATETIME2        NOT NULL,
    UsedAt    DATETIME2        NULL,
    CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_EmailVerifications_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE
);

-- ============================================
-- FAMILY
-- ============================================

CREATE TABLE FamilyGroups (
    Id        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    Name      NVARCHAR(100)    NOT NULL,
    AdminId   UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DeletedAt DATETIME2        NULL,
    CONSTRAINT FK_FamilyGroups_Users FOREIGN KEY (AdminId)
        REFERENCES Users(Id)
);

CREATE TABLE FamilyMembers (
    Id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    FamilyGroupId   UNIQUEIDENTIFIER NOT NULL,
    UserId          UNIQUEIDENTIFIER NULL,     -- NULL nếu member không có tài khoản (trẻ em)
    FullName        NVARCHAR(255)    NOT NULL,
    DateOfBirth     DATE             NOT NULL,
    Gender          NVARCHAR(10)     NOT NULL,
    Relationship    NVARCHAR(50)     NULL,     -- 'self', 'spouse', 'child', 'parent', 'sibling'
    ManagedBy       UNIQUEIDENTIFIER NULL,     -- Ai quản lý hồ sơ này
    CreatedAt       DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DeletedAt       DATETIME2        NULL,
    CONSTRAINT FK_FamilyMembers_FamilyGroups FOREIGN KEY (FamilyGroupId)
        REFERENCES FamilyGroups(Id) ON DELETE CASCADE,
    CONSTRAINT FK_FamilyMembers_Users_UserId FOREIGN KEY (UserId)
        REFERENCES Users(Id),
    CONSTRAINT FK_FamilyMembers_Users_ManagedBy FOREIGN KEY (ManagedBy)
        REFERENCES Users(Id)
);

-- ============================================
-- HEALTH PROFILES
-- ============================================

CREATE TABLE HealthProfiles (
    Id                     UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    UserId                 UNIQUEIDENTIFIER NULL,
    FamilyMemberId         UNIQUEIDENTIFIER NULL,
    BloodType              NVARCHAR(5)      NULL,  -- 'A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'
    -- Mảng JSON: ["Penicillin","Aspirin"]
    Allergies              NVARCHAR(MAX)    NULL,
    -- Mảng JSON: ["Hypertension","Diabetes"]
    ChronicConditions      NVARCHAR(MAX)    NULL,
    EmergencyContactName   NVARCHAR(255)    NULL,
    EmergencyContactPhone  NVARCHAR(20)     NULL,
    InsuranceNumber        VARBINARY(MAX)   NULL,  -- AES-256 encrypted
    PrimaryDoctor          NVARCHAR(255)    NULL,
    Notes                  NVARCHAR(MAX)    NULL,
    CreatedAt              DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt              DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_HealthProfiles_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_HealthProfiles_FamilyMembers FOREIGN KEY (FamilyMemberId)
        REFERENCES FamilyMembers(Id) ON DELETE NO ACTION,
    CONSTRAINT CHK_HealthProfiles_Owner CHECK (
        (UserId IS NOT NULL AND FamilyMemberId IS NULL) OR
        (UserId IS NULL AND FamilyMemberId IS NOT NULL)
    )
);

-- Time-series measurements — KHÔNG ghi đè, luôn INSERT mới
CREATE TABLE HealthMeasurements (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    HealthProfileId  UNIQUEIDENTIFIER NOT NULL,
    MeasuredAt       DATETIME2        NOT NULL,
    WeightKg         DECIMAL(5,2)     NULL,
    HeightCm         DECIMAL(5,1)     NULL,
    -- BMI được tính trong application layer (EF Core computed / service)
    Bmi              AS (
                         CASE
                             WHEN HeightCm > 0 AND WeightKg IS NOT NULL AND HeightCm IS NOT NULL
                             THEN ROUND(
                                 CAST(WeightKg AS DECIMAL(10,4)) /
                                 (CAST(HeightCm AS DECIMAL(10,4)) / 100 *
                                  CAST(HeightCm AS DECIMAL(10,4)) / 100),
                             2)
                             ELSE NULL
                         END
                     ) PERSISTED,
    HeartRateBpm     INT              NULL,
    BodyTemperature  DECIMAL(4,1)     NULL,
    BloodGlucose     DECIMAL(5,2)     NULL,
    Spo2Percent      DECIMAL(4,1)     NULL,
    Notes            NVARCHAR(MAX)    NULL,
    Source           NVARCHAR(50)     NOT NULL DEFAULT 'manual', -- 'manual','device','import'
    CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_HealthMeasurements_HealthProfiles FOREIGN KEY (HealthProfileId)
        REFERENCES HealthProfiles(Id) ON DELETE CASCADE
);

CREATE TABLE BloodPressureLogs (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    HealthProfileId  UNIQUEIDENTIFIER NOT NULL,
    MeasuredAt       DATETIME2        NOT NULL,
    Systolic         INT              NOT NULL,
    Diastolic        INT              NOT NULL,
    Pulse            INT              NULL,
    Arm              NVARCHAR(5)      NOT NULL DEFAULT 'left',    -- 'left', 'right'
    Position         NVARCHAR(10)     NOT NULL DEFAULT 'sitting', -- 'sitting','standing','lying'
    Notes            NVARCHAR(MAX)    NULL,
    CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_BloodPressureLogs_HealthProfiles FOREIGN KEY (HealthProfileId)
        REFERENCES HealthProfiles(Id) ON DELETE CASCADE
);

CREATE TABLE HealthAlerts (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    HealthProfileId  UNIQUEIDENTIFIER NOT NULL,
    MeasurementId    UNIQUEIDENTIFIER NULL,
    BpLogId          UNIQUEIDENTIFIER NULL,
    AlertType        NVARCHAR(50)     NOT NULL, -- 'high_bp','low_spo2','high_bmi', ...
    Severity         NVARCHAR(10)     NOT NULL, -- 'warning', 'critical'
    Message          NVARCHAR(MAX)    NOT NULL,
    IsAcknowledged   BIT              NOT NULL DEFAULT 0,
    AcknowledgedAt   DATETIME2        NULL,
    CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_HealthAlerts_HealthProfiles FOREIGN KEY (HealthProfileId)
        REFERENCES HealthProfiles(Id) ON DELETE CASCADE
);

-- ============================================
-- MEDICAL HISTORY
-- ============================================

CREATE TABLE MedicalVisits (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    HealthProfileId  UNIQUEIDENTIFIER NOT NULL,
    VisitDate        DATE             NOT NULL,
    FacilityName     NVARCHAR(255)    NOT NULL,
    DoctorName       NVARCHAR(255)    NULL,
    ChiefComplaint   NVARCHAR(MAX)    NOT NULL,
    Diagnosis        NVARCHAR(MAX)    NOT NULL,
    Icd10Code        NVARCHAR(10)     NULL,
    Treatment        NVARCHAR(MAX)    NULL,
    FollowUpDate     DATE             NULL,
    Cost             DECIMAL(12,2)    NULL,
    Notes            NVARCHAR(MAX)    NULL,
    CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DeletedAt        DATETIME2        NULL,
    CONSTRAINT FK_MedicalVisits_HealthProfiles FOREIGN KEY (HealthProfileId)
        REFERENCES HealthProfiles(Id) ON DELETE CASCADE
);

CREATE TABLE MedicalDocuments (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    MedicalVisitId   UNIQUEIDENTIFIER NULL,
    HealthProfileId  UNIQUEIDENTIFIER NOT NULL,
    FileName         NVARCHAR(255)    NOT NULL,
    FileSizeBytes    BIGINT           NOT NULL,
    MimeType         NVARCHAR(100)    NOT NULL,
    StorageKey       NVARCHAR(500)    NOT NULL, -- path trong MinIO/S3
    ThumbnailKey     NVARCHAR(500)    NULL,
    DocumentType     NVARCHAR(50)     NULL,     -- 'prescription','test_result','xray','report','other'
    OcrStatus        NVARCHAR(20)     NOT NULL DEFAULT 'pending', -- 'pending','processing','done','failed'
    OcrText          NVARCHAR(MAX)    NULL,
    UploadedBy       UNIQUEIDENTIFIER NOT NULL,
    CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DeletedAt        DATETIME2        NULL,
    CONSTRAINT FK_MedicalDocuments_MedicalVisits FOREIGN KEY (MedicalVisitId)
        REFERENCES MedicalVisits(Id) ON DELETE SET NULL,
    CONSTRAINT FK_MedicalDocuments_HealthProfiles FOREIGN KEY (HealthProfileId)
        REFERENCES HealthProfiles(Id),
    CONSTRAINT FK_MedicalDocuments_Users FOREIGN KEY (UploadedBy)
        REFERENCES Users(Id)
);

-- ============================================
-- MEDICATIONS
-- ============================================

CREATE TABLE DrugCatalog (
    Id            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    NameGeneric   NVARCHAR(255)    NOT NULL,
    NameBrand     NVARCHAR(255)    NULL,
    DrugClass     NVARCHAR(100)    NULL,
    CommonDosages NVARCHAR(MAX)    NULL, -- JSON array
    CreatedAt     DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt     DATETIME2        NOT NULL DEFAULT GETUTCDATE()
);

-- Full-Text Search cho drug lookup
CREATE FULLTEXT CATALOG HealthPlusCatalog AS DEFAULT;
CREATE FULLTEXT INDEX ON DrugCatalog(NameGeneric, NameBrand)
    KEY INDEX PK__DrugCatalog__Id;

CREATE TABLE Medications (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    HealthProfileId  UNIQUEIDENTIFIER NOT NULL,
    MedicalVisitId   UNIQUEIDENTIFIER NULL,
    DrugCatalogId    UNIQUEIDENTIFIER NULL,
    DrugName         NVARCHAR(255)    NOT NULL,
    Strength         NVARCHAR(100)    NULL,
    DosageForm       NVARCHAR(100)    NULL,
    Instructions     NVARCHAR(MAX)    NULL,
    StartDate        DATE             NOT NULL,
    EndDate          DATE             NULL,
    IsOngoing        BIT              NOT NULL DEFAULT 0,
    OcrSourceDocId   UNIQUEIDENTIFIER NULL,
    ConfidenceScore  DECIMAL(3,2)     NULL,
    CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DeletedAt        DATETIME2        NULL,
    CONSTRAINT FK_Medications_HealthProfiles FOREIGN KEY (HealthProfileId)
        REFERENCES HealthProfiles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Medications_MedicalVisits FOREIGN KEY (MedicalVisitId)
        REFERENCES MedicalVisits(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Medications_DrugCatalog FOREIGN KEY (DrugCatalogId)
        REFERENCES DrugCatalog(Id) ON DELETE SET NULL
);

CREATE TABLE MedicationSchedules (
    Id                    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    MedicationId          UNIQUEIDENTIFIER NOT NULL,
    ScheduledTime         TIME             NOT NULL,
    DosageAmount          NVARCHAR(50)     NULL,
    ReminderEnabled       BIT              NOT NULL DEFAULT 1,
    ReminderMinutesBefore INT              NOT NULL DEFAULT 10,
    CreatedAt             DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_MedicationSchedules_Medications FOREIGN KEY (MedicationId)
        REFERENCES Medications(Id) ON DELETE CASCADE
);

CREATE TABLE MedicationLogs (
    Id                     UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    MedicationScheduleId   UNIQUEIDENTIFIER NOT NULL,
    ScheduledAt            DATETIME2        NOT NULL,
    TakenAt                DATETIME2        NULL,
    Status                 NVARCHAR(20)     NOT NULL DEFAULT 'pending', -- 'taken','skipped','pending'
    SkipReason             NVARCHAR(MAX)    NULL,
    CreatedAt              DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_MedicationLogs_MedicationSchedules FOREIGN KEY (MedicationScheduleId)
        REFERENCES MedicationSchedules(Id) ON DELETE CASCADE
);

-- ============================================
-- VACCINES
-- ============================================

CREATE TABLE VaccineCatalog (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    Name             NVARCHAR(255)    NOT NULL,
    ShortName        NVARCHAR(50)     NULL,
    DiseasesCovered  NVARCHAR(MAX)    NULL, -- JSON array
    TotalDoses       INT              NOT NULL,
    IsMandatory      BIT              NOT NULL DEFAULT 0, -- Có trong TCMR bắt buộc không
    AgeStartMonths   INT              NULL,
    Notes            NVARCHAR(MAX)    NULL,
    CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE()
);

CREATE TABLE VaccineScheduleRules (
    Id                    UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    VaccineCatalogId      UNIQUEIDENTIFIER NOT NULL,
    DoseNumber            INT              NOT NULL,
    MinAgeMonths          INT              NULL,
    MaxAgeMonths          INT              NULL,
    MinIntervalDays       INT              NULL, -- Khoảng cách tối thiểu từ mũi trước
    RecommendedIntervalDays INT            NULL,
    CONSTRAINT UQ_VaccineScheduleRules UNIQUE (VaccineCatalogId, DoseNumber),
    CONSTRAINT FK_VaccineScheduleRules_VaccineCatalog FOREIGN KEY (VaccineCatalogId)
        REFERENCES VaccineCatalog(Id) ON DELETE CASCADE
);

CREATE TABLE VaccineRecords (
    Id                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    HealthProfileId   UNIQUEIDENTIFIER NOT NULL,
    VaccineCatalogId  UNIQUEIDENTIFIER NULL,
    VaccineName       NVARCHAR(255)    NOT NULL,
    DoseNumber        INT              NOT NULL,
    InjectionDate     DATE             NOT NULL,
    NextDueDate       DATE             NULL,
    Facility          NVARCHAR(255)    NULL,
    LotNumber         NVARCHAR(100)    NULL,
    AdministeredBy    NVARCHAR(255)    NULL,
    Reaction          NVARCHAR(MAX)    NULL,
    DocumentId        UNIQUEIDENTIFIER NULL,
    CreatedAt         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    DeletedAt         DATETIME2        NULL,
    CONSTRAINT FK_VaccineRecords_HealthProfiles FOREIGN KEY (HealthProfileId)
        REFERENCES HealthProfiles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_VaccineRecords_VaccineCatalog FOREIGN KEY (VaccineCatalogId)
        REFERENCES VaccineCatalog(Id) ON DELETE SET NULL
);

-- ============================================
-- REMINDERS & NOTIFICATIONS
-- ============================================

CREATE TABLE NotificationPreferences (
    Id                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    UserId            UNIQUEIDENTIFIER NOT NULL,
    Timezone          NVARCHAR(50)     NOT NULL DEFAULT 'Asia/Ho_Chi_Minh',
    QuietStartTime    TIME             NOT NULL DEFAULT '22:00',
    QuietEndTime      TIME             NOT NULL DEFAULT '07:00',
    PushEnabled       BIT              NOT NULL DEFAULT 1,
    EmailEnabled      BIT              NOT NULL DEFAULT 1,
    VaccineReminder   BIT              NOT NULL DEFAULT 1,
    MedicationReminder BIT             NOT NULL DEFAULT 1,
    FollowupReminder  BIT              NOT NULL DEFAULT 1,
    HealthAlert       BIT              NOT NULL DEFAULT 1,
    UpdatedAt         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_NotificationPreferences_UserId UNIQUE (UserId),
    CONSTRAINT FK_NotificationPreferences_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE PushSubscriptions (
    Id          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    UserId      UNIQUEIDENTIFIER NOT NULL,
    FcmToken    NVARCHAR(MAX)    NOT NULL,
    DeviceType  NVARCHAR(20)     NULL, -- 'web', 'android', 'ios'
    IsActive    BIT              NOT NULL DEFAULT 1,
    LastUsedAt  DATETIME2        NULL,
    CreatedAt   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_PushSubscriptions_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE
);

CREATE TABLE Reminders (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    UserId           UNIQUEIDENTIFIER NOT NULL,
    HealthProfileId  UNIQUEIDENTIFIER NOT NULL,
    ReminderType     NVARCHAR(50)     NOT NULL, -- 'vaccine','medication','followup','custom'
    ReferenceId      UNIQUEIDENTIFIER NULL,
    Title            NVARCHAR(255)    NOT NULL,
    Body             NVARCHAR(MAX)    NULL,
    RemindAt         DATETIME2        NOT NULL,
    Status           NVARCHAR(20)     NOT NULL DEFAULT 'pending', -- 'pending','sent','failed','cancelled'
    SentAt           DATETIME2        NULL,
    CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT FK_Reminders_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE
);

-- ============================================
-- OFFLINE SYNC
-- ============================================

CREATE TABLE SyncQueue (
    Id            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    UserId        UNIQUEIDENTIFIER NOT NULL,
    Operation     NVARCHAR(10)     NOT NULL, -- 'create', 'update', 'delete'
    EntityType    NVARCHAR(50)     NOT NULL,
    EntityId      UNIQUEIDENTIFIER NOT NULL,
    Payload       NVARCHAR(MAX)    NOT NULL, -- JSON
    ClientVersion BIGINT           NOT NULL, -- Lamport timestamp từ client
    ServerVersion BIGINT           NULL,
    Status        NVARCHAR(20)     NOT NULL DEFAULT 'pending', -- 'pending','processing','done','conflict'
    ConflictData  NVARCHAR(MAX)    NULL,     -- JSON
    RetryCount    INT              NOT NULL DEFAULT 0,
    CreatedAt     DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    ProcessedAt   DATETIME2        NULL,
    CONSTRAINT FK_SyncQueue_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE
);

-- ============================================
-- AUDIT LOG (append-only, không xóa)
-- ============================================

CREATE TABLE AuditLogs (
    Id            BIGINT           NOT NULL IDENTITY(1,1) PRIMARY KEY,
    EventType     NVARCHAR(50)     NOT NULL,
    UserId        UNIQUEIDENTIFIER NULL,
    TargetUserId  UNIQUEIDENTIFIER NULL,
    Resource      NVARCHAR(100)    NOT NULL,
    Action        NVARCHAR(20)     NOT NULL,
    EntityId      UNIQUEIDENTIFIER NULL,
    OldValueHash  NVARCHAR(64)     NULL,
    NewValueHash  NVARCHAR(64)     NULL,
    IpAddress     NVARCHAR(45)     NULL,
    UserAgent     NVARCHAR(MAX)    NULL,
    CreatedAt     DATETIME2        NOT NULL DEFAULT GETUTCDATE()
    -- Không có FK để tránh cascade delete xóa mất audit log
);

-- ============================================
-- INDEXES
-- ============================================

CREATE INDEX IX_Users_Email         ON Users(Email);
CREATE INDEX IX_Users_Active        ON Users(DeletedAt) WHERE DeletedAt IS NULL;

CREATE INDEX IX_HealthMeasurements_ProfileTime
    ON HealthMeasurements(HealthProfileId, MeasuredAt DESC);

CREATE INDEX IX_BloodPressureLogs_ProfileTime
    ON BloodPressureLogs(HealthProfileId, MeasuredAt DESC);

CREATE INDEX IX_MedicalVisits_ProfileDate
    ON MedicalVisits(HealthProfileId, VisitDate DESC)
    WHERE DeletedAt IS NULL;

CREATE INDEX IX_VaccineRecords_Profile
    ON VaccineRecords(HealthProfileId, InjectionDate DESC)
    WHERE DeletedAt IS NULL;

CREATE INDEX IX_Reminders_RemindAt
    ON Reminders(RemindAt)
    WHERE Status = 'pending';

CREATE INDEX IX_MedicationLogs_Scheduled
    ON MedicationLogs(ScheduledAt)
    WHERE Status = 'pending';

CREATE INDEX IX_SyncQueue_UserStatus
    ON SyncQueue(UserId, Status, CreatedAt);

CREATE INDEX IX_AuditLogs_UserId
    ON AuditLogs(UserId, CreatedAt DESC);

CREATE INDEX IX_AuditLogs_TargetUserId
    ON AuditLogs(TargetUserId, CreatedAt DESC);

CREATE INDEX IX_RefreshTokens_UserId
    ON RefreshTokens(UserId)
    WHERE IsRevoked = 0;
```

---

## 7. KIẾN TRÚC HỆ THỐNG

### 7.1 Tổng quan kiến trúc

```
┌─────────────────────────────────────────────────────┐
│                    CLIENT LAYER                      │
│  Angular 20 PWA (Service Worker + IndexedDB)        │
└────────────────────┬────────────────────────────────┘
                     │ HTTPS
┌────────────────────▼────────────────────────────────┐
│                  NGINX (Reverse Proxy)               │
│  - SSL Termination                                  │
│  - Static file serving (Angular build)              │
│  - Rate Limiting                                    │
└────────────────────┬────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────┐
│              ASP.NET Core 9 Web API                  │
│  ┌─────────────────────────────────────────────┐   │
│  │  API Layer (Controllers + Middleware)        │   │
│  │  ├── JWT Middleware (RS256)                  │   │
│  │  ├── Rate Limiter Middleware                 │   │
│  │  ├── Audit Log Middleware                    │   │
│  │  └── Global Exception Handler               │   │
│  ├─────────────────────────────────────────────┤   │
│  │  Application Layer (CQRS / MediatR)          │   │
│  │  ├── Commands / Queries                     │   │
│  │  ├── Validators (FluentValidation)          │   │
│  │  └── Event Handlers                         │   │
│  ├─────────────────────────────────────────────┤   │
│  │  Domain Layer                               │   │
│  │  ├── Entities / Value Objects               │   │
│  │  ├── Domain Events                          │   │
│  │  └── Business Rules                         │   │
│  ├─────────────────────────────────────────────┤   │
│  │  Infrastructure Layer                       │   │
│  │  ├── EF Core (SQL Server)                   │   │
│  │  ├── MinIO/S3 Client                        │   │
│  │  ├── Email Service (SMTP)                   │   │
│  │  ├── FCM Service                            │   │
│  │  ├── OCR Service                            │   │
│  │  └── Background Jobs (Hangfire)             │   │
│  └─────────────────────────────────────────────┘   │
└──────┬──────────────┬────────────────┬──────────────┘
       │              │                │
┌──────▼──────┐ ┌─────▼──────┐ ┌──────▼──────┐
│ SQL Server  │ │   MinIO    │ │  Hangfire   │
│   2022      │ │  (Files)   │ │  (Jobs)     │
└─────────────┘ └────────────┘ └─────────────┘
```

### 7.2 Cấu trúc thư mục Backend (Clean Architecture)

```
src/
├── HealthCare.Domain/
│   ├── Entities/
│   │   ├── User.cs
│   │   ├── HealthProfile.cs
│   │   ├── HealthMeasurement.cs
│   │   ├── BloodPressureLog.cs
│   │   ├── MedicalVisit.cs
│   │   ├── MedicalDocument.cs
│   │   ├── Medication.cs
│   │   ├── VaccineRecord.cs
│   │   ├── Reminder.cs
│   │   └── ...
│   ├── ValueObjects/
│   │   ├── BloodPressure.cs
│   │   ├── Bmi.cs
│   │   └── Email.cs
│   ├── Events/
│   │   ├── MeasurementRecordedEvent.cs
│   │   └── VaccineDueEvent.cs
│   └── Enums/
│
├── HealthCare.Application/
│   ├── Common/
│   │   ├── Interfaces/
│   │   │   ├── IApplicationDbContext.cs
│   │   │   ├── IFileStorageService.cs
│   │   │   ├── IOcrService.cs
│   │   │   ├── IEmailService.cs
│   │   │   └── ICurrentUser.cs
│   │   └── Behaviours/
│   │       ├── ValidationBehaviour.cs
│   │       ├── LoggingBehaviour.cs
│   │       └── AuditBehaviour.cs
│   ├── Auth/
│   ├── HealthRecords/
│   ├── MedicalHistory/
│   ├── Vaccines/
│   ├── Medications/
│   ├── Reminders/
│   └── Analytics/
│
├── HealthCare.Infrastructure/
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   ├── Configurations/   (EF Fluent API)
│   │   └── Migrations/
│   ├── Services/
│   │   ├── FileStorageService.cs
│   │   ├── OcrService.cs
│   │   ├── EmailService.cs
│   │   └── FcmService.cs
│   └── BackgroundJobs/
│       ├── ReminderJob.cs
│       └── SyncProcessorJob.cs
│
└── HealthCare.API/
    ├── Controllers/
    ├── Middleware/
    └── Program.cs
```

### 7.3 NuGet Packages chính (Backend)

```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.*" />
<PackageReference Include="MediatR" Version="12.*" />
<PackageReference Include="AutoMapper" Version="13.*" />
<PackageReference Include="FluentValidation.AspNetCore" Version="11.*" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.*" />
<PackageReference Include="Hangfire.SqlServer" Version="1.*" />
<PackageReference Include="Minio" Version="6.*" />
<PackageReference Include="SixLabors.ImageSharp" Version="3.*" />
<PackageReference Include="iTextSharp.LGPLv2.Core" Version="3.*" />  <!-- PDF export -->
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.*" />
<PackageReference Include="Serilog.AspNetCore" Version="8.*" />
```

### 7.4 Cấu trúc thư mục Frontend (Angular 20)

```
src/
├── app/
│   ├── core/
│   │   ├── auth/
│   │   │   ├── auth.service.ts
│   │   │   ├── auth.store.ts        (Signal Store)
│   │   │   └── auth.guard.ts
│   │   ├── interceptors/
│   │   │   ├── auth.interceptor.ts
│   │   │   ├── retry.interceptor.ts
│   │   │   └── offline-queue.interceptor.ts
│   │   └── services/
│   │       ├── offline.service.ts
│   │       └── sync.service.ts
│   │
│   ├── shared/
│   │   ├── components/
│   │   │   ├── loading-spinner/
│   │   │   ├── alert-banner/
│   │   │   ├── offline-indicator/
│   │   │   └── chart-wrapper/
│   │   ├── pipes/
│   │   └── directives/
│   │
│   ├── features/
│   │   ├── auth/               (lazy loaded)
│   │   ├── health-records/     (lazy loaded)
│   │   ├── medical-history/    (lazy loaded)
│   │   ├── vaccines/           (lazy loaded)
│   │   ├── medications/        (lazy loaded)
│   │   ├── reminders/          (lazy loaded)
│   │   ├── dashboard/          (lazy loaded)
│   │   └── admin/              (lazy loaded)
│   │
│   ├── layouts/
│   │   ├── main-layout/
│   │   └── auth-layout/
│   │
│   └── app.routes.ts
│
├── environments/
│   ├── environment.ts
│   └── environment.prod.ts
├── sw.js              (Service Worker)
└── manifest.webmanifest
```

### 7.5 NPM Packages chính (Frontend)

```json
{
  "@angular/core": "^20.0.0",
  "@angular/material": "^20.0.0",
  "tailwindcss": "^3.4.0",
  "apexcharts": "^3.54.0",
  "ng-apexcharts": "^1.10.0",
  "@ngrx/signals": "^20.0.0",
  "rxjs": "^7.8.0",
  "idb": "^8.0.0",
  "workbox-sw": "^7.0.0"
}
```

---

## 8. ĐẶC TẢ API

### 8.1 Conventions

- Base URL: `https://api.healthplus.vn/api/v1`
- Authentication: `Authorization: Bearer <access_token>`
- Content-Type: `application/json`
- Phân trang: `?page=1&pageSize=20`
- Timestamps: ISO 8601 UTC (`2026-06-08T10:30:00Z`)

### 8.2 Response Format chuẩn

```json
// Success
{
  "success": true,
  "data": { },
  "meta": { "page": 1, "pageSize": 20, "total": 150 }
}

// Error
{
  "success": false,
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Dữ liệu không hợp lệ",
    "details": [
      { "field": "email", "message": "Email không đúng định dạng" }
    ]
  }
}
```

### 8.3 Danh sách Endpoints

```
AUTH
  POST   /auth/register
  POST   /auth/login
  POST   /auth/refresh
  POST   /auth/logout
  POST   /auth/forgot-password
  POST   /auth/reset-password
  POST   /auth/verify-email
  PUT    /auth/change-password

HEALTH PROFILES
  GET    /health-profiles/me
  PUT    /health-profiles/me

HEALTH MEASUREMENTS
  GET    /measurements?from=&to=&type=
  POST   /measurements
  DELETE /measurements/{id}

BLOOD PRESSURE
  GET    /blood-pressure?from=&to=
  POST   /blood-pressure
  DELETE /blood-pressure/{id}

MEDICAL VISITS
  GET    /medical-visits?page=&pageSize=
  POST   /medical-visits
  GET    /medical-visits/{id}
  PUT    /medical-visits/{id}
  DELETE /medical-visits/{id}

MEDICAL DOCUMENTS
  POST   /documents/upload
  GET    /documents/{id}/download      (returns signed URL)
  DELETE /documents/{id}
  POST   /documents/{id}/ocr
  GET    /documents/{id}/ocr-result

MEDICATIONS
  GET    /medications
  POST   /medications
  PUT    /medications/{id}
  DELETE /medications/{id}
  POST   /medications/{id}/schedules
  DELETE /medications/{id}/schedules/{scheduleId}
  POST   /medication-logs/{scheduleId}/taken
  POST   /medication-logs/{scheduleId}/skip

VACCINES
  GET    /vaccines
  POST   /vaccines
  PUT    /vaccines/{id}
  DELETE /vaccines/{id}
  GET    /vaccines/catalog
  GET    /vaccines/passport             (PDF export)

REMINDERS
  GET    /reminders?status=pending
  POST   /reminders
  PUT    /reminders/{id}
  DELETE /reminders/{id}

ANALYTICS
  GET    /analytics/health-score
  GET    /analytics/bmi-trend?from=&to=
  GET    /analytics/bp-trend?from=&to=
  GET    /analytics/vaccine-progress
  GET    /analytics/medication-compliance?from=&to=

FAMILY
  GET    /family
  POST   /family
  POST   /family/invite
  DELETE /family/members/{memberId}

NOTIFICATIONS
  GET    /notifications/preferences
  PUT    /notifications/preferences
  POST   /notifications/push-subscribe
  DELETE /notifications/push-subscribe/{tokenId}

SYNC
  POST   /sync/push
  GET    /sync/pull?since=<timestamp>

ADMIN
  GET    /admin/users?page=&search=
  PUT    /admin/users/{id}/status
  GET    /admin/audit-logs?userId=&from=&to=
  GET    /admin/stats
```

---

## 9. BẢO MẬT & TUÂN THỦ

### 9.1 Mã hóa

| Loại dữ liệu | Phương thức |
|---|---|
| Password | bcrypt, cost factor 12 |
| Số BHYT | AES-256-GCM (field-level, lưu VARBINARY(MAX)) |
| Refresh token | SHA-256 hash trong DB |
| OTP | SHA-256 hash |
| Files trong MinIO | Server-side encryption (AES-256) |
| Transport | TLS 1.2+ bắt buộc |

### 9.2 JWT Configuration

```
Access Token:
  - Algorithm: RS256 (asymmetric — private key ký, public key verify)
  - Expiry: 15 phút
  - Claims: sub, email, roles, jti

Refresh Token:
  - Random 64-byte, lưu SHA-256 hash trong SQL Server
  - Expiry: 7 ngày
  - Rotation: mỗi lần dùng phát token mới, invalidate token cũ
  - Revoke all khi: đổi mật khẩu, logout all devices
```

### 9.3 Security Headers (Nginx)

```nginx
add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;
add_header X-Content-Type-Options "nosniff" always;
add_header X-Frame-Options "DENY" always;
add_header Content-Security-Policy "default-src 'self'" always;
add_header Referrer-Policy "strict-origin-when-cross-origin" always;
```

### 9.4 SQL Server Specific Security

```sql
-- Tạo login riêng, không dùng sa
CREATE LOGIN healthplus_app WITH PASSWORD = '...';
CREATE USER healthplus_app FOR LOGIN healthplus_app;

-- Chỉ cấp quyền cần thiết (least privilege)
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::dbo TO healthplus_app;
DENY DROP TABLE, ALTER TABLE TO healthplus_app;

-- Enable Transparent Data Encryption (TDE) để mã hóa toàn bộ database file
CREATE DATABASE ENCRYPTION KEY
    WITH ALGORITHM = AES_256
    ENCRYPTION BY SERVER CERTIFICATE HealthPlusCert;
ALTER DATABASE HealthPlus SET ENCRYPTION ON;
```

### 9.5 Tuân thủ pháp lý Việt Nam

- **Nghị định 13/2023/NĐ-CP:** Có form đồng ý (consent) khi đăng ký
- **Quyền xóa dữ liệu:** Người dùng yêu cầu → soft delete toàn bộ, hard delete sau 30 ngày
- **Quyền export:** `/api/v1/users/me/export` trả về ZIP toàn bộ dữ liệu
- **Thông báo vi phạm:** Nếu breach, thông báo trong 72 giờ

---

## 10. OFFLINE FIRST & ĐỒNG BỘ DỮ LIỆU

### 10.1 Kiến trúc Offline (Angular PWA)

```
Service Worker (workbox-sw)
  ├── Cache Strategy: StaleWhileRevalidate cho API GET
  ├── Cache Strategy: CacheFirst cho static assets
  └── Background Sync: gửi SyncQueue khi online

IndexedDB (idb library)
  ├── Stores: healthProfiles, measurements, medicalVisits,
  │          vaccines, medications, syncQueue
  └── Schema versioning: upgrade callback mỗi version

Sync Service
  ├── Online/Offline detection (navigator.onLine + fetch probe)
  ├── Push: gom batch operations → POST /sync/push
  └── Pull: GET /sync/pull?since={lastSyncTimestamp}
```

### 10.2 Chiến lược Conflict Resolution

| Loại dữ liệu | Chiến lược | Lý do |
|---|---|---|
| Profile tĩnh | Last Write Wins (by timestamp) | Ít thay đổi |
| Measurements | Append-only, không conflict | Mỗi đo INSERT mới |
| Medical visits | Last Write Wins | User tự quản lý |
| Vaccine records | Last Write Wins | Ít thay đổi |
| Medication schedules | Server Wins | Tránh miss dose |

### 10.3 Sync Protocol

```json
// PUSH: POST /sync/push
{
  "operations": [
    {
      "operation": "create",
      "entityType": "health_measurement",
      "entityId": "guid",
      "payload": { "weightKg": 65.5, "measuredAt": "2026-06-08T08:00:00Z" },
      "clientVersion": 1749369600000,
      "clientTimestamp": "2026-06-08T08:00:00Z"
    }
  ]
}

// PULL: GET /sync/pull?since=1749369600000
// Response: { "changes": [...], "serverTimestamp": 1749369700000 }
```

---

## 11. THÔNG BÁO THÔNG MINH

### 11.1 Reminder Engine (Hangfire)

```
Recurring Job: Chạy mỗi 15 phút
1. Query Reminders WHERE Status='pending' AND RemindAt <= DATEADD(MINUTE,15,GETUTCDATE())
2. Với mỗi reminder:
   a. Load NotificationPreferences của user
   b. Convert RemindAt sang timezone của user
   c. Nếu trong QuietHours: delay đến QuietEndTime
   d. Gửi FCM Push Notification
   e. Nếu push fail: fallback sang Email
   f. Cập nhật Status = 'sent', SentAt = GETUTCDATE()

Daily Job: Chạy lúc 00:00 UTC
  - Tạo MedicationLogs cho ngày N+30
  - Tạo Reminders cho vaccine/followup sắp tới
  - Cleanup Reminders đã sent > 7 ngày
```

---

## 12. OCR PIPELINE

### 12.1 Provider

```
Primary:   Google Vision API (Document AI)
           - Tốt nhất cho tiếng Việt viết tay
           - Confidence score per word
           - Cost: ~$1.50/1000 pages

Fallback:  EasyOCR (self-hosted Docker container)
           - Dùng khi Google Vision không khả dụng
           - Hỗ trợ tiếng Việt (vie model)
```

### 12.2 Abstraction Layer

```csharp
public interface IOcrService
{
    Task<OcrResult> ExtractTextAsync(Stream imageStream, CancellationToken ct);
    Task<PrescriptionData> ExtractPrescriptionAsync(Stream imageStream, CancellationToken ct);
}

// GoogleVisionOcrService : IOcrService  (primary)
// EasyOcrService : IOcrService          (fallback)
// OcrServiceProxy : IOcrService         (circuit breaker + fallback logic)
```

### 12.3 Regex Extraction (Tiếng Việt)

```csharp
// Tên thuốc
@"^[A-ZÁÀẢÃẠĂẮẰẲẴẶÂẤẦẨẪẬ][a-zA-ZÀ-ỹ\s]+(?:\d+mg|\d+mcg|\d+g)?"

// Liều dùng
@"(\d+)\s*(viên|gói|ml|mg|mcg)"

// Tần suất
@"(\d+)\s*lần\s*/\s*(ngày|tuần|tháng)"

// Hướng dẫn uống
@"(sau|trước|trong khi)\s*ăn"

// Số ngày điều trị
@"(?:trong|uống)\s*(\d+)\s*ngày"
```

---

## 13. DEVOPS & TRIỂN KHAI

### 13.1 Docker Compose

```yaml
version: '3.9'

services:
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./nginx/ssl:/etc/nginx/ssl:ro
    depends_on: [api, frontend]

  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile
    environment:
      - API_URL=https://api.healthplus.vn

  api:
    build:
      context: ./backend
      dockerfile: Dockerfile
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=Server=sqlserver;Database=HealthPlus;User Id=healthplus_app;Password=${DB_PASSWORD};TrustServerCertificate=True;
      - Jwt__PrivateKeyPath=/run/secrets/jwt_private.pem
      - Storage__Endpoint=http://minio:9000
      - Storage__AccessKey=${MINIO_ACCESS_KEY}
      - Storage__SecretKey=${MINIO_SECRET_KEY}
    depends_on:
      sqlserver:
        condition: service_healthy
      minio:
        condition: service_started
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 10s
      retries: 3

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=${SA_PASSWORD}
      - MSSQL_PID=Express          # Dùng Express edition (miễn phí, giới hạn 10GB)
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: ["CMD", "/opt/mssql-tools/bin/sqlcmd", "-S", "localhost",
             "-U", "sa", "-P", "${SA_PASSWORD}", "-Q", "SELECT 1"]
      interval: 30s
      timeout: 10s
      retries: 5

  minio:
    image: minio/minio:latest
    command: server /data --console-address ":9001"
    volumes:
      - minio_data:/data
    environment:
      - MINIO_ROOT_USER=${MINIO_ACCESS_KEY}
      - MINIO_ROOT_PASSWORD=${MINIO_SECRET_KEY}

  ocr-service:
    image: jaidedai/easyocr:latest
    # EasyOCR REST wrapper (fallback khi Google Vision unavailable)

volumes:
  sqlserver_data:
  minio_data:
```

### 13.2 Dockerfile — Backend

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["HealthCare.API/HealthCare.API.csproj", "HealthCare.API/"]
COPY ["HealthCare.Application/HealthCare.Application.csproj", "HealthCare.Application/"]
COPY ["HealthCare.Domain/HealthCare.Domain.csproj", "HealthCare.Domain/"]
COPY ["HealthCare.Infrastructure/HealthCare.Infrastructure.csproj", "HealthCare.Infrastructure/"]
RUN dotnet restore "HealthCare.API/HealthCare.API.csproj"
COPY . .
RUN dotnet publish "HealthCare.API/HealthCare.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "HealthCare.API.dll"]
```

### 13.3 Dockerfile — Frontend

```dockerfile
FROM node:22-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci
COPY . .
RUN npm run build -- --configuration production

FROM nginx:alpine AS runtime
COPY --from=build /app/dist/healthcare/browser /usr/share/nginx/html
COPY nginx/default.conf /etc/nginx/conf.d/default.conf
```

### 13.4 CI/CD GitHub Actions

```yaml
# .github/workflows/ci.yml
on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

jobs:
  test-backend:
    runs-on: ubuntu-latest
    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: TestPassword123!
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '9.x' }
      - run: dotnet restore
      - run: dotnet test --collect:"XPlat Code Coverage"
      - name: Fail if coverage < 80%
        run: dotnet reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage -reporttypes:TextSummary

  test-frontend:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with: { node-version: '22' }
      - run: npm ci
      - run: npm run test -- --watch=false --browsers=ChromeHeadless

  build-and-push:
    needs: [test-backend, test-frontend]
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - name: Build & push Docker images
        run: |
          docker build -t healthplus/api:${{ github.sha }} ./backend
          docker build -t healthplus/frontend:${{ github.sha }} ./frontend
          # Push to registry...

  deploy:
    needs: build-and-push
    environment: production    # Requires manual approval
    runs-on: ubuntu-latest
    steps:
      - name: Deploy via SSH
        run: |
          # SSH to server, pull images, docker-compose up -d
          # Run health check
          # Rollback nếu fail
```

### 13.5 Backup Strategy (SQL Server)

```sql
-- Full backup hàng ngày (2:00 AM)
BACKUP DATABASE HealthPlus
    TO DISK = '/backup/healthplus_full_$(date +%Y%m%d).bak'
    WITH COMPRESSION, CHECKSUM;

-- Transaction log backup mỗi giờ
BACKUP LOG HealthPlus
    TO DISK = '/backup/healthplus_log_$(date +%Y%m%d_%H).bak';

-- Retention: giữ 30 ngày, xóa file cũ hơn
```

---

## 14. CHIẾN LƯỢC KIỂM THỬ

### 14.1 Unit Tests (40 tests)

```
Auth Domain (10):
  ✓ Password validation: đúng/sai từng rule
  ✓ Email format validation
  ✓ Brute force lockout: sau 5 lần sai
  ✓ OTP hash và verify
  ✓ Token expiry check

Health Domain (15):
  ✓ BMI calculation (ngưỡng châu Á: 23.0 thừa cân)
  ✓ BP classification: normal/elevated/stage1/stage2/crisis
  ✓ Health alert threshold: 9 loại chỉ số
  ✓ Weight change percentage alert (>5%/tháng)
  ✓ BMI tính từ weight/height (null safety)

Vaccine Domain (10):
  ✓ Next due date: 8 loại vaccine
  ✓ Catch-up khi quá hạn
  ✓ Overdue detection

Reminder Domain (5):
  ✓ Quiet hours check (timezone-aware: Asia/Ho_Chi_Minh)
  ✓ Medication compliance rate calculation
  ✓ Reminder scheduling
```

### 14.2 Integration Tests (20 tests)

```
Auth Flow (5):
  ✓ Register → Verify Email → Login → Refresh → Logout
  ✓ Brute force: 5 sai → khóa 15 phút
  ✓ Reset password flow đầy đủ
  ✓ Token rotation: dùng cùng refresh token 2 lần → revoke
  ✓ Concurrent login 2 thiết bị

Health Record Flow (8):
  ✓ Tạo measurement → BMI computed column chính xác
  ✓ BP vượt ngưỡng → HealthAlert được tạo
  ✓ Upload document → OCR trigger → result lưu vào DB
  ✓ Family member: tạo hồ sơ cho con → quản lý vaccine
  ✓ Medical visit CRUD với documents
  ✓ Medication schedule → auto-create reminders
  ✓ Vaccine record → next_due_date calculation
  ✓ Analytics query trả đúng time-range

Sync Flow (7):
  ✓ Push offline operations → apply đúng thứ tự (Lamport clock)
  ✓ Conflict detection và resolution
  ✓ Pull changes từ server
  ✓ Idempotency: gửi cùng operation 2 lần → không duplicate
  ✓ Soft delete sync
  ✓ Concurrent edits từ 2 thiết bị
  ✓ Partial sync khi timeout
```

### 14.3 E2E Tests — Playwright (15 tests)

```
Happy Path (8):
  ✓ Đăng ký → xác nhận email → đăng nhập
  ✓ Nhập cân nặng → xem BMI + classification
  ✓ Nhập huyết áp → xem trend chart (ApexCharts)
  ✓ Thêm vaccine → kiểm tra next_due_date đúng
  ✓ Upload toa thuốc → OCR → lưu medication
  ✓ Đặt reminder → nhận push notification
  ✓ Tạo family group → thêm con → quản lý vaccine
  ✓ Xem dashboard với đủ 7 biểu đồ

Edge Cases (7):
  ✓ Offline: nhập measurement → reconnect → sync thành công
  ✓ OCR ảnh mờ → confidence thấp → user review dialog
  ✓ Vaccine quá hạn → cảnh báo đỏ → catch-up schedule
  ✓ Quiet hours: không nhận push lúc 23:00
  ✓ Access token hết hạn → auto refresh → request retry
  ✓ Export Vaccine Passport PDF
  ✓ Admin: vô hiệu hóa user → user không đăng nhập được
```

---

## 15. KẾ HOẠCH SPRINT

| Sprint | Tuần | Nội dung | Deliverables |
|---|---|---|---|
| Sprint 0 | 1 | Setup & Foundation | Solution structure, CI/CD, Docker local |
| Sprint 1 | 2-3 | Authentication | Register/Login/JWT/RBAC/Rate limiting |
| Sprint 2 | 4-5 | Health Profile & Measurements | Time-series, BMI, BP, Alerts |
| Sprint 3 | 6-7 | Medical History & Documents | Visits CRUD, MinIO upload, Signed URL |
| Sprint 4 | 8-9 | OCR Pipeline | Google Vision, EasyOCR fallback, Drug catalog |
| Sprint 5 | 10-11 | Vaccines & Reminders | Catalog TCMR, Schedule engine, Hangfire, FCM |
| Sprint 6 | 12-13 | Dashboard & Analytics | ApexCharts, Health Score, Compliance |
| Sprint 7 | 14-15 | Offline First | IndexedDB, Sync Protocol, Conflict Resolution |
| Sprint 8 | 16 | Admin & Polish | Audit log UI, Vaccine Passport PDF, PWA icons |
| Sprint 9 | 17 | Testing & Deploy | Full test suite, Production deployment |

**Tổng thời gian ước tính: 17 tuần (~4.5 tháng)**

---

## 16. RỦI RO & GIẢM THIỂU

| Rủi ro | Mức độ | Giảm thiểu |
|---|---|---|
| OCR tiếng Việt viết tay kém | Cao | User review bắt buộc với confidence < 75%; Google Vision > Tesseract |
| Data breach dữ liệu y tế | Cao | TDE SQL Server; field-level encryption; audit log; pentest trước go-live |
| Offline sync conflict | Trung bình | Strategy cụ thể từng loại entity; test kỹ scenario concurrent |
| FCM push không đến | Trung bình | Fallback Email; retry 3 lần; status tracking |
| SQL Server Express 10GB limit | Trung bình | Monitor DB size; migrate sang Standard khi gần đạt giới hạn |
| Chi phí Google Vision API | Thấp | Cache kết quả OCR; EasyOCR fallback miễn phí |
| Angular version compatibility | Thấp | Lock major version; update theo schedule |

---

## PHỤ LỤC

### A. Environment Variables

```env
# SQL Server
ConnectionStrings__Default=Server=sqlserver;Database=HealthPlus;User Id=healthplus_app;Password=...;TrustServerCertificate=True;

# JWT (RS256)
Jwt__PrivateKeyPath=/run/secrets/jwt_private.pem
Jwt__PublicKeyPath=/run/secrets/jwt_public.pem
Jwt__AccessTokenExpiryMinutes=15
Jwt__RefreshTokenExpiryDays=7
Jwt__Issuer=https://api.healthplus.vn
Jwt__Audience=https://healthplus.vn

# Storage (MinIO)
Storage__Endpoint=http://minio:9000
Storage__AccessKey=...
Storage__SecretKey=...
Storage__BucketDocuments=health-documents
Storage__UseSSL=false

# Email (SMTP)
Email__Host=smtp.gmail.com
Email__Port=587
Email__Username=noreply@healthplus.vn
Email__Password=...
Email__FromName=Health+

# Firebase Cloud Messaging
Fcm__ServerKey=...
Fcm__ProjectId=...

# OCR
Ocr__GoogleVisionApiKey=...
Ocr__EasyOcrUrl=http://ocr-service:5000
Ocr__ConfidenceThreshold=0.75

# Hangfire
Hangfire__DashboardUser=admin
Hangfire__DashboardPass=...

# Feature Flags
Features__OcrEnabled=true
Features__FamilyGroupEnabled=true
Features__OfflineSyncEnabled=true
```

### B. Health Check Endpoints

```
GET /health          → Overall (200 = healthy)
GET /health/db       → SQL Server connectivity
GET /health/storage  → MinIO connectivity
GET /health/email    → SMTP connectivity
GET /health/fcm      → Firebase connectivity
```

### C. Error Codes

```
AUTH_001  Email đã tồn tại
AUTH_002  Email hoặc mật khẩu không đúng
AUTH_003  Tài khoản bị khóa tạm thời
AUTH_004  Access token hết hạn
AUTH_005  Refresh token không hợp lệ hoặc đã bị thu hồi
AUTH_006  OTP không đúng hoặc hết hạn

HEALTH_001  Hồ sơ sức khỏe không tìm thấy
HEALTH_002  Giá trị đo lường ngoài khoảng cho phép

FILE_001  File quá lớn (tối đa 10MB)
FILE_002  Loại file không được hỗ trợ
FILE_003  Lỗi lưu trữ

OCR_001   Không thể xử lý ảnh (chất lượng quá thấp)
OCR_002   OCR service không khả dụng

SYNC_001  Version conflict — cần resolve thủ công
SYNC_002  Entity không tồn tại trên server
SYNC_003  Payload không hợp lệ
```

---

*Tài liệu này được soạn thảo theo tiêu chuẩn IEEE 830 SRS và thực tiễn quản trị hệ thống y tế.*  
*Phiên bản: 1.1.0 | Ngày: 2026-06-08 | Health+ Project*  
*Database: SQL Server 2022 | Backend: ASP.NET Core 9 | Frontend: Angular 20*
