# SỔ ÁNH XẠ KỸ THUẬT & TRUY VẾT TRIỂN KHAI (TECHNICAL IMPLEMENTATION & MAPPING LEDGER)
# HỆ THỐNG QUẢN LÝ PHÒNG TẬP THỂ THAO (GYM MANAGEMENT SYSTEM)
**Tài liệu tham chiếu:** [`PRD.md`](PRD.md) | [`predevrule.md`](predevrule.md) | [`task.md`](task.md) | [`dev_rule.md`](dev_rule.md)

---

## 1. BẢNG TIẾN ĐỘ TỔNG THỂ (IMPLEMENTATION DASHBOARD)

* **Tổng số Nhiệm vụ trong Roadmap:** 33 Tasks
* **Tiến độ Hoàn thành:** **12 / 33 Tasks (36.4%)**
* **Trạng thái Triển khai theo 3 Khối Thành phần Kiến trúc:**
  * 🟢 **Khối Hạ Tầng & Tiêu Chuẩn (Infrastructure & Standards):** Đã hoàn tất 100% (Task 0.0, Task 0.0b, Task 5.0, Task 0.1)
  * 🟢 **Khối Khởi tạo Nền tảng (Phase 0 Scaffolding):** Đã hoàn tất 100% (Task 0.1, 0.2, 0.3, 0.4)
  * 🟡 **Khối Máy Chủ (Server Component):** Đang triển khai Giai đoạn 1 (Đã hoàn tất Task 1.2.1, 1.2.2, Task 1.3.1, 1.3.2, 1.3.3)
  * ⚪ **Khối Giao Diện (Client Component - WinUI 3 & Blazor):** Khung ứng dụng đã sẵn sàng, chờ tích hợp nghiệp vụ (Giai đoạn 4.0)

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
| **Task 1.2.1** | Interface Database API: GenericRepository | Toàn bộ 10 Epics (CRUD an toàn) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Repositories/GenericRepository.cs`, `Contracts/Data/IGenericRepository.cs` | `tests/GymManagement.Server.DataTests/Repositories/GenericRepositoryTests.cs` (13 tests) | 9 bảng CSDL | `04a79c7` |
| **Task 1.2.2** | Interface Database API: UnitOfWork (ACID) | Toàn bộ 10 Epics (Transaction ACID) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Repositories/UnitOfWork.cs`, `Contracts/Data/IUnitOfWork.cs`, `Persistence/GymDbContext.cs` | `tests/GymManagement.Server.DataTests/Repositories/UnitOfWorkTests.cs` (9 tests) | 9 bảng CSDL | `04a79c7` |
| **Task 1.3.1** | Cổng Payment API (VietQR Adapter) | UC05 (Thanh toán POS, VietQR động) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Adapters/VietQrAdapter.cs`, `Contracts/Adapters/IVietQrAdapter.cs`, `DTOs/Adapters/VietQrDtos.cs` | `tests/GymManagement.Server.DataTests/Adapters/VietQrAdapterTests.cs` (4 tests) | `Invoice` | `b2f0cab` |
| **Task 1.3.2** | Cổng SMS Gateway (Notification Adapter) | UC02, UC04, UC05 (Hóa đơn Email, Nhắc lịch PT SMS) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Adapters/NotificationAdapter.cs`, `Contracts/Adapters/INotificationAdapter.cs`, `DTOs/Adapters/NotificationDtos.cs` | `tests/GymManagement.Server.DataTests/Adapters/NotificationAdapterTests.cs` (4 tests) | `[User]`, `Invoice` | `b2f0cab` |
| **Task 1.3.3** | Cổng Camera Scanner (QR Decoder & Token Validator) | UC03 (Check-in QR quay vòng 60s) | `Server.Data` & `Server.Core` | `src/Server/GymManagement.Server.Data/Adapters/CameraScannerAdapter.cs`, `Contracts/Adapters/ICameraScannerAdapter.cs`, `DTOs/Adapters/QrScanDtos.cs` | `tests/GymManagement.Server.DataTests/Adapters/CameraScannerAdapterTests.cs` (5 tests) | `CheckInLog`, `[User]` | `b2f0cab` |

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
