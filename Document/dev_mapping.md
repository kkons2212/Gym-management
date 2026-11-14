# SỔ ÁNH XẠ KỸ THUẬT & TRUY VẾT TRIỂN KHAI (TECHNICAL IMPLEMENTATION & MAPPING LEDGER)
# HỆ THỐNG QUẢN LÝ PHÒNG TẬP THỂ THAO (GYM MANAGEMENT SYSTEM)
**Tài liệu tham chiếu:** [`PRD.md`](PRD.md) | [`predevrule.md`](predevrule.md) | [`task.md`](task.md) | [`dev_rule.md`](dev_rule.md)

---

## 1. BẢNG TIẾN ĐỘ TỔNG THỂ (IMPLEMENTATION DASHBOARD)

* **Tổng số Nhiệm vụ trong Roadmap:** 33 Tasks
* **Tiến độ Hoàn thành:** **28 / 33 Tasks (84.8%)**
* **Trạng thái Triển khai theo 3 Khối Thành phần Kiến trúc:**
  * 🟢 **Khối Hạ Tầng & Tiêu Chuẩn (Infrastructure & Standards):** Đã hoàn tất 100% (Task 0.0, Task 0.0b, Task 5.0, Task 0.1)
  * 🟢 **Khối Khởi tạo Nền tảng (Phase 0 Scaffolding):** Đã hoàn tất 100% (Task 0.1, 0.2, 0.3, 0.4)
  * 🟡 **Khối Máy Chủ (Server Component):** Đang triển khai Giai đoạn 1, 2 & 3 (Đã hoàn tất Task 1.1.1 – 1.1.5, Task 1.2.1, 1.2.2, Task 1.3.1, 1.3.2, 1.3.3, Task 2.1, Task 2.5, Task 2.6, Task 3.1, Task 3.2)
  * 🟢 **Khối Giao Diện Admin Desktop (Task 4.1):** Đã hoàn tất 100% (Task 4.1.1, 4.1.2, 4.1.3 - WinUI 3 MVVM)
  * 🟢 **Khối Giao Diện Web Portal (Task 4.2):** Đã hoàn tất 100% (Task 4.2.1, 4.2.2, 4.2.3 - Blazor Server Mobile Portal)

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
| **Task 4.2.1** | Quản lý phiên làm việc Member & PT | UC01 (Xác thực & Phân quyền Web) | `Client.Web` | `src/Client/GymManagement.Web/Services/*`, `Components/Layout/*`, `Components/Pages/Login.razor` | Blazor Build & Auth State Verification | `[User]`, `Role` | `dd35e13` |
| **Task 4.2.2** | Cổng Hội viên: QR số, Lịch PT, InBody | UC03, UC05, UC09, UC10 | `Client.Web` | `Components/Member/*`, `Components/Pages/Member/*` | Blazor Build 0 Warnings 0 Errors | `MemberSubscription`, `PackagePlan`, `PTTrainingSession`, `MemberBodyMetric` | `dd35e13` |
| **Task 4.2.3** | Cổng Huấn luyện viên PT: Lịch dạy, InBody | UC08 (8.1.1, 8.1.2, 8.2.1, 8.2.2, 8.2.3) | `Client.Web` | `Components/PT/*`, `Components/Pages/PT/*` | Blazor Build 0 Warnings 0 Errors | `PTAvailability`, `PTTrainingSession`, `MemberBodyMetric` | `dd35e13` |
| **Task 2.7** | ReportService Component (UC06) | UC06 (US 06.1, US 06.2) — Xem & Xuất báo cáo thống kê BI đa chiều | `Server.Core` & `Server.Api` | `Contracts/IReportService.cs`, `DTOs/Reports/ReportDtos.cs`, `Services/Reporting/ReportExportHelper.cs`, `Services/ReportService.cs`, `Server.Api/Program.cs` | `tests/.../Services/ReportServiceTests.cs` (16 tests) | `Invoice`, `MemberSubscription`, `CheckInLog`, `PTTrainingSession`, `[User]`, `PackagePlan` | `ce9e620` |
| **Task 3.3** | API Controllers Component | Toàn bộ 10 Epics (Cổng RESTful API công khai cho Client) | `Server.Api` | `Controllers/ApiControllerBase.cs`, `AuthController.cs`, `MembersController.cs`, `CheckInController.cs`, `PtController.cs`, `InvoicesController.cs`, `ReportsController.cs`, `SettingsController.cs`, `Program.cs` | `tests/GymManagement.Server.ApiTests/Controllers/*Tests.cs` (18 tests mới) | 9 bảng CSDL | `9016fe4` |
| **Task 4.1** | Admin Desktop App (WinUI 3 MVVM) | Toàn bộ 10 Epics (Admin & Thu ngân) | `Client.Desktop` | `src/Client/GymManagement.Desktop/Styles/ColorTokens.xaml`, `Views/*` (In-place Wipe về canvas chuẩn), `ViewModels/*`, `App.xaml(.cs)`, `MainWindow.xaml(.cs)` | Biên dịch WinUI 3 Desktop `0 Warning(s), 0 Error(s)` | 9 bảng CSDL | `6832b09` (WIP) |
| **Task 4.1.4** | Tích hợp Camera & Quét mã QR Check-in thời gian thực | UC03 (US 03.1 - Quét QR qua Camera tại quầy) | `Client.Desktop` | `Services/Camera/*`, `ViewModels/CheckInCounterViewModel.cs`, `Views/CheckInCounterPage.xaml(.cs)`, `App.xaml.cs` | WinUI 3 Desktop Build `0 Warning(s), 0 Error(s)` & Solution Tests 186/186 pass (100%) | `CheckInLog`, `MemberSubscription`, `[User]` | `79fd3df` |
| **Docker DB** | Cấu hình Docker SQL Server, DDL Seed, Hỗ trợ Unicode (NVARCHAR) & CI Test | Toàn bộ 10 Epics (9 bảng & Seed Admin) | `Server.Data`, `Server.Api`, `Document/`, CI | `Document/init_docker_db.sql`, `GymManagement_Physical_Model.sql`, `scripts/setup-docker-db.ps1`, `src/Server/GymManagement.Server.Data/Configurations/*`, `appsettings.json`, `DependencyInjection.cs`, `.github/workflows/ci.yml` | Python Schema Verifier, Ephemeral Docker SQL Server Test in CI & Server.DataTests (42/42 pass) | 9 bảng CSDL (`nvarchar` cho toàn bộ cột text tiếng Việt) | `91abb28` |

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

