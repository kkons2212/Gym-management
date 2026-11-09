# SỔ ÁNH XẠ KỸ THUẬT & TRUY VẾT TRIỂN KHAI (TECHNICAL IMPLEMENTATION & MAPPING LEDGER)
# HỆ THỐNG QUẢN LÝ PHÒNG TẬP THỂ THAO (GYM MANAGEMENT SYSTEM)
**Tài liệu tham chiếu:** [`PRD.md`](PRD.md) | [`predevrule.md`](predevrule.md) | [`task.md`](task.md) | [`dev_rule.md`](dev_rule.md)

---

## 1. BẢNG TIẾN ĐỘ TỔNG THỂ (IMPLEMENTATION DASHBOARD)

* **Tổng số Nhiệm vụ trong Roadmap:** 33 Tasks
* **Tiến độ Hoàn thành:** **25 / 33 Tasks (75.8%)**
* **Trạng thái Triển khai theo 3 Khối Thành phần Kiến trúc:**
  * 🟢 **Khối Hạ Tầng & Tiêu Chuẩn (Infrastructure & Standards):** Đã hoàn tất 100% (Task 0.0, Task 0.0b, Task 5.0, Task 0.1)
  * 🟢 **Khối Khởi tạo Nền tảng (Phase 0 Scaffolding):** Đã hoàn tất 100% (Task 0.1, 0.2, 0.3, 0.4)
  * 🟡 **Khối Máy Chủ (Server Component):** Đang triển khai Giai đoạn 1, 2 & 3 (Đã hoàn tất Task 1.1.1 – 1.1.5, Task 1.2.1, 1.2.2, Task 1.3.1, 1.3.2, 1.3.3, Task 2.1, Task 2.3, Task 2.5, Task 2.6, Task 2.7, Task 3.1, Task 3.2)
  * 🟢 **Khối Giao Diện Admin Desktop (Task 4.1):** Đã hoàn tất 100% (Task 4.1.1, 4.1.2, 4.1.3 - WinUI 3 MVVM)
  * ⚪ **Khối Giao Diện Web Portal (Task 4.2):** Sẵn sàng triển khai Blazor Server Portal (Task 4.2.1 – 4.2.3)

---

## 2. MA TRẬN TRUY VẾT TỔNG HỢP (TRACEABILITY MATRIX TABLE)

Tra cứu nhanh toàn bộ task đã hoàn thành theo hàng ngang:

