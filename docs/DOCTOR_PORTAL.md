# ĐẶC TẢ — DOCTOR PORTAL & CHIA SẺ HỒ SƠ
# Health+ | E-Health Record & Vaccine Tracker
---
**Phiên bản:** 1.0.0
**Ngày:** 2026-07-09
**Trạng thái:** Draft — Chờ phê duyệt
**Tài liệu liên quan:** SRS_HealthPlus.md v1.1.0 (§1.5 RBAC, §2.2 Ủy quyền truy cập)

---

## MỤC LỤC

1. [Tổng quan & Mục tiêu](#1-tổng-quan--mục-tiêu)
2. [Vai trò & Ma trận phân quyền](#2-vai-trò--ma-trận-phân-quyền)
3. [Module A — Chia sẻ hồ sơ có thời hạn (ShareGrant)](#3-module-a--chia-sẻ-hồ-sơ-có-thời-hạn)
4. [Module B1 — Đăng ký & Xác minh bác sĩ](#4-module-b1--đăng-ký--xác-minh-bác-sĩ)
5. [Module B2 — Liên kết Bác sĩ ↔ Bệnh nhân](#5-module-b2--liên-kết-bác-sĩ--bệnh-nhân)
6. [Module B3 — Doctor Dashboard](#6-module-b3--doctor-dashboard)
7. [Module B4 — Thông báo cho bác sĩ](#7-module-b4--thông-báo-cho-bác-sĩ)
8. [Thiết kế cơ sở dữ liệu](#8-thiết-kế-cơ-sở-dữ-liệu)
9. [Đặc tả API](#9-đặc-tả-api)
10. [Cấu trúc file Backend](#10-cấu-trúc-file-backend)
11. [Cấu trúc file Frontend](#11-cấu-trúc-file-frontend)
12. [Bảo mật & Tuân thủ](#12-bảo-mật--tuân-thủ)
13. [Kế hoạch Sprint](#13-kế-hoạch-sprint)
14. [Ngoài phạm vi (Giai đoạn C)](#14-ngoài-phạm-vi-giai-đoạn-c)

---

## 1. TỔNG QUAN & MỤC TIÊU

### 1.1 Vấn đề

Health+ hiện là sổ y tế cá nhân/gia đình khép kín. Người dùng không có cách nào cho
bác sĩ xem hồ sơ của mình một cách an toàn, và bác sĩ không có công cụ theo dõi
tình trạng sức khỏe của nhiều bệnh nhân đã đồng ý chia sẻ.

### 1.2 Mục tiêu

| # | Mục tiêu | Đo lường |
|---|---|---|
| G1 | Người dùng chia sẻ hồ sơ cho bất kỳ ai qua link/QR có thời hạn | Link hoạt động, tự hết hạn, revoke được |
| G2 | Bác sĩ có tài khoản được xác minh chứng chỉ hành nghề | Admin duyệt thủ công 100% hồ sơ |
| G3 | Bác sĩ theo dõi danh sách bệnh nhân đã đồng ý liên kết | Dashboard nhiều bệnh nhân, read-only |
| G4 | Bác sĩ nhận cảnh báo khi bệnh nhân có chỉ số bất thường | Push/Email trong 15 phút (Reminder Engine) |
| G5 | Mọi truy cập của bác sĩ đều được ghi vết | 100% lượt xem có AuditLog với TargetUserId |

### 1.3 Nguyên tắc thiết kế

1. **Consent-first:** Không có bất kỳ truy cập nào không xuất phát từ hành động đồng ý
   rõ ràng của bệnh nhân (tạo link chia sẻ, hoặc bấm chấp nhận lời mời liên kết).
2. **Read-only ở MVP:** Bác sĩ chỉ xem. Ghi chú/chỉ định của bác sĩ thuộc Giai đoạn C.
3. **Tái sử dụng tối đa:** Trang bác sĩ xem bệnh nhân dùng lại toàn bộ Queries + widgets
   dashboard hiện có, chỉ thay lớp kiểm tra quyền.
4. **Thu hồi tức thì:** Bệnh nhân revoke → bác sĩ mất quyền ngay ở request kế tiếp.

### 1.4 Phân giai đoạn

```
Giai đoạn A  ── ShareGrant (link chia sẻ có thời hạn)        ← làm trước, nền móng
Giai đoạn B  ── Doctor Portal MVP (B1 → B2 → B3 → B4)        ← tài liệu này
Giai đoạn C  ── Ghi chú bác sĩ, đặt lịch, telemedicine        ← ngoài phạm vi (mục 14)
```

---

## 2. VAI TRÒ & MA TRẬN PHÂN QUYỀN

### 2.1 Role mới

| Role | Mô tả | Cách có được |
|---|---|---|
| `doctor` | Bác sĩ đã xác minh chứng chỉ hành nghề | Đăng ký hồ sơ bác sĩ → admin duyệt → hệ thống gán role |

> Role `doctor` **cộng thêm** vào role `user` sẵn có (bác sĩ vẫn có hồ sơ sức khỏe cá nhân
> của chính mình như người dùng thường). Không thay thế.

### 2.2 Ma trận phân quyền (bổ sung vào RBAC hiện tại)

| Resource | user | doctor (với bệnh nhân đã liên kết) | admin |
|---|---|---|---|
| Hồ sơ sức khỏe của bệnh nhân liên kết | — | **read** | read (audit) |
| Measurements / BP logs của bệnh nhân | — | **read** | — |
| Medical visits + documents của bệnh nhân | — | **read** | — |
| Medications của bệnh nhân | — | **read** | — |
| Vaccine records của bệnh nhân | — | **read** | — |
| Health alerts của bệnh nhân | — | **read + acknowledge** | — |
| ShareGrant của chính mình | CRUD | CRUD | — |
| PatientDoctorLink | accept/reject/revoke (phía mình) | create-invite/view (phía mình) | read |
| DoctorProfile | — | CRUD (của mình) | approve/reject |

### 2.3 Cơ chế kiểm tra quyền (Authorization Policy)

```
Policy "LinkedDoctor":
  1. User có role 'doctor'                          (JWT claim)
  2. DoctorProfile.Status == Approved               (DB check, cache 5 phút)
  3. Tồn tại PatientDoctorLink:
       DoctorUserId == currentUser
       AND HealthProfileId == {profileId trong route}
       AND Status == Active
       AND RevokedAt IS NULL
  → Fail bất kỳ điều kiện nào ⇒ 403 Forbidden
```

Implement bằng `IAuthorizationHandler` + resource-based authorization, KHÔNG hard-code
trong từng handler. Mọi query của bác sĩ đi qua cùng một `DoctorAccessGuard`.

---

## 3. MODULE A — CHIA SẺ HỒ SƠ CÓ THỜI HẠN

### 3.1 User Story

> Là người dùng, tôi muốn tạo một link (kèm mã QR) cho phép người khác — thường là
> bác sĩ khi tôi đi khám — xem hồ sơ sức khỏe của tôi trong thời gian giới hạn,
> và tôi có thể thu hồi bất cứ lúc nào.

### 3.2 Luồng chính

```
[Bệnh nhân] Trang "Chia sẻ hồ sơ"
     │  chọn: phạm vi dữ liệu (measurements/visits/medications/vaccines)
     │        thời hạn (1 giờ / 24 giờ / 7 ngày)
     ▼
[API] POST /share-grants
     │  - sinh token 64-byte random → lưu SHA-256 hash
     │  - trả về URL: {FrontendUrl}/shared/{token} + QR code data
     ▼
[Bệnh nhân] gửi link / đưa QR cho bác sĩ quét
     ▼
[Bác sĩ - KHÔNG cần tài khoản] mở /shared/{token}
     │  - API validate: hash khớp, chưa hết hạn, chưa revoke
     │  - trả dữ liệu read-only theo đúng scope đã chọn
     │  - tăng AccessCount, ghi AuditLog (IP, UserAgent)
     ▼
[Bệnh nhân] tab "Đang chia sẻ": thấy danh sách link + số lượt xem → Revoke
```

### 3.3 Luồng ngoại lệ

- Token sai / hết hạn / đã revoke → HTTP 404 (không phân biệt lý do, tránh dò token)
- Rate limit trang public: 30 requests/phút/IP
- Quá 3 link đang active trên 1 hồ sơ → yêu cầu revoke bớt (chống spam link)

### 3.4 Quy tắc nghiệp vụ

| Quy tắc | Giá trị |
|---|---|
| Thời hạn cho phép | 1h, 24h, 7 ngày (mặc định 24h) |
| Số link active tối đa / hồ sơ | 3 |
| Scope mặc định | Toàn bộ trừ documents gốc (chỉ metadata) |
| Token | 64-byte random, chỉ lưu SHA-256 hash — như RefreshToken hiện tại |
| Family member | `family_admin` tạo được link cho hồ sơ thành viên mình quản lý |

---

## 4. MODULE B1 — ĐĂNG KÝ & XÁC MINH BÁC SĨ

### 4.1 Luồng đăng ký

```
[User đã có tài khoản] bấm "Đăng ký làm bác sĩ"
     ▼
Form: Họ tên đầy đủ · Số chứng chỉ hành nghề (CCHN) · Chuyên khoa
      Nơi công tác · Upload ảnh CCHN (bắt buộc, tối đa 2 file, 10MB/file)
     ▼
[API] POST /doctor/register → DoctorProfile (Status = Pending)
     │  Ảnh CCHN lưu MinIO bucket riêng 'doctor-licenses' (KHÔNG dùng chung
     │  bucket health-documents), signed URL chỉ cấp cho admin
     ▼
[Admin] Trang "Duyệt bác sĩ": xem hồ sơ + ảnh CCHN
     │  ├─ Approve → Status=Approved, gán role 'doctor', gửi email chúc mừng
     │  └─ Reject  → Status=Rejected + lý do, gửi email, cho phép nộp lại
     ▼
[Hệ thống] AuditLog: ai duyệt, lúc nào, hồ sơ nào
```

### 4.2 Trạng thái DoctorProfile

```
Pending ──approve──► Approved ──admin thu hồi──► Suspended
   │                                                  │
   └──reject──► Rejected ──nộp lại──► Pending ◄───────┘ (khiếu nại)
```

- `Suspended`: mất role `doctor` ngay, mọi PatientDoctorLink chuyển Inactive,
  bệnh nhân nhận thông báo.

### 4.3 Validation

| Trường | Quy tắc |
|---|---|
| Số CCHN | Bắt buộc, 6-20 ký tự, unique trong hệ thống |
| Chuyên khoa | Chọn từ danh mục cố định (nội, nhi, sản, da liễu, ...) |
| Ảnh CCHN | JPG/PNG/PDF, tối đa 10MB, bắt buộc ≥ 1 file |
| Tài khoản | Email đã verify, không bị khóa |

---

## 5. MODULE B2 — LIÊN KẾT BÁC SĨ ↔ BỆNH NHÂN

### 5.1 Hai chiều khởi tạo

**Chiều 1 — Bác sĩ mời bệnh nhân (phổ biến khi khám trực tiếp):**
```
Bác sĩ nhập email bệnh nhân → hệ thống gửi email + notification in-app
→ Bệnh nhân mở app, thấy "BS. X (BV Y) muốn theo dõi hồ sơ của bạn"
→ Bệnh nhân chọn phạm vi dữ liệu cho phép → bấm Đồng ý / Từ chối
```

**Chiều 2 — Bệnh nhân mời bác sĩ:**
```
Bệnh nhân tìm bác sĩ theo tên/CCHN trong danh bạ bác sĩ đã xác minh
→ gửi lời mời → bác sĩ chấp nhận
```

### 5.2 Consent (trọng tâm tuân thủ Nghị định 13/2023/NĐ-CP)

Khi bệnh nhân bấm Đồng ý, hệ thống lưu **bản ghi consent bất biến**:

| Trường | Nội dung |
|---|---|
| ConsentAt | Thời điểm chính xác (UTC) |
| ConsentScope | JSON: modules được phép xem |
| ConsentText | Snapshot nguyên văn nội dung đồng ý hiển thị lúc đó |
| IpAddress / UserAgent | Ngữ cảnh kỹ thuật |

- Revoke bất kỳ lúc nào từ cả 2 phía. Revoke không xóa bản ghi — set `RevokedAt`.
- Hồ sơ family member: người `ManagedBy` (bố/mẹ) là người bấm đồng ý thay.

### 5.3 Trạng thái PatientDoctorLink

```
Pending ──accept──► Active ──revoke (2 phía)──► Revoked
   │
   └──reject──► Rejected        Expired ◄── (tùy chọn: auto sau 12 tháng không tương tác)
```

---

## 6. MODULE B3 — DOCTOR DASHBOARD

### 6.1 Trang danh sách bệnh nhân (`/doctor/patients`)

| Cột | Nguồn dữ liệu |
|---|---|
| Tên, tuổi, giới tính | HealthProfile / FamilyMember |
| Cảnh báo đang active | HealthAlerts (badge đỏ: số alert chưa acknowledge) |
| Chỉ số mới nhất | HealthMeasurements (BMI, BP gần nhất + thời điểm đo) |
| Lần khám gần nhất | MedicalVisits |
| Liên kết từ | PatientDoctorLink.ConsentAt |

- Sort mặc định: bệnh nhân có alert `critical` lên đầu.
- Filter: có cảnh báo / theo chuyên khoa vấn đề / tìm theo tên.
- Phân trang 20/trang (PagedResult sẵn có).

### 6.2 Trang chi tiết bệnh nhân (`/doctor/patients/:profileId`)

**Tái sử dụng 100% widgets dashboard hiện có** (bmi-chart, bp-chart, glucose-chart,
vaccine-progress, medication-compliance...) ở chế độ read-only:

```
Tabs: Tổng quan │ Chỉ số │ Lịch sử khám │ Thuốc │ Vaccine │ Cảnh báo
```

- Bác sĩ được phép **Acknowledge alert** (đánh dấu "đã xem xét") — ghi rõ
  acknowledged-by-doctor trong AuditLog. Đây là thao tác ghi duy nhất ở MVP.
- Mọi lần mở tab = 1 AuditLog entry (`event_type=data_access`, `TargetUserId`=bệnh nhân).

### 6.3 Điều hướng theo role

- User có role `doctor` → sidebar hiện thêm mục "Bệnh nhân của tôi".
- Route guard: `doctorGuard` (kiểm tra role + DoctorProfile approved từ auth store).

---

## 7. MODULE B4 — THÔNG BÁO CHO BÁC SĨ

### 7.1 Trigger

Mở rộng luồng event hiện có — khi `MeasurementRecordedEvent` / `BloodPressureAlertEvent`
tạo `HealthAlert` cho bệnh nhân:

```
HealthAlert created (severity = warning | critical)
     ▼
Query PatientDoctorLinks WHERE HealthProfileId = X AND Status = Active
     ▼
Với mỗi bác sĩ liên kết:
  - severity == critical → Reminder (Push + Email) NGAY LẬP TỨC, bỏ qua quiet hours
  - severity == warning  → gom vào Daily Digest (1 email/ngày, 7:00 sáng giờ bác sĩ)
```

### 7.2 Daily Digest (Hangfire job mới)

```
DoctorDigestJob — Recurring (0 0 * * * UTC ≈ 7:00 Asia/Ho_Chi_Minh)
  1. Nhóm alerts warning chưa gửi theo DoctorUserId
  2. Render email: "3 bệnh nhân có chỉ số cần chú ý hôm qua: ..."
  3. Ghi trạng thái đã gửi để không lặp
```

### 7.3 Cài đặt phía bác sĩ

Mở rộng `NotificationPreferences` thêm: `DoctorCriticalAlert` (mặc định bật,
khuyến cáo không tắt), `DoctorDailyDigest` (bật/tắt).

---

## 8. THIẾT KẾ CƠ SỞ DỮ LIỆU

> Quy ước giống schema hiện tại: `UNIQUEIDENTIFIER NEWSEQUENTIALID()`, `DATETIME2`,
> `GETUTCDATE()`, soft-delete qua `RevokedAt`/`Status` (không xóa vật lý).

```sql
-- ============================================
-- MODULE A: SHARE GRANTS
-- ============================================

CREATE TABLE ShareGrants (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    HealthProfileId  UNIQUEIDENTIFIER NOT NULL,
    CreatedByUserId  UNIQUEIDENTIFIER NOT NULL,   -- người tạo (chủ hồ sơ hoặc family_admin)
    TokenHash        NVARCHAR(255)    NOT NULL,   -- SHA-256, không lưu token gốc
    Scope            NVARCHAR(MAX)    NOT NULL,   -- JSON: ["measurements","visits",...]
    ExpiresAt        DATETIME2        NOT NULL,
    RevokedAt        DATETIME2        NULL,
    AccessCount      INT              NOT NULL DEFAULT 0,
    LastAccessedAt   DATETIME2        NULL,
    LastAccessedIp   NVARCHAR(45)     NULL,
    CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_ShareGrants_TokenHash UNIQUE (TokenHash),
    CONSTRAINT FK_ShareGrants_HealthProfiles FOREIGN KEY (HealthProfileId)
        REFERENCES HealthProfiles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_ShareGrants_Users FOREIGN KEY (CreatedByUserId)
        REFERENCES Users(Id)
);

CREATE INDEX IX_ShareGrants_Profile_Active
    ON ShareGrants(HealthProfileId, ExpiresAt)
    WHERE RevokedAt IS NULL;

-- ============================================
-- MODULE B1: DOCTOR PROFILES
-- ============================================

CREATE TABLE DoctorProfiles (
    Id                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    UserId            UNIQUEIDENTIFIER NOT NULL,
    LicenseNumber     NVARCHAR(20)     NOT NULL,  -- số chứng chỉ hành nghề
    Specialty         NVARCHAR(100)    NOT NULL,
    Workplace         NVARCHAR(255)    NOT NULL,
    LicenseDocKeys    NVARCHAR(MAX)    NOT NULL,  -- JSON: storage keys (bucket doctor-licenses)
    Status            NVARCHAR(20)     NOT NULL DEFAULT 'pending',
                      -- 'pending' | 'approved' | 'rejected' | 'suspended'
    RejectReason      NVARCHAR(MAX)    NULL,
    VerifiedAt        DATETIME2        NULL,
    VerifiedBy        UNIQUEIDENTIFIER NULL,      -- admin user id
    CreatedAt         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt         DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_DoctorProfiles_UserId UNIQUE (UserId),
    CONSTRAINT UQ_DoctorProfiles_License UNIQUE (LicenseNumber),
    CONSTRAINT FK_DoctorProfiles_Users FOREIGN KEY (UserId)
        REFERENCES Users(Id) ON DELETE CASCADE
);

-- ============================================
-- MODULE B2: PATIENT-DOCTOR LINKS
-- ============================================

CREATE TABLE PatientDoctorLinks (
    Id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    DoctorUserId     UNIQUEIDENTIFIER NOT NULL,
    HealthProfileId  UNIQUEIDENTIFIER NOT NULL,
    InitiatedBy      NVARCHAR(10)     NOT NULL,   -- 'doctor' | 'patient'
    Status           NVARCHAR(20)     NOT NULL DEFAULT 'pending',
                     -- 'pending' | 'active' | 'rejected' | 'revoked' | 'expired'
    -- Consent record (bất biến sau khi accept)
    ConsentAt        DATETIME2        NULL,
    ConsentScope     NVARCHAR(MAX)    NULL,       -- JSON modules được phép
    ConsentText      NVARCHAR(MAX)    NULL,       -- snapshot nguyên văn
    ConsentIp        NVARCHAR(45)     NULL,
    ConsentUserAgent NVARCHAR(MAX)    NULL,
    ConsentByUserId  UNIQUEIDENTIFIER NULL,       -- ai bấm đồng ý (chủ hồ sơ / người quản lý)
    RevokedAt        DATETIME2        NULL,
    RevokedBy        NVARCHAR(10)     NULL,       -- 'doctor' | 'patient' | 'system'
    CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_PatientDoctorLinks UNIQUE (DoctorUserId, HealthProfileId),
    CONSTRAINT FK_PDL_Users FOREIGN KEY (DoctorUserId) REFERENCES Users(Id),
    CONSTRAINT FK_PDL_HealthProfiles FOREIGN KEY (HealthProfileId)
        REFERENCES HealthProfiles(Id) ON DELETE CASCADE
);

CREATE INDEX IX_PDL_Doctor_Active
    ON PatientDoctorLinks(DoctorUserId, Status);
CREATE INDEX IX_PDL_Profile_Active
    ON PatientDoctorLinks(HealthProfileId, Status);

-- ============================================
-- MODULE B4: DIGEST TRACKING
-- ============================================

CREATE TABLE DoctorAlertDeliveries (
    Id            UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
    DoctorUserId  UNIQUEIDENTIFIER NOT NULL,
    HealthAlertId UNIQUEIDENTIFIER NOT NULL,
    Channel       NVARCHAR(20)     NOT NULL,      -- 'push' | 'email' | 'digest'
    SentAt        DATETIME2        NULL,
    Status        NVARCHAR(20)     NOT NULL DEFAULT 'pending',
    CreatedAt     DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT UQ_DoctorAlertDeliveries UNIQUE (DoctorUserId, HealthAlertId, Channel)
);

-- Seed role mới
INSERT INTO Roles (Id, Name, Description)
VALUES (NEWID(), 'doctor', N'Bác sĩ đã xác minh');
```

---

## 9. ĐẶC TẢ API

> Quy ước hiện tại: `api/[controller]`, response `{ success, data, meta }`,
> JWT Bearer trừ khi ghi chú `[public]`.

```
MODULE A — SHARE GRANTS
  POST   /share-grants                       Tạo link chia sẻ {profileId, scope[], ttlHours}
  GET    /share-grants?profileId=            Danh sách link của hồ sơ (kèm AccessCount)
  DELETE /share-grants/{id}                  Revoke

  [public — rate limit 30 req/phút/IP]
  GET    /shared/{token}                     Metadata: tên, scope, hết hạn lúc nào
  GET    /shared/{token}/profile             Hồ sơ tĩnh (theo scope)
  GET    /shared/{token}/measurements        Chỉ số (theo scope)
  GET    /shared/{token}/blood-pressure
  GET    /shared/{token}/medical-visits
  GET    /shared/{token}/medications
  GET    /shared/{token}/vaccines

MODULE B1 — DOCTOR REGISTRATION
  POST   /doctor/register                    Nộp hồ sơ bác sĩ (multipart: form + ảnh CCHN)
  GET    /doctor/me                          Trạng thái hồ sơ của tôi
  PUT    /doctor/me                          Cập nhật + nộp lại (khi Rejected)

  [Authorize(Roles = "admin")]
  GET    /admin/doctor-verifications?status=pending
  GET    /admin/doctor-verifications/{id}    Chi tiết + signed URL ảnh CCHN (5 phút)
  POST   /admin/doctor-verifications/{id}/approve
  POST   /admin/doctor-verifications/{id}/reject     {reason}
  POST   /admin/doctor-verifications/{id}/suspend    {reason}

MODULE B2 — PATIENT-DOCTOR LINKS
  [Doctor]
  POST   /doctor/invitations                 Mời bệnh nhân {patientEmail, message}
  GET    /doctor/invitations?status=         Lời mời đã gửi
  DELETE /doctor/invitations/{id}            Hủy lời mời / revoke liên kết

  [Patient]
  GET    /doctor-links                       Liên kết + lời mời của tôi (mọi hồ sơ tôi quản lý)
  POST   /doctor-links/{id}/accept           {consentScope[]} — chấp nhận
  POST   /doctor-links/{id}/reject
  DELETE /doctor-links/{id}                  Revoke
  GET    /doctors/search?q=                  Danh bạ bác sĩ đã xác minh (tên/CCHN/chuyên khoa)
  POST   /doctor-links                       Bệnh nhân chủ động mời {doctorUserId, profileId}

MODULE B3 — DOCTOR DASHBOARD  [Policy: LinkedDoctor]
  GET    /doctor/patients?filter=&page=      Danh sách bệnh nhân liên kết
  GET    /doctor/patients/{profileId}/summary        Tổng quan + alerts active
  GET    /doctor/patients/{profileId}/measurements   (tái dùng GetMeasurementsQuery)
  GET    /doctor/patients/{profileId}/blood-pressure
  GET    /doctor/patients/{profileId}/medical-visits
  GET    /doctor/patients/{profileId}/medications
  GET    /doctor/patients/{profileId}/vaccines
  POST   /doctor/patients/{profileId}/alerts/{alertId}/acknowledge

MODULE B4 — NOTIFICATIONS
  PUT    /notifications/preferences          (mở rộng DTO: doctorCriticalAlert, doctorDailyDigest)
```

---

## 10. CẤU TRÚC FILE BACKEND

> Theo đúng Clean Architecture + CQRS hiện tại. `(+)` = file mới, `(~)` = sửa file có sẵn.

```
src/HealthCare.Domain/
├── Entities/
│   ├── Sharing/
│   │   └── ShareGrant.cs                        (+) Token hash, Scope, ExpiresAt, Revoke()
│   └── Doctors/
│       ├── DoctorProfile.cs                     (+) Approve()/Reject()/Suspend()
│       ├── PatientDoctorLink.cs                 (+) Accept()/Reject()/Revoke() + consent record
│       └── DoctorAlertDelivery.cs               (+)
├── Enums/
│   ├── DoctorProfileStatus.cs                   (+) Pending/Approved/Rejected/Suspended
│   ├── LinkStatus.cs                            (+) Pending/Active/Rejected/Revoked/Expired
│   └── ShareScope.cs                            (+) Measurements/Visits/Medications/Vaccines
└── Events/
    └── HealthAlertCreatedEvent.cs               (~) thêm publish tới doctors liên kết

src/HealthCare.Application/
├── Sharing/
│   ├── Commands/
│   │   ├── CreateShareGrant/                    (+) Command+Handler+Validator
│   │   └── RevokeShareGrant/                    (+)
│   ├── Queries/
│   │   ├── GetMyShareGrants/                    (+)
│   │   └── GetSharedData/                       (+) validate token → trả data theo scope
│   └── DTOs/ ShareGrantDto, SharedProfileDto    (+)
├── Doctors/
│   ├── Commands/
│   │   ├── RegisterDoctor/                      (+)
│   │   ├── ApproveDoctor/ RejectDoctor/ SuspendDoctor/   (+) [admin]
│   │   ├── InvitePatient/                       (+)
│   │   ├── AcceptDoctorLink/ RejectDoctorLink/ RevokeDoctorLink/  (+)
│   │   └── AcknowledgeAlertAsDoctor/            (+)
│   ├── Queries/
│   │   ├── GetDoctorProfile/ SearchDoctors/     (+)
│   │   ├── GetPendingVerifications/             (+) [admin]
│   │   ├── GetMyPatients/                       (+) join links + latest measurements + alerts
│   │   ├── GetPatientSummaryForDoctor/          (+)
│   │   └── GetMyDoctorLinks/                    (+) [patient]
│   └── DTOs/                                    (+)
└── Common/
    ├── Interfaces/IDoctorAccessGuard.cs          (+) EnsureLinkedAsync(doctorId, profileId)
    └── Behaviours/AuditBehaviour.cs              (~) map các command mới

src/HealthCare.Infrastructure/
├── Persistence/
│   ├── Configurations/
│   │   ├── ShareGrantConfiguration.cs           (+)
│   │   ├── DoctorProfileConfiguration.cs        (+)
│   │   ├── PatientDoctorLinkConfiguration.cs    (+)
│   │   └── DoctorAlertDeliveryConfiguration.cs  (+)
│   ├── Migrations/  → dotnet ef migrations add DoctorPortal
│   └── Seeds/RoleSeeder.cs                      (~) thêm role 'doctor'
├── Services/
│   ├── Doctors/DoctorAccessGuard.cs             (+) implement policy mục 2.3
│   └── Email/Templates/
│       ├── DoctorApprovedEmail.html             (+)
│       ├── DoctorRejectedEmail.html             (+)
│       ├── PatientInviteEmail.html              (+)
│       └── DoctorDigestEmail.html               (+)
└── BackgroundJobs/
    ├── DoctorDigestJob.cs                       (+) daily 0:00 UTC
    └── ShareGrantCleanupJob.cs                  (+) daily: xóa grants hết hạn > 30 ngày

src/HealthCare.API/
├── Controllers/
│   ├── ShareGrantsController.cs                 (+)
│   ├── SharedController.cs                      (+) [AllowAnonymous + rate limit]
│   ├── DoctorController.cs                      (+)
│   ├── DoctorLinksController.cs                 (+)
│   └── AdminController.cs                       (~) thêm endpoints duyệt bác sĩ
└── Extensions/ServiceCollectionExtensions.cs    (~) đăng ký policy "LinkedDoctor"
```

---

## 11. CẤU TRÚC FILE FRONTEND

```
src/app/
├── core/auth/
│   ├── auth.store.ts                            (~) thêm isDoctor computed
│   └── doctor.guard.ts                          (+) role doctor + profile approved
├── features/
│   ├── sharing/                                 (+) MODULE A
│   │   ├── sharing.routes.ts                        /sharing (quản lý) + /shared/:token (public)
│   │   ├── share-manager/
│   │   │   ├── share-manager.component.ts           danh sách link + tạo mới + revoke
│   │   │   └── create-share-dialog.component.ts     chọn scope + thời hạn → hiện QR
│   │   └── shared-viewer/
│   │       └── shared-viewer.component.ts           trang public read-only (không cần login)
│   ├── doctor/                                  (+) MODULE B — lazy, doctorGuard
│   │   ├── doctor.routes.ts                         /doctor/...
│   │   ├── doctor.store.ts                          SignalStore: patients, alerts
│   │   ├── registration/
│   │   │   └── doctor-registration.component.ts     form + upload CCHN + trạng thái duyệt
│   │   ├── patient-list/
│   │   │   └── patient-list.component.ts            bảng bệnh nhân + badge cảnh báo
│   │   ├── patient-detail/
│   │   │   └── patient-detail.component.ts          tabs, tái dùng widgets dashboard
│   │   └── invitations/
│   │       └── invite-patient.component.ts
│   ├── settings/
│   │   └── doctor-links/                        (+) [phía bệnh nhân]
│   │       └── doctor-links.component.ts            lời mời đến + liên kết active + revoke
│   └── admin/
│       └── doctor-verifications/                (+)
│           ├── verification-list.component.ts
│           └── verification-detail.component.ts     xem ảnh CCHN + approve/reject
└── layouts/main-layout/sidebar/                 (~) mục "Bệnh nhân của tôi" (chỉ doctor)
                                                     mục "Chia sẻ hồ sơ" (mọi user)
```

---

## 12. BẢO MẬT & TUÂN THỦ

| Hạng mục | Biện pháp |
|---|---|
| Token chia sẻ | 64-byte random, lưu SHA-256 hash; 404 đồng nhất cho mọi lỗi; rate limit IP |
| Ảnh CCHN | Bucket MinIO riêng `doctor-licenses`, signed URL 5 phút, chỉ admin |
| Consent | Bản ghi bất biến (ConsentText snapshot) — chứng cứ tuân thủ NĐ 13/2023 |
| Audit | Mọi truy cập của bác sĩ/link chia sẻ → AuditLog với `TargetUserId`; bệnh nhân xem được lịch sử "ai đã xem hồ sơ tôi" |
| Revoke | Kiểm tra Status ở mọi request (không cache quyền quá 5 phút) |
| Số CCHN | Unique — một chứng chỉ không thể đăng ký 2 tài khoản |
| Suspended | Mất quyền ngay lập tức, thông báo cho toàn bộ bệnh nhân liên kết |
| Least privilege | Doctor KHÔNG có API ghi nào ngoài acknowledge alert |

---

## 13. KẾ HOẠCH SPRINT

| Sprint | Thời gian | Nội dung | Deliverables |
|---|---|---|---|
| A | 1 tuần | ShareGrant end-to-end | Entity + migration + API + trang share/QR + public viewer + tests |
| B1 | 1 tuần | Đăng ký & xác minh bác sĩ | DoctorProfile + upload CCHN + admin duyệt + gán role + emails |
| B2 | 1 tuần | Liên kết & consent | PatientDoctorLink + luồng mời 2 chiều + consent record + revoke |
| B3 | 1-1.5 tuần | Doctor dashboard | Patient list + patient detail (tái dùng widgets) + DoctorAccessGuard + audit |
| B4 | 0.5 tuần | Thông báo | Critical alert realtime + DoctorDigestJob + preferences |
| — | 0.5 tuần | Hardening | Integration tests luồng revoke/suspend, pentest thủ công token, rà soát audit |

**Tổng: ~5-6 tuần.** Điều kiện tiên quyết trước khi mời bác sĩ thật: hệ thống đã deploy
HTTPS + backup tự động (xem SRS §13).

### Thứ tự test bắt buộc (Definition of Done từng sprint)

```
Sprint A:  tạo link → xem bằng trình duyệt ẩn danh → revoke → 404 ngay
Sprint B1: đăng ký → admin reject → nộp lại → approve → JWT có role doctor
Sprint B2: mời → từ chối/chấp nhận → revoke từ phía bệnh nhân → bác sĩ 403 ngay
Sprint B3: bác sĩ xem đúng bệnh nhân liên kết, 403 với hồ sơ khác, audit đầy đủ
Sprint B4: đo huyết áp 190/120 → bác sĩ nhận push < 15 phút
```

---

## 14. NGOÀI PHẠM VI (GIAI ĐOẠN C)

Các hạng mục sau **không** thuộc MVP này, sẽ đặc tả riêng khi Giai đoạn B ổn định:

- Ghi chú / lời dặn của bác sĩ trên hồ sơ bệnh nhân (doctor notes)
- Bác sĩ đề xuất lịch tái khám → tự tạo Reminder cho bệnh nhân
- Chat bác sĩ ↔ bệnh nhân
- Telemedicine / video call (cần giấy phép khám chữa bệnh từ xa)
- Kê đơn điện tử có chữ ký số
- Tổ chức phòng khám (nhiều bác sĩ chung 1 clinic, phân quyền nội bộ)
- Xác minh CCHN tự động qua API Bộ Y tế (hiện chưa có API công khai)

---

*Tài liệu này là phần mở rộng của SRS_HealthPlus.md v1.1.0 — các quy ước kỹ thuật,*
*bảo mật và kiến trúc không nêu lại ở đây đều tuân theo tài liệu gốc.*
*Health+ Project | 2026-07-09*