---

### 📌 [Task 4.2.1, 4.2.2, 4.2.3] Cổng Thông Tin Web Hội Viên & Huấn Luyện Viên PT (Blazor Server)
* **Ngày hoàn tất:** 09/11/2026 | **Commit:** `dd35e13` (nhánh `feat/task-4.2-member-pt-web-portal`)
* **Phân hệ nghiệp vụ liên quan:** UC01 (Auth & Session), UC03 (Check-in QR tĩnh chứa mã hội viên), UC05 (VietQR NAPAS 247), UC08 (PT Weekly Schedule, Slots, InBody & Complete), UC09 (Member Profile & Workout History), UC10 (PT Booking, Cancel & Rating).
* **Ranh giới thành phần:** `src/Client/GymManagement.Web/`
* **Mã nguồn tạo / sửa:**
  - **Hạ tầng & Xác thực (Task 4.2.1):**
    - `Services/IWebApiClient.cs`: Hợp đồng kết nối Client Web với Server.
    - `Services/CustomAuthenticationStateProvider.cs`: Quản lý phiên làm việc, giải mã vai trò Member/PT.
    - `Services/MockWebApiClient.cs`: Giả lập dữ liệu tuân thủ 100% CSDL 9 bảng.
    - `Components/Layout/NavMenu.razor` & `MainLayout.razor`: Mobile-first shell (440px max-width), bottom navigation bar theo vai trò.
    - `Components/Pages/Login.razor`: Màn hình đăng nhập chuyển hướng tự động theo vai trò.
    - `wwwroot/app.css`: Token màu 100% Dark Mode (#0F172A, #1E293B, #38BDF8), slot-pt-open trung tính, tabular-nums.
  - **Cổng Hội Viên (Task 4.2.2):**
    - `Components/Member/DigitalSubscriptionWidget.razor`: Gói tập số, đếm số buổi còn lại, mã QR hội viên cố định.
    - `Components/Member/VietQrModal.razor`: Thanh toán chuyển khoản NAPAS 247 (220x220px, 10 phút, simulation sandbox).
    - `Components/Member/PackageCatalog.razor`: Danh mục gói tập giá niêm yết cố định.
    - `Components/Member/PtBookingWizard.razor`: Wizard 3 bước (chọn ngày -> lưới slot trung tính -> xác nhận).
    - `Components/Member/CancelBookingModal.razor`: Chắn bảo vệ hủy ca (< 2h cảnh báo đỏ mất buổi, nút giữ lịch tập làm nút chính).
    - `Components/Member/RescheduleBookingModal.razor`: Đổi lịch sang khung giờ khác của cùng PT.
    - `Components/Member/SessionRatingModal.razor`: Đánh giá sao 1-5 và nhận xét buổi tập đã hoàn thành.
    - `Components/Member/InBodyDashboard.razor`: 4 thẻ chỉ số, thước đo BMI 4 dải màu với kim chỉ, biểu đồ SVG diễn tiến cân nặng.
    - `Components/Member/WorkoutHistoryTimeline.razor`: Dòng thời gian kết hợp quét mã vào cổng và ca tập PT kèm bộ lọc.
    - `Components/Pages/Member/*`: `Dashboard.razor`, `Packages.razor`, `PtBooking.razor`, `InBody.razor`, `History.razor`.
  - **Cổng Huấn Luyện Viên PT (Task 4.2.3):**
    - `Components/PT/PtAvailabilityGrid.razor`: Lưới bật/tắt lịch rảnh trong tuần (06:00-21:00), khóa slot đã có người đặt.
    - `Components/PT/PtWeeklySchedule.razor`: Lịch huấn luyện theo tuần, 1-tap hoàn tất ca tập có ghi chú (tuyệt đối KHÔNG có link `tel:`).
    - `Components/PT/PtStudentsList.razor`: Danh sách học viên phụ trách (số điện thoại văn bản thuần, theo dõi số buổi).
    - `Components/PT/InBodyEntryModal.razor`: Form nhập chỉ số InBody với cơ chế tự động tính BMI tức thời.
    - `Components/Pages/PT/*`: `Schedule.razor`, `Availability.razor`, `Students.razor`.
* **Kiểm thử tương ứng (Verification):**
  - Biên dịch toàn bộ Solution: `GymManagement.Web`, `GymManagement.Server.Core`, `GymManagement.Desktop`, `GymManagement.Server.UnitTests` -> **0 Warnings, 0 Errors**.
  - Kiểm tra giao diện: Tuyệt đối không xuất hiện mã thô `(UC...)` hoặc thẻ UC trên text hiển thị người dùng.
  - Kiểm tra thuật ngữ: Chỉ dùng "gói tập", không dùng "thẻ tập".
* **Bảng CSDL liên quan:** `[User]`, `Role`, `PackagePlan`, `MemberSubscription`, `PTAvailability`, `PTTrainingSession`, `CheckInLog`, `Invoice`, `MemberBodyMetric`.
* **Ràng buộc nghiệp vụ & Kiến trúc:**
  - Khung giờ PT trung tính (`#334155`), chỉ đổi xanh dương khi chọn, không bao giờ dùng xanh lá cho slot trống.
  - Chống bấm đúp trên tất cả các nút submit (`_isSubmitting = true`).
  - Hủy ca dưới 2 giờ bắt buộc xác nhận mất buổi.
  - Tuân thủ nghiêm ngặt Dark Mode WCAG AA, không hỗ trợ Light Mode.
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

### 📌 [Task 3.3] Xây dựng các API Controllers công khai giao tiếp GymManagement API
* **Ngày hoàn tất:** 09/11/2026 | **Commit:** `9016fe4` (nhánh `feat/task-3.3-api-controllers`)
* **Phân hệ nghiệp vụ liên quan:** Toàn bộ 10 Epics (UC01 – UC10) — Cung cấp giao diện RESTful HTTP công khai cho Client Component.
* **Ranh giới thành phần:** `Server.Api` (Controllers & Configuration) & `Server.ApiTests`.
* **Mã nguồn tạo / sửa:**
  - `src/Server/GymManagement.Server.Api/Controllers/ApiControllerBase.cs` (Lớp Controller cơ sở chuẩn hóa phản hồi `ApiResponse<T>`, `CreatedResponse`, và Claims helpers)
  - `src/Server/GymManagement.Server.Api/Controllers/AuthController.cs` (Cổng `/api/v1/auth`: Đăng nhập cấp JWT token, tra cứu hồ sơ phiên làm việc)
  - `src/Server/GymManagement.Server.Api/Controllers/MembersController.cs` (Cổng `/api/v1/members`: Tra cứu, đăng ký mới, gia hạn, nâng cấp, bảo lưu, hủy gói, lịch sử tập)
  - `src/Server/GymManagement.Server.Api/Controllers/CheckInController.cs` (Cổng `/api/v1/checkin`: Quét QR webcam, tra cứu thủ công SĐT/Mã, nhật ký và tổng lượt check-in hôm nay)
  - `src/Server/GymManagement.Server.Api/Controllers/PtController.cs` (Cổng `/api/v1/pt`: Quản lý hồ sơ PT, lịch rảnh slot, đặt/đổi/hủy lịch tập, hoàn tất ca, InBody, đánh giá sao)
  - `src/Server/GymManagement.Server.Api/Controllers/InvoicesController.cs` (Cổng `/api/v1/invoices`: Lập hóa đơn POS, tạo mã VietQR động, xác nhận thanh toán, hoàn tiền)
  - `src/Server/GymManagement.Server.Api/Controllers/ReportsController.cs` (Cổng `/api/v1/reports`: Dashboard tổng quan, doanh thu, biến động hội viên, lưu lượng check-in, hiệu suất PT, xuất Excel/PDF streaming)
  - `src/Server/GymManagement.Server.Api/Controllers/SettingsController.cs` (Cổng `/api/v1/admin`: Quản trị tài khoản nhân viên, phân quyền, khóa/mở khóa, quản lý danh mục gói tập)
  - `src/Server/GymManagement.Server.Api/Program.cs` (Đăng ký toàn bộ 7 Service Components của tầng Core vào DI container)
  - `task.md` & `Document/task.md` (Đánh dấu hoàn tất `[x]` Task 3.3)
* **Kiểm thử tương ứng (Verification):**
  - `tests/GymManagement.Server.ApiTests/Controllers/AuthControllerTests.cs`
  - `tests/GymManagement.Server.ApiTests/Controllers/MembersControllerTests.cs`
  - `tests/GymManagement.Server.ApiTests/Controllers/CheckInControllerTests.cs`
  - `tests/GymManagement.Server.ApiTests/Controllers/PtControllerTests.cs`
  - `tests/GymManagement.Server.ApiTests/Controllers/InvoicesControllerTests.cs`
  - `tests/GymManagement.Server.ApiTests/Controllers/ReportsControllerTests.cs`
  - `tests/GymManagement.Server.ApiTests/Controllers/SettingsControllerTests.cs`
  - Kết quả kiểm thử toàn solution:
    - `ApiTests`: 31/31 test cases pass 100%.
    - `UnitTests`: 110/110 test cases pass 100%.
    - `DataTests`: 42/42 test cases pass 100%.
    - **Tổng cộng: 183/183 test cases pass 100%.**
* **Bảng CSDL liên quan:** Toàn bộ 9 bảng CSDL (`Role`, `[User]`, `PackagePlan`, `PTAvailability`, `PTTrainingSession`, `MemberSubscription`, `CheckInLog`, `Invoice`, `MemberBodyMetric`).
* **Ràng buộc nghiệp vụ & Kiến trúc:**
  - **Chuẩn hóa phản hồi API:** 100% Endpoints trả về cấu trúc đồng nhất `ApiResponse<T>` khi thành công và `ApiErrorResponse` qua Middleware khi gặp ngoại lệ.
  - **Bảo mật & Phân quyền:** Yêu cầu đăng nhập mặc định (`FallbackPolicy`), phân quyền role policies (`AdminOnly`, `PtOnly`, `MemberOnly`), cho phép ẩn danh `[AllowAnonymous]` tại `/health`, `/login`, và tra cứu danh mục gói tập.
  - **Quy chuẩn lập trình:** 100% không sử dụng toán tử 3 ngôi (`? :`), không dùng expression-bodied methods `=>` trên hàm nghiệp vụ, toàn bộ constants/fields ở đầu file.

### 📌 [Task 4.1] Hoàn Thiện Toàn Diện 6 Giai Đoạn Admin Desktop App (WinUI 3 MVVM) — Fulfillment Phase
* **Ngày hoàn tất:** 10/11/2026 | **Nhánh:** `feat/task-4.1-desktop-fulfillment`
* **Phân hệ nghiệp vụ liên quan:** Toàn bộ 10 Epics (UC01 – UC10) dành riêng cho Quản trị viên (Admin) và Thu ngân theo [`PRD.md`](PRD.md).
* **Ranh giới thành phần:** `Client.Desktop` (Views, ViewModels, Services, Converters, Dialogs, Styles).
* **Kết quả thực hiện 6 Giai đoạn (Fulfillment Plan):**
  1. **Phase 1 (Foundation, Token & Login UX):** Bảng màu 6 phân tầng Dark Mode L0-L5 đạt chuẩn WCAG 2.2 AA (`ColorTokens.xaml`), Checkbox Ghi nhớ đăng nhập (RememberMe), Idempotency lock, tự động xóa mật khẩu khi lỗi và bản địa hóa 100% tiếng Việt cho lỗi mạng (`LoginPage`, `LoginViewModel`).
  2. **Phase 2 (Member Lifecycle & PT Assignment):** Dialog Sửa hồ sơ hội viên (`MemberDetailDialog` - UC 02.2), Gia hạn/nâng cấp gói (`RenewUpgradeModal` - UC 02.3), Hủy gói (`CancelSubscriptionDialog` - UC 02.5), Bảo lưu gói (`FreezeSubscriptionDialog` - UC 02.4), Đo InBody (`InBodyEntryDialog` - UC 02.6), và Phân công HLV cá nhân (`PtAssignDialog` - UC 04.4).
  3. **Phase 3 (POS Invoicing, History & K80 Receipts):** Trang Lịch sử hóa đơn (`InvoiceHistoryPage`, `InvoiceHistoryViewModel` - UC 05.2), Xem trước và in phiếu thu nhiệt K80 (`ReceiptPreviewDialog`), và Xử lý hoàn tiền hóa đơn (`RefundInvoiceDialog` - UC 05.3).
  4. **Phase 4 (PT Management & Scheduling):** Dialog Thêm HLV mới (`TrainerProfileDialog` - UC 04.1), Cập nhật hồ sơ HLV (UC 04.3), Quản lý danh sách HLV (`TrainerListDialog` - UC 04.2), Hàng rào chống trùng lịch ca tập (Collision Guardrail), và Xác nhận kết quả buổi tập (`SessionReviewDialog` - UC 04.5).
  5. **Phase 5 (Staff RBAC & Package Catalog Management):** Dialog Cập nhật nhân sự & phân quyền (`UserEditDialog` - UC 7.1.2) kèm chốt chặn bảo mật chống tự hạ quyền Admin đang đăng nhập, Hộp thoại xác nhận Khóa/Mở tài khoản kèm chốt chặn chống tự khóa (UC 7.1.3), Dialog Chỉnh sửa gói tập (`PackagePlanEditDialog` - UC 7.2.2), và Xóa gói tập có kiểm tra ràng buộc giao dịch tự động chuyển sang "Ngừng kinh doanh" `DISCONTINUED` (UC 7.2.3).
  6. **Phase 6 (BI Analytics & Real File Saving):** Nâng cấp Dashboard BI thời gian thực với phân bổ lưu lượng check-in 24h và cơ cấu doanh thu theo gói tập / hình thức thanh toán (UC 06.1). Tích hợp Windows App SDK `FileSavePicker` xuất file thực tế Excel (.xlsx) và PDF (.pdf) lưu trực tiếp vào máy tính người dùng (UC 06.2).
* **Kiểm thử tương ứng (Verification):**
  - Biên dịch toàn bộ dự án Desktop: `dotnet build src/Client/GymManagement.Desktop -p:Platform=x64` đạt **0 Warning(s), 0 Error(s)**.
  - Toàn bộ 183 / 183 unit/integration tests trên solution đều đạt 100% Pass.
  - Tuân thủ nghiêm ngặt [`dev_rule.md`](dev_rule.md) (0 toán tử 3 ngôi `? :`, 0 expression body `=>` trên hàm nghiệp vụ, 100% tiếng Việt trên giao diện UI).
* **Bảng CSDL liên quan:** Toàn bộ 9 bảng CSDL (`Role`, `[User]`, `PackagePlan`, `PTAvailability`, `PTTrainingSession`, `MemberSubscription`, `CheckInLog`, `Invoice`, `MemberBodyMetric`).

### 📌 [Chore / Infrastructure] Chuẩn Bị Cấu Hình Docker SQL Server, Script DDL Seed Data & Kiểm Thử Tự Động Trong CI
* **Ngày hoàn tất:** 14/11/2026 | **Commit:** `91abb28` (nhánh `feat/docker-sqlserver-setup`)
* **Phân hệ nghiệp vụ liên quan:** Nền tảng dữ liệu cho toàn bộ 10 Epics (UC01 – UC10) phục vụ kết nối Docker SQL Server tự host.
* **Ranh giới thành phần:** `Document/`, `scripts/`, `Server.Data`, `Server.Api`, `.github/workflows/`.
* **Mã nguồn tạo / sửa:**
  - `Document/database_setup.md` & `database_setup.md`: Tài liệu hướng dẫn chi tiết quy trình thiết lập, khởi chạy Docker SQL Server container (`docker pull`, `docker run` với các tham số volume mount `D:\ws:/ws`, `mssql_data:/var/opt/mssql`, mật khẩu SA, cổng 1433), nạp DDL & Seed Data, thông tin kết nối và cẩm nang xử lý sự cố thường gặp (Troubleshooting).
  - `Document/GymManagement_Physical_Model.sql` & `Document/init_docker_db.sql`: Chuyển đổi toàn bộ các cột chứa chuỗi văn bản tiếng Việt sang kiểu `NVARCHAR` (`Role.RoleName`, `User.FullName`, `PackagePlan.Name`, `PackagePlan.Description`, `PTTrainingSession.WorkoutNotes`, `PTTrainingSession.ReviewComment`, `CheckInLog.DenialReason`, `Invoice.TransactionType`, `Invoice.PaymentMethod`, `Invoice.RefundReason`, `MemberBodyMetric.Notes`) để khắc phục triệt để lỗi hiển thị dấu hỏi chấm (`?`) khi lưu dữ liệu tiếng Việt có dấu trong SQL Server.
  - `src/Server/GymManagement.Server.Data/Configurations/*`: Đồng bộ kiểu cột `.HasColumnType("nvarchar(...)")` trong toàn bộ 7 file Fluent API mapping (`RoleConfiguration`, `UserConfiguration`, `PackagePlanConfiguration`, `PTTrainingSessionConfiguration`, `CheckInLogConfiguration`, `InvoiceConfiguration`, `MemberBodyMetricConfiguration`).
  - `scripts/setup-docker-db.ps1`: Script PowerShell tiện ích hỗ trợ nạp CSDL vào Docker host qua `docker exec` hoặc `sqlcmd` / .NET `SqlClient` với mật khẩu mặc định cập nhật.
  - `.env.example`: File mẫu biến môi trường phục vụ kết nối Docker SQL Server an toàn, không lộ secret.
  - `src/Server/GymManagement.Server.Api/appsettings.json` & `appsettings.Development.json`: Chuẩn hóa chuỗi kết nối Docker SQL Server `Server=localhost,1433;...`.
  - `src/Server/GymManagement.Server.Data/DependencyInjection.cs`: Tái cấu trúc hàm đọc connection string dạng phẳng (`if / else`), hỗ trợ fallback biến môi trường `ConnectionStrings__DefaultConnection` tuân thủ 100% `dev_rule.md` (0 toán tử 3 ngôi `? :`).
  - `.github/workflows/ci.yml`: Bổ sung bài test Docker SQL Server tự động (khởi chạy ephemeral container `mcr.microsoft.com/mssql/server:2022-latest`, nạp `init_docker_db.sql` và xác thực 9 bảng cùng tài khoản Admin active trong CI).
* **Kiểm thử tương ứng (Verification):**
  - Python schema verification: `init_docker_db.sql` đạt 9/9 bảng và đầy đủ seed data.
  - Kiểm thử `Server.DataTests`: 42 / 42 tests passed 100%.
  - Kiểm thử `Server.UnitTests`: 113 / 113 tests passed 100%.
  - Kiểm thử `Server.ApiTests`: 31 / 31 tests passed 100%.
  - Thực thi trực tiếp trên Docker SQL Server `mssql` xác nhận các ký tự tiếng Việt lưu trữ chuẩn xác UTF-16, không còn hiện tượng ký tự `?`.
  - Tuân thủ 100% [`dev_rule.md`](dev_rule.md) (Zero hardcoded secrets, 0 toán tử 3 ngôi, flat guard clauses).
* **Bảng CSDL liên quan:** Toàn bộ 9 bảng CSDL (`Role`, `[User]`, `PackagePlan`, `PTAvailability`, `PTTrainingSession`, `MemberSubscription`, `CheckInLog`, `Invoice`, `MemberBodyMetric`).

### 📌 [Task 4.1.4] Tích Hợp Camera Phần Cứng & Quét Mã QR Thời Gian Thực Quầy Điểm Danh (UC03 - US 03.1)
* **Ngày hoàn tất:** 14/11/2026 | **Commit:** `79fd3df` (nhánh `feat/us-03.1-camera-scanner-integration`)
* **Phân hệ nghiệp vụ liên quan:** UC03 (US 03.1) - Quét mã QR qua Camera tại quầy tiếp đón hội viên.
* **Ranh giới thành phần:** `Client.Desktop` (Services, ViewModels, Views).
* **Mã nguồn tạo / sửa:**
  - `src/Client/GymManagement.Desktop/Services/Camera/CameraDeviceInfo.cs`: Model thông tin thiết bị camera/webcam trên máy tính.
  - `src/Client/GymManagement.Desktop/Services/Camera/ICameraCaptureService.cs` & `CameraCaptureService.cs`: Dịch vụ thu nhận luồng video thời gian thực qua `Windows.Media.Capture.MediaCapture` và `MediaFrameReader` (chuẩn Windows App SDK), hỗ trợ liệt kê webcam, bật/tắt camera và bắn frame `SoftwareBitmap`.
  - `src/Client/GymManagement.Desktop/Services/Camera/IQrDecoderService.cs` & `QrDecoderService.cs`: Dịch vụ giải mã mã QR từ frame camera / file ảnh qua `ZXing.Net`, tích hợp hàng rào chống quét trùng (Debounce Cooldown 3s cho cùng mã QR, 1.2s cho mã khác).
  - `src/Client/GymManagement.Desktop/App.xaml.cs`: Đăng ký `ICameraCaptureService` và `IQrDecoderService` vào hệ thống DI.
  - `src/Client/GymManagement.Desktop/ViewModels/CheckInCounterViewModel.cs`: Điều phối luồng camera, ComboBox chọn webcam, bật/tắt camera, xử lý quét từ file ảnh, kết nối API `CheckInWebcamQrAsync`, hiệu ứng viền xanh nhấp nháy khi nhận diện mã QR.
  - `src/Client/GymManagement.Desktop/Views/CheckInCounterPage.xaml` & `CheckInCounterPage.xaml.cs`: Giao diện khung ngắm camera thời gian thực qua `Image` + `SoftwareBitmapSource`, overlay viền neon căn chỉnh QR, dọn dẹp và tắt camera an toàn khi chuyển trang.
* **Kiểm thử tương ứng (Verification):**
  - Biên dịch toàn bộ Solution: `dotnet build GymManagement.sln -c Release` đạt **0 Warning(s), 0 Error(s)**.
  - Kiểm thử `Server.UnitTests`: 113 / 113 tests pass (100%).
  - Kiểm thử `Server.DataTests`: 42 / 42 tests pass (100%).
  - Kiểm thử `Server.ApiTests`: 31 / 31 tests pass (100%).
  - Tuân thủ 100% [`dev_rule.md`](dev_rule.md) (0 toán tử 3 ngôi `? :`, flat guard clauses, 100% tiếng Việt trên giao diện).
* **Bảng CSDL liên quan:** `CheckInLog`, `MemberSubscription`, `[User]`.

### 📌 [Task 4.1.4b] Khắc Phục Lệch Hướng Camera, Lật Gương & Căn Giữa Cố Định (Camera Orientation, Mirror & Centered Fix)
* **Ngày hoàn tất:** 14/11/2026 | **Commit:** `ddb63f7` (nhánh `feat/us-03.1-camera-scanner-integration`)
* **Phân hệ nghiệp vụ liên quan:** UC03 (US 03.1) - Quét mã QR qua Camera tại quầy tiếp đón hội viên.
* **Ranh giới thành phần:** `Client.Desktop` (Views, ViewModels).
* **Mã nguồn tạo / sửa:**
  - `src/Client/GymManagement.Desktop/Views/CheckInCounterPage.xaml.cs`: Khai báo tập trung hằng số cấu hình hướng camera (`CAMERA_ROTATION_DEGREES = -90.0`, `CAMERA_FLIP_HORIZONTAL = true`, `CAMERA_FLIP_VERTICAL = false`) ở đầu class tuân thủ nghiêm ngặt `dev_rule.md` (Mục 3.5.5 & 3.6); cài đặt hàm `ApplyCameraTransform()` cấu hình `CompositeTransform` và gán vào `ImgCameraPreview.RenderTransform`.
  - `src/Client/GymManagement.Desktop/Views/CheckInCounterPage.xaml`: Căn giữa cố định video feed (`Stretch="Uniform"`, `HorizontalAlignment="Center"`, `VerticalAlignment="Center"`, `RenderTransformOrigin="0.5,0.5"`), giữ vị trí khung hình cố định không bị trôi hay dịch chuyển.
* **Kiểm thử tương ứng (Verification):**
  - Biên dịch `GymManagement.Desktop.csproj -p:Platform=x64`: **0 Warning(s), 0 Error(s)**.
  - Kiểm thử `Server.UnitTests`: 113 / 113 tests pass (100%).
  - Kiểm thử `Server.DataTests`: 42 / 42 tests pass (100%).
  - Kiểm thử `Server.ApiTests`: 31 / 31 tests pass (100%).
  - Toàn bộ 186/186 tests trong hệ thống pass 100%.
  - Tuân thủ 100% [`dev_rule.md`](dev_rule.md) (0 toán tử 3 ngôi `? :`, flat guard clauses, 100% tiếng Việt trên giao diện & comment).
* **Bảng CSDL liên quan:** `CheckInLog`, `MemberSubscription`, `[User]`.

### 📌 [Task DevOps.2] Thiết Lập Continuous Deployment (CD) Toàn Diện & Tự Động Hóa Phân Phối
* **Ngày hoàn tất:** 14/11/2026 | **Commit:** `d4e5c5f` (nhánh `feat/cd-pipeline-automation`)
* **Phân hệ nghiệp vụ liên quan:** Hạ tầng DevOps, CI/CD Pipeline, Containerization & Client Distribution.
* **Ranh giới thành phần:** `DevOps` / `Docker` / `GitHub Actions` / `Scripts`.
* **Mã nguồn tạo / sửa:**
  - `.github/workflows/cd.yml`: Workflow GitHub Actions tự động hóa quy trình CD: build & push Docker image lên GitHub Container Registry (GHCR) cho Server API và Blazor Web Portal; biên dịch, nén ZIP và phát hành tự động ứng dụng Desktop (WinUI 3) đính kèm GitHub Releases khi có tag `v*.*.*` hoặc push nhánh `main`.
  - `src/Server/GymManagement.Server.Api/Dockerfile`: Multi-stage Dockerfile tối ưu kích thước và bảo mật cho Backend API (.NET 8 runtime).
  - `src/Client/GymManagement.Web/Dockerfile`: Multi-stage Dockerfile cho Blazor Web Portal (.NET 8 runtime).
  - `.dockerignore`: Loại trừ cache, thư mục tạm, Git và build artifacts khỏi Docker build context.
  - `docker-compose.prod.yml`: Điều phối 3 thành phần production: SQL Server 2022 (Volume bền vững `gym_mssql_prod_data`, healthcheck tự động), Server API và Web Portal với mạng nội bộ `gym-network`.
  - `.env.example`: Mẫu khai báo biến môi trường chuẩn cho Production (SA password, JWT key, port mapping, image tags).
  - `scripts/deploy.sh` & `scripts/deploy.ps1`: Script 1-click tự động hóa kiểm tra cấu hình, kéo image mới nhất từ GHCR và kích hoạt container không gián đoạn trên Linux và Windows.
  - `Document/deployment_guide.md`: Cẩm nang hướng dẫn triển khai máy chủ và phân phối ứng dụng Desktop bằng tiếng Việt chi tiết.
* **Kiểm thử tương ứng (Verification):**
  - Cấu hình Docker Compose: `docker compose -f docker-compose.prod.yml config` đạt chuẩn cú pháp 100%.
  - Biên dịch thử nghiệm Docker images `Server.Api` và `Web`: Cả 2 image build thành công 100%.
  - Kiểm thử `Server.UnitTests`: 113 / 113 tests pass (100%).
  - Kiểm thử `Server.DataTests`: 42 / 42 tests pass (100%).
  - Kiểm thử `Server.ApiTests`: 31 / 31 tests pass (100%).
  - Toàn bộ 186/186 tests trong hệ thống pass 100%.
  - Tuân thủ 100% [`dev_rule.md`](dev_rule.md) (0 toán tử 3 ngôi `? :`, flat guard clauses, 100% tiếng Việt trong tài liệu & script).
* **Bảng CSDL liên quan:** Toàn bộ 9 bảng CSDL (`Role`, `[User]`, `PackagePlan`, `PTAvailability`, `PTTrainingSession`, `MemberSubscription`, `CheckInLog`, `Invoice`, `MemberBodyMetric`).

### 📌 [Task DevOps.3] Bổ Sung Cơ Chế Kiểm Tra Cưỡng Chế Phiên Bản .NET 8 (TargetFramework Alignment Guard) & Chuẩn Hóa global.json
* **Ngày hoàn tất:** 14/11/2026 | **Commit:** `c8b3fad` (nhánh `feat/cd-pipeline-automation`)
* **Phân hệ nghiệp vụ liên quan:** Hạ tầng DevOps, CI/CD Pipeline & Architecture Governance.
* **Ranh giới thành phần:** `DevOps` / `CI-CD` / `Governance`.
* **Mã nguồn tạo / sửa:**
  - `global.json`: Chuẩn hóa cấu hình pin .NET 8 SDK (`version: "8.0.400"`, `rollForward: "latestFeature"`), loại bỏ hoàn toàn thuộc tính dị biệt `__Do_Not_Use_This_rollForward`.
  - `.github/workflows/ci.yml`: Bổ sung bước `Verify .NET 8 SDK & Enforce TargetFramework Across All Projects` trong Job `server-compile-and-lint` tự động quét 100% các file `.csproj` trong repo trước khi build; bổ sung bước log phiên bản `dotnet --version` trong Job `client-compile-and-build`.
* **Kiểm thử tương ứng (Verification):**
  - Kiểm thử quét 8 dự án trong Solution: Cả 8/8 dự án vượt qua kiểm tra nghiêm ngặt (`net8.0` và `net8.0-windows10.0.19041.0`).
  - Kiểm thử Fail-safe: Thử nghiệm cố tình chèn dự án giả định nhắm `.NET 9` -> Script phát hiện vi phạm và dừng pipeline ngay lập tức (exit code 1).
  - Kiểm thử `Server.UnitTests`: 113 / 113 tests pass (100%).
  - Kiểm thử `Server.DataTests`: 42 / 42 tests pass (100%).
  - Kiểm thử `Server.ApiTests`: 31 / 31 tests pass (100%).
  - Toàn bộ 186/186 tests trong hệ thống pass 100%.
  - Tuân thủ 100% [`dev_rule.md`](dev_rule.md) (0 toán tử 3 ngôi `? :`, flat guard clauses, 100% tiếng Việt).
* **Bảng CSDL liên quan:** Không tác động lược đồ CSDL.

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
