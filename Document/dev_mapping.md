# SỔ ÁNH XẠ KỸ THUẬT & TRUY VẾT TRIỂN KHAI (TECHNICAL IMPLEMENTATION & MAPPING LEDGER)
# HỆ THỐNG QUẢN LÝ PHÒNG TẬP THỂ THAO (GYM MANAGEMENT SYSTEM)
**Tài liệu tham chiếu:** [`PRD.md`](PRD.md) | [`predevrule.md`](predevrule.md) | [`task.md`](task.md) | [`dev_rule.md`](dev_rule.md)

---

## 1. BẢNG TIẾN ĐỘ TỔNG THỂ (IMPLEMENTATION DASHBOARD)

* **Tổng số Nhiệm vụ trong Roadmap:** 33 Tasks
* **Tiến độ Hoàn thành:** **19 / 33 Tasks (57.6%)**
* **Trạng thái Triển khai theo 3 Khối Thành phần Kiến trúc:**
  * 🟢 **Khối Hạ Tầng & Tiêu Chuẩn (Infrastructure & Standards):** Đã hoàn tất 100% (Task 0.0, Task 0.0b, Task 5.0, Task 0.1)
  * 🟢 **Khối Khởi tạo Nền tảng (Phase 0 Scaffolding):** Đã hoàn tất 100% (Task 0.1, 0.2, 0.3, 0.4)
  * 🟡 **Khối Máy Chủ (Server Component):** Đang triển khai Giai đoạn 1, 2 & 3 (Đã hoàn tất Task 1.2.1, 1.2.2, Task 1.3.1, 1.3.2, 1.3.3, Task 2.4, Task 2.5, Task 3.1, Task 3.2)
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
| **Task 3.1** | Global API error handling | GymManagement API | `Server.Api/Middleware` | `GlobalExceptionHandlingMiddleware.cs`, `ApiErrorResponse.cs`, `Program.cs` | `ApiPipelineTests` | N/A | — |
| **Task 3.2** | JWT authentication and role policies | GymManagement API | `Server.Api/Configuration`, `Program.cs` | `JwtOptions.cs`, `Program.cs` | `JwtAuthenticationTests` | N/A | — |
| **Task 5.0** | Kế hoạch CI toàn diện Server & Client | Toàn bộ 10 Epics | `.github/workflows` | `.github/workflows/ci.yml` | Gitleaks, DDL check, Quality Gate | 9 bảng DDL | `2131cff` |
| **Task 1.2.1** | Interface Database API: GenericRepository | Toàn bộ 10 Epics (CRUD an toàn) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Repositories/GenericRepository.cs`, `Contracts/Data/IGenericRepository.cs` | `tests/GymManagement.Server.DataTests/Repositories/GenericRepositoryTests.cs` (13 tests) | 9 bảng CSDL | `04a79c7` |
| **Task 1.2.2** | Interface Database API: UnitOfWork (ACID) | Toàn bộ 10 Epics (Transaction ACID) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Repositories/UnitOfWork.cs`, `Contracts/Data/IUnitOfWork.cs`, `Persistence/GymDbContext.cs` | `tests/GymManagement.Server.DataTests/Repositories/UnitOfWorkTests.cs` (9 tests) | 9 bảng CSDL | `04a79c7` |
| **Task 1.3.1** | Cổng Payment API (VietQR Adapter) | UC05 (Thanh toán POS, VietQR động) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Adapters/VietQrAdapter.cs`, `Contracts/Adapters/IVietQrAdapter.cs`, `DTOs/Adapters/VietQrDtos.cs` | `tests/GymManagement.Server.DataTests/Adapters/VietQrAdapterTests.cs` (4 tests) | `Invoice` | `b2f0cab` |
| **Task 1.3.2** | Cổng SMS Gateway (Notification Adapter) | UC02, UC04, UC05 (Hóa đơn Email, Nhắc lịch PT SMS) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Adapters/NotificationAdapter.cs`, `Contracts/Adapters/INotificationAdapter.cs`, `DTOs/Adapters/NotificationDtos.cs` | `tests/GymManagement.Server.DataTests/Adapters/NotificationAdapterTests.cs` (4 tests) | `[User]`, `Invoice` | `b2f0cab` |
| **Task 1.3.3** | Cổng Camera Scanner (QR Decoder & Token Validator) | UC03 (Check-in QR quay vòng 60s) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Adapters/CameraScannerAdapter.cs`, `Contracts/Adapters/ICameraScannerAdapter.cs`, `DTOs/Adapters/QrScanDtos.cs` | `tests/GymManagement.Server.DataTests/Adapters/CameraScannerAdapterTests.cs` (5 tests) | `CheckInLog`, `[User]` | `b2f0cab` |
| **Task 2.4** | BillingService Component | UC05 (POS, VietQR, hóa đơn, hoàn tiền nội bộ) | `Server.Core`, `Server.Data`, `Server.Api` | `Entities/BillingEntities.cs`, `Contracts/IBillingService.cs`, `Services/BillingService.cs`, `GymDbContext.cs`, VietQR adapter DTO/options, `Program.cs` | `BillingServiceTests.cs` (5 tests), `VietQrAdapterTests.cs` (4 tests), `GymDbContextBillingModelTests.cs` | `Invoice`, `MemberSubscription`, `PackagePlan`, `[User]` | — |
| **Task 2.5** | PtService Component (UC04, UC08, UC10) | UC04, UC08, UC10 (PT profile, lịch rảnh, đặt/hủy/đổi ca, InBody, đánh giá) | `Server.Core` | `Enums/PtSessionStatus.cs`, `DTOs/PT/PtDtos.cs`, `Contracts/IPtService.cs`, `Services/PtService.cs` | `tests/GymManagement.Server.UnitTests/Services/PtServiceTests.cs` (16 tests) | `PTAvailability`, `PTTrainingSession`, `MemberSubscription`, `MemberBodyMetric`, `[User]` | `03e9053` |
| **Task 4.1** | Admin Desktop App (WinUI 3 MVVM) | UC01, UC02, UC04, UC05, UC07, UC10 | `src/Client/GymManagement.Desktop`, `Core/DTOs` | CheckIn, PosBilling, Members, PtSchedule, PackagePlans, Dashboard, Settings, Dialogs | WinUI 3 Release Compilation 0 Errors | 9 bảng DDL | `89c525a` |

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

### 📌 [Task 3.2] Xác thực JWT và phân quyền theo vai trò
* **Ngày hoàn tất:** 2026-11-08 | **Nhánh:** `feat/task-3.2-jwt-authentication`
* **Ranh giới thành phần:** `Server.Api`
* **Mã nguồn tạo / sửa:**
  - `src/Server/GymManagement.Server.Api/Configuration/JwtOptions.cs`
  - `src/Server/GymManagement.Server.Api/Program.cs`
  - `tests/GymManagement.Server.ApiTests/JwtAuthenticationTests.cs`
* **Kiểm thử tương ứng:** API integration tests kiểm tra health anonymous, JWT thiếu/sai, role policy Admin và định dạng phản hồi 401/403.
* **Cấu hình JWT:** Bắt buộc cung cấp issuer, audience và signing key tối thiểu 32 byte qua secret store hoặc environment variables; không có khóa mặc định trong repository.

### 📌 [Task 3.1] Chuẩn hóa lỗi API
* **Ngày hoàn tất:** 2026-11-08 | **Nhánh:** `feat/task-3.1-global-errors`
* **Ranh giới thành phần:** `Server.Api` và `Server.Core` DTO dùng chung
* **Mã nguồn tạo / sửa:**
  - `src/Server/GymManagement.Server.Api/Middleware/GlobalExceptionHandlingMiddleware.cs`
  - `src/Server/GymManagement.Server.Api/Program.cs`
  - `src/Server/GymManagement.Server.Core/DTOs/Common/ApiErrorResponse.cs`
  - `tests/GymManagement.Server.ApiTests/ApiPipelineTests.cs`
* **Kiểm thử tương ứng:** API integration tests kiểm tra ánh xạ lỗi, lỗi model validation, ẩn thông tin lỗi nội bộ và anonymous health check.

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

### 📌 [Task 4.1] Toàn Diện Phân Hệ Quản Trị Admin Desktop App (WinUI 3 MVVM)
* **Ngày hoàn tất:** 08/11/2026 | **Nhánh:** `feat/task-4.1-admin-desktop`
* **Phân hệ nghiệp vụ liên quan:** UC01 (Xác thực Admin), UC02 (Hồ sơ hội viên & InBody), UC04 (Quầy điểm danh Check-in & Camera QR), UC05 (Lịch huấn luyện viên PT), UC07 (Bán gói & POS VietQR), UC10 (Báo cáo kinh doanh BI đa chiều & Quản lý gói tập/tài khoản).
* **Ranh giới thành phần:** `src/Client/GymManagement.Desktop`, `src/Server/GymManagement.Server.Core/DTOs/*` (Tuyệt đối không tham chiếu `Server.Data`).
* **Mã nguồn tạo / sửa:**
  - **DTOs (`Server.Core`):** `Identity`, `CheckIn`, `Billing`, `Members`, `PTSchedules`, `Reports`, `Packages`
  - **ViewModels (`Client.Desktop`):** `LoginViewModel`, `CheckInCounterViewModel`, `PosBillingViewModel`, `MembersViewModel`, `PtScheduleViewModel`, `PackagePlansViewModel`, `DashboardViewModel`, `SettingsViewModel`, `ShellViewModel`, `FreezeSubscriptionViewModel`, `InBodyEntryViewModel`
  - **Views & Dialogs (`Client.Desktop`):**
    - `LoginPage.xaml` / `.xaml.cs` (Xác thực đăng nhập Admin)
    - `CheckInCounterPage.xaml` / `.xaml.cs` (Camera scanner QR code, tín hiệu đèn xanh/đỏ, âm thanh beep/buzz, ghi đè thủ công có lý do)
    - `PosBillingPage.xaml` / `.xaml.cs` (Lập hóa đơn POS, dialog VietQR chuẩn NAPAS 247, mô phỏng 10s thanh toán thành công, in hóa đơn K80)
    - `MembersPage.xaml` / `.xaml.cs` (Tìm kiếm hội viên, xem chi tiết gói, Dialog bảo lưu `FreezeSubscriptionDialog`, Dialog đo chỉ số `InBodyEntryDialog` với gauge BMI tức thì)
    - `PtSchedulePage.xaml` / `.xaml.cs` (Lịch làm việc PT, chống xung đột thời gian collision, trạng thái màu sắc)
    - `PackagePlansPage.xaml` / `.xaml.cs` (Quản lý và tạo mới gói tập)
    - `DashboardPage.xaml` / `.xaml.cs` (Báo cáo doanh thu BI đa chiều, tỷ lệ lấp đầy PT, giờ cao điểm check-in)
    - `SettingsPage.xaml` / `.xaml.cs` (Quản lý tài khoản Admin/PT, khóa tài khoản có chốt chặn bảo mật không khóa chính mình)
    - `MainPage.xaml` / `MainWindow.xaml` (Giao diện Shell Sidebar điều hướng mượt mà, phím tắt F1-F5)
  - **Dịch vụ & Tiện ích:** `IApiClient`, `MockApiClient`, `AdminSessionService`, `AudioFeedbackService`, Value Converters, Design Tokens màu tối
* **Kiểm thử tương ứng (Verification):**
  - Unit Tests: `tests/GymManagement.Server.UnitTests/DTOs/AdminDesktopDtoTests.cs` (5 tests mới cho DTOs: PackagePlanDto, CheckInRequest, CreateInvoiceRequest, FreezeSubscriptionRequest, BookPtSlotRequest; 100% Pass).
  - Biên dịch Release `dotnet build src/Client/GymManagement.Desktop -c Release`: 100% thành công (0 errors, 0 warnings kiến trúc).
  - Xác thực kiến trúc Pre-dev Rule 1: Không có tham chiếu tới `GymManagement.Server.Data`.
* **Ghi chú kiến trúc & Chuẩn hóa theo `dev_rule.md`:**
  - **DoD 6 (Cú pháp rẽ nhánh & Bố cục):** Triệt tiêu 100% toán tử 3 ngôi (`? :`), toàn bộ rẽ nhánh sử dụng `if / else` tường minh có khối `{ ... }`; loại bỏ các expression-bodied methods trên hàm nghiệp vụ; tách biệt named methods riêng cho các event handlers; toàn bộ fields/properties tập trung ở đầu class.
  - **Nghiệp vụ phòng gym:** Tuân thủ triệt để vai trò duy nhất Quản trị viên (`Admin`), thanh toán đúng theo giá niêm yết (không chiết khấu/voucher), điểm danh bằng camera quét QR từ màn hình điện thoại hội viên kèm nhập số điện thoại thủ công.

---

---

### 📌 [Task 2.5] Hiện thực PtService Component (UC04, UC08, UC10)
* **Ngày hoàn tất:** 08/11/2026 | **Commit:** `03e9053` | **Nhánh:** `feat/task-2.5-pt-service`
* **Phân hệ nghiệp vụ liên quan:**
  - UC04 (Admin quản lý hồ sơ PT, phân công PT, xếp lịch dạy, xác nhận kết quả ca tập)
  - UC08 (PT đăng ký lịch rảnh, xem học viên, ghi InBody, xác nhận hoàn thành ca dạy)
  - UC10 (Hội viên đặt/hủy/đổi lịch tập PT, đánh giá sao)
* **Ranh giới thành phần:** `Server.Core` (Contracts, DTOs, Enums, Services)
* **Mã nguồn tạo / sửa:**
  - `src/Server/GymManagement.Server.Core/Enums/PtSessionStatus.cs` (6 trạng thái: Booked, Completed, Cancelled, AbsentMember, AbsentPt, Postponed)
  - `src/Server/GymManagement.Server.Core/DTOs/PT/PtDtos.cs` (15 record types: PtProfileDto, MemberSummaryDto, PtSlotDto, PtSlotUpsertDto, PtSessionDto, + 9 Request types)
  - `src/Server/GymManagement.Server.Core/Contracts/IPtService.cs` (17 method signatures chia 6 nhóm nghiệp vụ)
  - `src/Server/GymManagement.Server.Core/Services/PtService.cs` (PtService sealed class, 5 entity stubs, ACID transactions)
  - `src/Server/GymManagement.Server.Core/GymManagement.Server.Core.csproj` (thêm InternalsVisibleTo cho UnitTests và DynamicProxyGenAssembly2)
  - `tests/GymManagement.Server.UnitTests/GymManagement.Server.UnitTests.csproj` (đổi TargetFramework sang net10.0)
* **Kiểm thử tương ứng (Verification):**
  - `tests/GymManagement.Server.UnitTests/Services/PtServiceTests.cs` (16 test cases — 16/16 pass 100%)
  - Coverage: SaveTrainer (phone/email duplicate), SetTrainerStatus (not found), CheckDeactivation (has students / no students), AssignPt (max student limit), BookSession (slot booked / no PT sessions), CancelSession (late / wrong status), SubmitRating (out of range / already rated / not completed), RecordInBody (invalid weight)
* **Bảng CSDL liên quan:** `PTAvailability`, `PTTrainingSession`, `MemberSubscription`, `MemberBodyMetric`, `[User]`
* **Ràng buộc nghiệp vụ & Giao dịch:**
  - **Quy tắc phạt hủy ca:** `CancellationGracePeriodHours = 2` — hủy < 2h → `ForceLateCancellation = true` mới thực thi, không hoàn buổi.
  - **Giới hạn học viên PT:** `MaxStudentsPerPt = 10` — throw `BusinessRuleException` khi vượt.
  - **Slot lock:** `is_Booked = 1` không cho phép hủy từ PT (UC 8.1.1 — phụ 5.1).
  - **Mọi luồng đa bảng** (BookSession, CancelSession, RescheduleSession, CompleteSession, ConfirmResult, RecordInBody) đều bọc trong `ExecuteTransactionAsync`.
  - **SMS không block nghiệp vụ:** `SendSessionReminderSafeAsync` catch exception riêng, chỉ log Warning.
* **Ghi chú kiến trúc:**
  - `PTAvailability`, `PTTrainingSession` và `MemberBodyMetric` vẫn là entity stubs `internal`; `User` và `MemberSubscription` đã được chuyển sang entity dùng chung trong `Server.Core.Entities` để BillingService có thể dùng persistence mapping.
  - `GetPtStudentsAsync` trả `MemberSummaryDto` (không phải `PtProfileDto`) để phản ánh đúng domain — học viên là Member, không phải PT.

### 📌 [Task 2.4] Hiện thực BillingService Component (UC05)
* **Ngày hoàn tất:** 08/11/2026
* **Phân hệ nghiệp vụ liên quan:** UC05 — POS, thanh toán VietQR, hóa đơn và hoàn tiền nội bộ.
* **Ranh giới thành phần:** `Server.Core`, VietQR adapter trong `Server.Data`, đăng ký DI trong `Server.Api`.
* **Mã nguồn tạo / sửa:**
  - `src/Server/GymManagement.Server.Core/Contracts/IBillingService.cs`
  - `src/Server/GymManagement.Server.Core/Entities/BillingEntities.cs`
  - `src/Server/GymManagement.Server.Core/Services/BillingService.cs`
  - `src/Server/GymManagement.Server.Core/DTOs/Billing/BillingDtos.cs`
  - `src/Server/GymManagement.Server.Core/DTOs/Adapters/VietQrDtos.cs`
  - `src/Server/GymManagement.Server.Core/Contracts/Adapters/IVietQrAdapter.cs`
  - `src/Server/GymManagement.Server.Data/Adapters/VietQrAdapter.cs`
  - `src/Server/GymManagement.Server.Data/Persistence/GymDbContext.cs`
  - `src/Server/GymManagement.Server.Api/Program.cs`
* **Kiểm thử tương ứng (Verification):**
  - `tests/GymManagement.Server.UnitTests/Services/BillingServiceTests.cs` (5 tests)
  - `tests/GymManagement.Server.DataTests/Adapters/VietQrAdapterTests.cs` (4 tests)
  - `tests/GymManagement.Server.DataTests/Persistence/GymDbContextBillingModelTests.cs` (1 test)
  - Xác nhận: UnitTests 26/26, DataTests 37/37, ApiTests 13/13.
* **Bảng CSDL liên quan:** `Invoice`, `MemberSubscription`, `PackagePlan`, `[User]`.
* **Ràng buộc nghiệp vụ & Giao dịch:**
  - Tạo hóa đơn và subscription trạng thái `PENDING` cùng transaction; chỉ bật subscription `ACTIVE` sau khi xác nhận thanh toán.
  - VietQR xác minh đúng mã giao dịch và số tiền; phương thức tiền mặt được xác nhận nội bộ.
  - Hoàn tiền chỉ cập nhật nội bộ `Invoice=REFUNDED`, `MemberSubscription=CANCELLED`, không thực hiện chuyển tiền qua ngân hàng.
  - Gửi email biên lai sau khi transaction thanh toán commit; lỗi gửi được ghi log và không đảo ngược thanh toán.
* **Ghi chú phạm vi:**
  - Đã ánh xạ EF Core cho bốn bảng cần thiết của billing (`Invoice`, `MemberSubscription`, `PackagePlan`, `[User]`); các bảng và ràng buộc còn lại vẫn thuộc Task 1.1.x.
  - Chưa tạo API controller hay kết nối Desktop API vì không thuộc Task 2.4.

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