| Task ID | Tên Nhiệm Vụ | Phân hệ PRD (Use Case) | Thành phần & Tầng | File Mã Nguồn Đã Tạo / Sửa | File Test Tương Ứng | Bảng CSDL | Commit Hash |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Task 0.0** | Pipeline CI GitHub Actions | Toàn bộ hệ thống | `.github/workflows` | `.github/workflows/ci.yml` | GitHub Runner Verification | N/A | `2131cff` |
| **Task 0.0b**| Ban hành Dev Rules & Dev Mapping | Toàn bộ 10 Epics | `Document/` | `Document/dev_rule.md`, `dev_mapping.md` | Verification via CI & DoD Checklist | 9 bảng DDL | `9ad7903` |
| **Task 0.1** | Khởi tạo Solution Root | Hạ tầng toàn Solution | `GymManagement.sln` | `GymManagement.sln` | CI Solution Build Verification | N/A | `f92e2c7` |
| **Task 0.2** | Scaffolding 3 Server Projects | Hạ tầng Server | `src/Server/` | `Server.Core`, `Server.Data`, `Server.Api` | `tests/GymManagement.Server.UnitTests` | 9 bảng DDL | `f92e2c7` |
| **Task 0.3** | Scaffolding 2 Client Projects | Hạ tầng Client | `src/Client/` | `GymManagement.Desktop`, `GymManagement.Web` | Desktop & Web Build Verification | N/A | `f92e2c7` |
| **Task 0.4** | Scaffolding 3 Test Projects | Hạ tầng Testing | `tests/` | `Server.UnitTests`, `Server.DataTests`, `Server.ApiTests` | 100% Pass (Sanity Tests) | In-Memory | `f92e2c7` |
| **Task 5.0** | Kế hoạch CI toàn diện Server & Client | Toàn bộ 10 Epics | `.github/workflows` | `.github/workflows/ci.yml` | Gitleaks, DDL check, Quality Gate | 9 bảng DDL | `2131cff` |
| **Task 1.1.1 - 1.1.5** | Thực thể CSDL & Cấu hình 9 bảng (SQL Server Database) | Toàn bộ 10 Epics (9 bảng vật lý chuẩn DDL) | `Server.Core` (Entities) & `Server.Data` (Configurations & DbContext) | `src/Server/GymManagement.Server.Core/Entities/*` (9 entities), `src/Server/GymManagement.Server.Data/Configurations/*` (9 configs), `GymDbContext.cs` | `tests/GymManagement.Server.DataTests/Persistence/GymDbContextAllEntitiesModelTests.cs` (5 tests) | Toàn bộ 9 bảng (`Role`, `[User]`, `PackagePlan`, `PTAvailability`, `PTTrainingSession`, `MemberSubscription`, `CheckInLog`, `Invoice`, `MemberBodyMetric`) | `afb9ef9` |
| **Task 1.2.1** | Interface Database API: GenericRepository | Toàn bộ 10 Epics (CRUD an toàn) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Repositories/GenericRepository.cs`, `Contracts/Data/IGenericRepository.cs` | `tests/GymManagement.Server.DataTests/Repositories/GenericRepositoryTests.cs` (13 tests) | 9 bảng CSDL | `04a79c7` |
| **Task 1.2.2** | Interface Database API: UnitOfWork (ACID) | Toàn bộ 10 Epics (Transaction ACID) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Repositories/UnitOfWork.cs`, `Contracts/Data/IUnitOfWork.cs`, `Persistence/GymDbContext.cs` | `tests/GymManagement.Server.DataTests/Repositories/UnitOfWorkTests.cs` (9 tests) | 9 bảng CSDL | `04a79c7` |
| **Task 1.3.1** | Cổng Payment API (VietQR Adapter) | UC05 (Thanh toán POS, VietQR động) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Adapters/VietQrAdapter.cs`, `Contracts/Adapters/IVietQrAdapter.cs`, `DTOs/Adapters/VietQrDtos.cs` | `tests/GymManagement.Server.DataTests/Adapters/VietQrAdapterTests.cs` (4 tests) | `Invoice` | `b2f0cab` |
| **Task 1.3.2** | Cổng SMS Gateway (Notification Adapter) | UC02, UC04, UC05 (Hóa đơn Email, Nhắc lịch PT SMS) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Adapters/NotificationAdapter.cs`, `Contracts/Adapters/INotificationAdapter.cs`, `DTOs/Adapters/NotificationDtos.cs` | `tests/GymManagement.Server.DataTests/Adapters/NotificationAdapterTests.cs` (4 tests) | `[User]`, `Invoice` | `b2f0cab` |
| **Task 1.3.3** | Cổng Camera Scanner (QR Decoder & Token Validator) | UC03 (Check-in QR quay vòng 60s) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Adapters/CameraScannerAdapter.cs`, `Contracts/Adapters/ICameraScannerAdapter.cs`, `DTOs/Adapters/QrScanDtos.cs` | `tests/GymManagement.Server.DataTests/Adapters/CameraScannerAdapterTests.cs` (5 tests) | `CheckInLog`, `[User]` | `b2f0cab` |
| **Task 2.3** | MemberService Component (UC02, UC09) | UC02, UC09 (Đăng ký, gia hạn, bảo lưu, hủy gói tập, lịch sử tập luyện) | `Server.Core` | `Enums/SubscriptionStatus.cs`, `DTOs/Members/MemberDtos.cs`, `Contracts/IMemberService.cs`, `Services/MemberService.cs` | `tests/.../Services/MemberServiceTests.cs` (13 tests) | `[User]`, `MemberSubscription`, `PackagePlan`, `CheckInLog` | *(pending)* |
| **Task 2.7** | ReportService Component (UC06) | UC06 (US 06.1, US 06.2) — Xem & Xuất báo cáo thống kê BI đa chiều | `Server.Core` & `Server.Api` | `Contracts/IReportService.cs`, `DTOs/Reports/ReportDtos.cs`, `Services/Reporting/ReportExportHelper.cs`, `Services/ReportService.cs`, `Server.Api/Program.cs` | `tests/.../Services/ReportServiceTests.cs` (16 tests) | `Invoice`, `MemberSubscription`, `CheckInLog`, `PTTrainingSession`, `[User]`, `PackagePlan` | `ce9e620` |

---

## 3. NHẬT KÝ ÁNH XẠ KỸ THUẬT CHI TIẾT THEO TỪNG TASK (TASK DETAILS LOG)

### 📌 [Task 0.0b] Ban Hành Bộ Quy Chuẩn Lập Trình (`dev_rule.md`) & Sổ Ánh Xạ Kỹ Thuật (`dev_mapping.md`)
* **Ngày hoàn tất:** 08/11/2026 | **Commit:** `9ad7903` (nhánh `feature/dev-rules`)
* **Phân hệ nghiệp vụ liên quan:** Chuẩn mực kỹ thuật & Truy vết cho toàn bộ 10 Epics (UC01 – UC10)
* **Ranh giới thành phần:** `Document/` (Quy định cho toàn bộ `Server`, `Client`, và `Testing`)
* **Mã nguồn tạo / sửa:**
  - [`Document/dev_rule.md`](dev_rule.md) (7 chương: Task Lifecycle, KISS/YAGNI/DRY/SOLID, quy tắc đặt tên, biến toàn cục ở đầu file, DoD 10 tiêu chí)
  - [`Document/dev_mapping.md`](dev_mapping.md) (Dashboard, Ma trận truy vết, Nhật ký chi tiết)
  - [`task.md`](../task.md)
  - [`Document/task.md`](task.md)
* **Kiểm thử & Ràng buộc xác thực:**
  - Đối soát 100% với [`PRD.md`](PRD.md), [`predevrule.md`](predevrule.md) và DDL 9 bảng CSDL.
  - Tích hợp 10 tiêu chí nghiệm thu (DoD) làm điều kiện bắt buộc trước khi đóng bất kỳ task nào.
* **Ghi chú kiến trúc:** Mọi task sau khi hoàn thành bắt buộc phải cập nhật bản ghi vào file này trước khi commit.

---

### 📌 [Task 0.0 & Task 5.0] Thiết lập Pipeline CI Kiểm thử Tự động Toàn diện
* **Ngày hoàn tất:** 08/11/2026 | **Commit:** `2131cff` (nhánh `feature/ci-pipeline`)
* **Phân hệ nghiệp vụ liên quan:** Hạ tầng DevOps & Kiểm chuẩn chất lượng toàn bộ 10 Epics (UC01 – UC10)
* **Ranh giới thành phần:** `.github/workflows/` (Điều phối `Server` và `Client`)
* **Mã nguồn tạo / sửa:**
  - [`.github/workflows/ci.yml`](../.github/workflows/ci.yml)
  - [`task.md`](../task.md)
  - [`Document/task.md`](task.md)
* **Kiểm thử & Ràng buộc xác thực:**
  - `dorny/paths-filter@v3`: Phát hiện thay đổi theo subsystem (`src/Server/**` vs `src/Client/**`)
  - `gitleaks/gitleaks-action@v2`: Quét rò rỉ secret/token/password trong git history
  - Script python: Kiểm tra cú pháp đầy đủ 9 bảng CSDL trong [`Document/GymManagement_Physical_Model.sql`](GymManagement_Physical_Model.sql)
  - Kiểm tra ranh giới 3 lớp (.csproj references)
  - Cổng chất lượng tổng hợp: `ci-gate` (Quality Gate bắt buộc để Merge PR)
* **Ghi chú kiến trúc:** Runner Windows (`windows-latest`) cho Desktop WinUI 3; Runner Linux (`ubuntu-latest`) cho Server Core & Database verification.

---

### 📌 [Task 0.1, 0.2, 0.3, 0.4] Khởi tạo Cấu trúc Solution, Server, Client và Test Projects (Giai đoạn 0)
* **Ngày hoàn tất:** 08/11/2026 | **Commit:** `f92e2c7` (nhánh `feat/phase-0-scaffolding`)
* **Phân hệ nghiệp vụ liên quan:** Hạ tầng mã nguồn toàn hệ thống (Toàn bộ 10 Epics UC01 – UC10)
* **Ranh giới thành phần:** `Solution Root`, `Server.Core`, `Server.Data`, `Server.Api`, `Client.Desktop`, `Client.Web`, `Server.UnitTests`, `Server.DataTests`, `Server.ApiTests`
* **Mã nguồn tạo / sửa:**
  - `GymManagement.sln`: File Solution chuẩn chứa toàn bộ 8 projects
  - `GymManagement.Server.slnf`: Solution Filter phân vùng riêng Server & Web cho Linux runner trong CI (tránh NETSDK1100 do Desktop WinUI 3)
  - `src/Server/GymManagement.Server.Core`: Class Library .NET 8 chứa 7 interface contracts (`IAuthService`, `IMemberService`, `ICheckInService`, `IPtService`, `IBillingService`, `IReportService`, `ISystemSettingService`), ApiResponse DTO, và Business Exceptions
  - `src/Server/GymManagement.Server.Data`: Class Library .NET 8 (tham chiếu `Server.Core`), cấu hình EF Core SqlServer 8.0.11 & Design
  - `src/Server/GymManagement.Server.Api`: ASP.NET Core Web API .NET 8 (tham chiếu `Server.Core` & `Server.Data`), JWT Bearer, Swagger, Controllers, `/health` endpoint
  - `src/Client/GymManagement.Desktop`: WinUI 3 MVVM .NET 8 (tham chiếu `Server.Core`), CommunityToolkit.Mvvm 8.2.2, WindowsAppSDK 1.7
  - `src/Client/GymManagement.Web`: Blazor Server .NET 8 (tham chiếu `Server.Core`)
  - `tests/GymManagement.Server.UnitTests`: xUnit .NET 8, Moq 4.20.70, FluentAssertions 6.12.0
  - `tests/GymManagement.Server.DataTests`: xUnit .NET 8, EF Core InMemory 8.0.11, FluentAssertions
  - `tests/GymManagement.Server.ApiTests`: xUnit .NET 8, WebApplicationFactory 8.0.11, FluentAssertions
* **Kiểm thử tương ứng (Verification):**
  - Architecture Boundary Guard: Đã xác thực `Core` và `Client` KHÔNG tham chiếu `Server.Data`
  - Build Solution: 100% build thành công 0 errors
  - Test Execution: 4/4 test cases passed (100% pass)
* **Ghi chú kiến trúc:** Tuân thủ Pre-dev Rule 1 (tách biệt ranh giới 3 lớp), không cho phép Client gọi trực tiếp CSDL.

---

### 📌 [Task 1.3.1, 1.3.2, 1.3.3] Cổng Giao Tiếp Dịch Vụ Bên Ngoài (External Adapters: VietQR, Notification, CameraScanner)
* **Ngày hoàn tất:** 08/11/2026 | **Commit:** `b2f0cab` | **Nhánh:** `feat/task-1.3-external-adapters`
* **Phân hệ nghiệp vụ liên quan:**
  - UC05 (Thanh toán POS, VietQR động theo chuẩn NAPAS 247 & Giả lập Sandbox)
  - UC02, UC04, UC05 (Gửi Email/SMS hóa đơn tức thì & Nhắc lịch tập PT trước 2 giờ)
  - UC03 (Giải mã QR điểm danh <100ms, xác thực token động xoay vòng 60 giây chống gian lận)
* **Ranh giới thành phần:** `Server.Core` (Contracts & DTOs) và `Server.Data` (Adapters & DI Extension)
* **Mã nguồn tạo / sửa:**
  - `src/Server/GymManagement.Server.Core/DTOs/Adapters/VietQrDtos.cs`
  - `src/Server/GymManagement.Server.Core/DTOs/Adapters/NotificationDtos.cs`
  - `src/Server/GymManagement.Server.Core/DTOs/Adapters/QrScanDtos.cs`
  - `src/Server/GymManagement.Server.Core/Contracts/Adapters/IVietQrAdapter.cs`
  - `src/Server/GymManagement.Server.Core/Contracts/Adapters/INotificationAdapter.cs`
  - `src/Server/GymManagement.Server.Core/Contracts/Adapters/ICameraScannerAdapter.cs`
  - `src/Server/GymManagement.Server.Data/Adapters/VietQrAdapter.cs`
  - `src/Server/GymManagement.Server.Data/Adapters/NotificationAdapter.cs`
  - `src/Server/GymManagement.Server.Data/Adapters/CameraScannerAdapter.cs`
  - `src/Server/GymManagement.Server.Data/DependencyInjection.cs`
  - `src/Server/GymManagement.Server.Api/Program.cs`
* **Kiểm thử tương ứng (Verification):**
  - `tests/GymManagement.Server.DataTests/Adapters/VietQrAdapterTests.cs` (4 unit tests - QuickLink NAPAS, validate amount > 0, Sandbox simulation, pending status)
  - `tests/GymManagement.Server.DataTests/Adapters/NotificationAdapterTests.cs` (4 unit tests - Sandbox logging, SMS dispatch, HTML invoice template, PT reminder)
  - `tests/GymManagement.Server.DataTests/Adapters/CameraScannerAdapterTests.cs` (5 unit tests - ZXing empty bytes, plain text fallback, 60s dynamic token valid, >60s token expired, invalid prefix)
  - **Kết quả:** 13/13 unit tests pass 100% (Tổng DataTests: 14/14, Solution Tests: 17/17 pass)
* **Bảng CSDL liên quan:** `Invoice`, `[User]`, `CheckInLog`
* **Ràng buộc nghiệp vụ & Kiến trúc:**
  - **Sandbox Simulation:** Khi `UseSandbox = true`, VietQR và Notification Adapter ghi log và mô phỏng giao dịch thành công mà không gọi mạng thực tế, đảm bảo an toàn môi trường phát triển và kiểm thử tự động.
  - **QR Token Security:** Token điểm danh có định dạng `GYM-{UserId}-{TimestampUtcTicks}[-{Signature}]`, tự động hết hạn sau 60 giây, ngăn chặn triệt để chụp ảnh màn hình chuyển tiếp.
  - **ZXing Decoding Performance:** Tối ưu hóa đọc mảng byte hình ảnh qua LuminanceSource và giải mã chuỗi dự phòng cho thiết bị quét quang học USB/HID với thời gian xử lý <100ms.

---

### 📌 [Task 1.2.1, 1.2.2] Hiện thực Interface "Database API" (Unit of Work & Generic Repository)
* **Ngày hoàn tất:** 08/11/2026 | **Commit:** `04a79c7` | **Nhánh:** `feat/task-1.2-unit-of-work-and-repositories`
* **Phân hệ nghiệp vụ liên quan:** Nền tảng truy xuất dữ liệu & Quản lý giao dịch ACID cho toàn bộ 10 Epics (UC01 – UC10)
* **Ranh giới thành phần:**
  - `Server.Core` (Contracts): `IGenericRepository<T>`, `IUnitOfWork` (Độc lập 100% với EF Core - DIP)
  - `Server.Data` (Persistence & Repositories): `GymDbContext`, `GenericRepository<T>`, `UnitOfWork`
* **Mã nguồn tạo / sửa:**
  - `src/Server/GymManagement.Server.Core/Contracts/Data/IGenericRepository.cs`
  - `src/Server/GymManagement.Server.Core/Contracts/Data/IUnitOfWork.cs`
  - `src/Server/GymManagement.Server.Data/Persistence/GymDbContext.cs`
  - `src/Server/GymManagement.Server.Data/Repositories/GenericRepository.cs`
  - `src/Server/GymManagement.Server.Data/Repositories/UnitOfWork.cs`
  - `src/Server/GymManagement.Server.Data/DependencyInjection.cs`
  - `src/Server/GymManagement.Server.Api/Program.cs`
* **Kiểm thử tương ứng (Verification):**
  - `tests/GymManagement.Server.DataTests/Repositories/GenericRepositoryTests.cs` (13 tests - GetById, GetAll, Find, FindSingleOrDefault, GetPaged, Exists, Count, Add, AddRange, Update, Remove, RemoveRange)
  - `tests/GymManagement.Server.DataTests/Repositories/UnitOfWorkTests.cs` (9 tests - Cache repository instance, SaveChanges, ExecuteTransaction success/rollback, BeginTransaction, Commit, Dispose/DisposeAsync)
  - **Kết quả:** 22/22 unit tests mới pass 100% (Tổng DataTests: 36/36, Solution Tests: 39/39 pass)
* **Bảng CSDL liên quan:** Toàn bộ 9 bảng CSDL (`Role`, `[User]`, `PackagePlan`, `PTAvailability`, `PTTrainingSession`, `MemberSubscription`, `CheckInLog`, `Invoice`, `MemberBodyMetric`)
* **Ràng buộc nghiệp vụ & Kiến trúc:**
  - **DIP (Dependency Inversion):** `IGenericRepository<T>` và `IUnitOfWork` nằm hoàn toàn trong `Server.Core`, không phụ thuộc EF Core hay SQL Server.
  - **ACID Transaction Guarantee:** `UnitOfWork.ExecuteTransactionAsync` tự động mở transaction, ghi log chi tiết khi phát sinh exception, tự động gọi `RollbackAsync` và làm sạch bộ theo dõi trạng thái (`ChangeTracker.Clear()`) để đảm bảo tính cô lập và nhất quán tuyệt đối.
  - **Hiệu năng & An toàn:** Dùng `AsNoTracking()` cho tất cả các truy vấn đọc, và quản lý repository instances qua `ConcurrentDictionary` để tránh khởi tạo dư thừa đối tượng.

---

### 📌 [Task 2.3] Hiện thực MemberService Component (UC02, UC09)
* **Ngày hoàn tất:** 08/11/2026 | **Commit:** *(pending)* | **Nhánh:** `feat/task-2.3-member-service`
* **Phân hệ nghiệp vụ liên quan:** UC02.1–UC02.5 (Đăng ký, cập nhật, gia hạn, bảo lưu, hủy gói tập), UC09.1–UC09.2 (Hội viên xem profile & lịch sử tập)
* **Ranh giới thành phần:** `Server.Core` (Contracts, DTOs, Enums, Services)
* **Mã nguồn tạo / sửa:**
  - `src/Server/GymManagement.Server.Core/Enums/SubscriptionStatus.cs`
  - `src/Server/GymManagement.Server.Core/DTOs/Members/MemberDtos.cs` (12 record types)
  - `src/Server/GymManagement.Server.Core/Contracts/IMemberService.cs` (9 methods)
  - `src/Server/GymManagement.Server.Core/Services/MemberService.cs`
  - `src/Server/GymManagement.Server.Core/GymManagement.Server.Core.csproj`
  - `tests/GymManagement.Server.UnitTests/GymManagement.Server.UnitTests.csproj`
* **Kiểm thử tương ứng (Verification):**
  - `tests/GymManagement.Server.UnitTests/Services/MemberServiceTests.cs` (13 tests — 13/13 pass)
* **Bảng CSDL liên quan:** `[User]`, `MemberSubscription`, `PackagePlan`, `CheckInLog`
* **Ràng buộc nghiệp vụ & Giao dịch:**
  - RegisterMember: tạo `[User]` + `MemberSubscription` trong 1 Transaction, sinh QrCode unique, `EndDate = StartDate + DurationDays`
  - FreezeSubscription: `EndDate += (FreezeEndDate - FreezeStartDate)`, Status → FROZEN
  - CancelSubscription: lý do hủy bắt buộc, Status → CANCELLED, giữ lịch sử
  - Welcome Email: fire-and-forget, lỗi chỉ log Warning

---

### 📌 [Task 1.1.1 – 1.1.5] Thực Thể CSDL & Cấu Hình 9 Bảng (SQL Server Database)
* **Ngày hoàn tất:** 08/11/2026 | **Commit:** `afb9ef9` | **Nhánh:** `feat/task-1.1-sql-entities-configuration`
* **Phân hệ nghiệp vụ liên quan:** Nền tảng dữ liệu cho toàn bộ 10 Epics (UC01 – UC10)
* **Ranh giới thành phần:** `Server.Core` (Entities) và `Server.Data` (Configurations & DbContext)
* **Mã nguồn tạo / sửa:**
  - `src/Server/GymManagement.Server.Core/Entities/Role.cs` (Task 1.1.1)
  - `src/Server/GymManagement.Server.Core/Entities/User.cs` (Task 1.1.1)
  - `src/Server/GymManagement.Server.Core/Entities/PackagePlan.cs` (Task 1.1.2)
  - `src/Server/GymManagement.Server.Core/Entities/MemberSubscription.cs` (Task 1.1.2)
  - `src/Server/GymManagement.Server.Core/Entities/PTAvailability.cs` (Task 1.1.3)
  - `src/Server/GymManagement.Server.Core/Entities/PTTrainingSession.cs` (Task 1.1.3)
  - `src/Server/GymManagement.Server.Core/Entities/CheckInLog.cs` (Task 1.1.4)
  - `src/Server/GymManagement.Server.Core/Entities/Invoice.cs` (Task 1.1.4)
  - `src/Server/GymManagement.Server.Core/Entities/MemberBodyMetric.cs` (Task 1.1.4)
  - `src/Server/GymManagement.Server.Data/Configurations/RoleConfiguration.cs` (Task 1.1.5)
  - `src/Server/GymManagement.Server.Data/Configurations/UserConfiguration.cs` (Task 1.1.5)
  - `src/Server/GymManagement.Server.Data/Configurations/PackagePlanConfiguration.cs` (Task 1.1.5)
  - `src/Server/GymManagement.Server.Data/Configurations/MemberSubscriptionConfiguration.cs` (Task 1.1.5)
  - `src/Server/GymManagement.Server.Data/Configurations/PTAvailabilityConfiguration.cs` (Task 1.1.5)
  - `src/Server/GymManagement.Server.Data/Configurations/PTTrainingSessionConfiguration.cs` (Task 1.1.5)
  - `src/Server/GymManagement.Server.Data/Configurations/CheckInLogConfiguration.cs` (Task 1.1.5)
  - `src/Server/GymManagement.Server.Data/Configurations/InvoiceConfiguration.cs` (Task 1.1.5)
  - `src/Server/GymManagement.Server.Data/Configurations/MemberBodyMetricConfiguration.cs` (Task 1.1.5)
  - `src/Server/GymManagement.Server.Data/Persistence/GymDbContext.cs` (Khai báo 9 `DbSet<T>`)
  - `src/Server/GymManagement.Server.Core/Services/PtService.cs` (Xóa entity stubs tạm thời, dùng chung Core Entities)
  - `src/Server/GymManagement.Server.Core/Services/SystemSettingService.cs` (Xóa entity stubs tạm thời, dùng chung Core Entities)
  - `src/Server/GymManagement.Server.Core/Services/AuthService.cs` (Import Core Entities)
* **Kiểm thử tương ứng (Verification):**
  - `tests/GymManagement.Server.DataTests/Persistence/GymDbContextAllEntitiesModelTests.cs` (5 tests mới: table names, primary keys, physical columns/types, unique constraints, foreign keys).
  - Kết quả kiểm thử toàn solution:
    - `DataTests`: 41/41 test cases pass 100%.
    - `UnitTests`: 56/56 test cases pass 100%.
    - `ApiTests`: 12/12 test cases pass 100%.
    - **Tổng cộng: 109/109 test cases pass 100%.**
* **Bảng CSDL liên quan:** Toàn bộ 9 bảng CSDL (`Role`, `[User]`, `PackagePlan`, `PTAvailability`, `PTTrainingSession`, `MemberSubscription`, `CheckInLog`, `Invoice`, `MemberBodyMetric`).
* **Ràng buộc nghiệp vụ & Kiến trúc:**
  - **Khớp 100% DDL:** Định kiểu chính xác `varchar(10)`, `decimal(19, 0)`, `date`, `time(7)`, `tinyint`.
  - **Ràng buộc duy nhất (Unique Constraints):** `UQ_MemberSubscription_QrCode` trên cột `QrCode`, `UQ_Invoice_InvoiceCode` trên cột `InvoiceCode`.
  - **Khóa ngoại & Toàn vẹn tham chiếu:** Toàn bộ khóa ngoại cấu hình `DeleteBehavior.Restrict` chống xóa tầng làm mất dữ liệu lịch sử hoặc xung đột DDL.
  - **Xóa bỏ triệt để Entity Stubs:** Loại bỏ hoàn toàn các class placeholder rải rác trong `PtService` và `SystemSettingService`, chuẩn hóa dùng một nguồn sự thật duy nhất (Single Source of Truth) trong `GymManagement.Server.Core.Entities`.
  - **DoD 6 & Cú pháp:** Tuân thủ 100% không toán tử 3 ngôi (`? :`), đặt tên chuẩn, properties hỗ trợ cả PascalCase lẫn alias tương thích ngược.

### 📌 [Task 2.7] Hiện thực ReportService Component (UC06)
* **Ngày hoàn tất:** 09/11/2026 | **Commit:** `ce9e620` (nhánh `feat/task-2.7-report-service`)
* **Phân hệ nghiệp vụ liên quan:** Epic 06 / UC06 (US 06.1 — Báo cáo thống kê thời gian thực; US 06.2 — Xuất dữ liệu báo cáo sang Excel / PDF).
* **Ranh giới thành phần:** `Server.Core` (Contracts, DTOs, Services, Helpers) & `Server.Api` (DI Registration).
* **Mã nguồn tạo / sửa:**
  - `src/Server/GymManagement.Server.Core/Contracts/IReportService.cs` (6 phương thức nghiệp vụ async)
  - `src/Server/GymManagement.Server.Core/DTOs/Reports/ReportDtos.cs` (17 request/response record types)
  - `src/Server/GymManagement.Server.Core/Services/Reporting/ReportExportHelper.cs` (Bộ đóng gói file OpenXML .xlsx và PDF 1.4 thuần .NET BCL)
  - `src/Server/GymManagement.Server.Core/Services/ReportService.cs` (Hiện thực tổng hợp đa chiều doanh thu, biến động hội viên, lưu lượng check-in, hiệu suất PT, dashboard)
  - `src/Server/GymManagement.Server.Api/Program.cs` (Đăng ký `AddScoped<IReportService, ReportService>()`)
  - `task.md` & `Document/task.md` (Đánh dấu hoàn thành `[x]` Task 2.7)
* **Kiểm thử tương ứng (Verification):**
  - `tests/GymManagement.Server.UnitTests/Services/ReportServiceTests.cs` (16 test cases mới theo chuẩn AAA kiểm tra đầy đủ các chiều báo cáo và xử lý ngoại lệ).
  - Kết quả kiểm thử toàn solution:
    - `UnitTests`: 108/108 test cases pass 100%.
    - `DataTests`: 42/42 test cases pass 100%.
    - `ApiTests`: 13/13 test cases pass 100%.
    - **Tổng cộng: 163/163 test cases pass 100%.**
* **Bảng CSDL liên quan:** 6 bảng chuẩn (`Invoice`, `MemberSubscription`, `CheckInLog`, `PTTrainingSession`, `[User]`, `PackagePlan`).
* **Ràng buộc nghiệp vụ & Kiến trúc:**
  - **Doanh thu thực thu:** Tổng hợp từ `Invoice` với trạng thái `PAID` và `REFUNDED`, phân loại theo hình thức thanh toán (`CASH`, `VIETQR`, `POS`), gói tập và xu hướng theo mốc thời gian.
  - **Biến động hội viên:** Tính toán số hội viên mới, gia hạn (`SUBSCRIPTION_RENEWAL`), bảo lưu (`FROZEN`), hủy (`CANCELLED`), hết hạn (`EXPIRED`) và tỷ lệ duy trì (`RetentionRatePercentage`).
  - **Lưu lượng điểm danh:** Phân tích 24 khung giờ cao điểm (Peak Hours Heatmap) và thống kê tỷ lệ từ chối kèm nguyên nhân chi tiết.
  - **Hiệu suất PT:** Tính toán tỷ lệ hoàn tất ca dạy, số lượng học viên phụ trách và điểm đánh giá trung bình 1–5 sao.
  - **Xuất dữ liệu streaming an toàn:** Sử dụng `MemoryStream` và `ZipArchive` tạo tệp tin `.xlsx` OpenXML và `.pdf` chuẩn không phụ thuộc bên ngoài, phản hồi nhanh dưới 3 giây.
  - **Quy chuẩn lập trình:** 100% không sử dụng toán tử 3 ngôi (`? :`), không dùng expression-bodied methods `=>` trên hàm nghiệp vụ, toàn bộ constants/fields ở đầu file.

### [MẪU ENTRY CHUẨN ĐỂ GHI NHẬN KHI HOÀN THÀNH TASK TIẾP THEO]
```markdown
### 📌 [Task X.Y] <Tên Task>
* **Ngày hoàn tất:** YYYY-MM-DD | **Commit:** `<hash>` (nhánh `<branch>`)
* **Phân hệ nghiệp vụ liên quan:** UC... (US ...) - <Tên Use Case trong PRD>
* **Ranh giới thành phần:** `Server.Core` / `Server.Data` / `Server.Api` / `Client.Desktop` / `Client.Web`
* **Mã nguồn tạo / sửa:**
  - `<đường_dẫn_file_1>`
  - `<đường_dẫn_file_2>`
* **Kiểm thử tương ứng (Verification):**
  - `<đường_dẫn_file_test>` (Số lượng test cases, độ phủ code %)
* **Bảng CSDL liên quan:** `TableName1`, `TableName2`
* **Ràng buộc nghiệp vụ & Giao dịch:** <Mô tả tóm tắt Transaction ACID, quy tắc kiểm tra, exceptions xử lý>
```
