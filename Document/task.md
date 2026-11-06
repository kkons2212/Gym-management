# KẾ HOẠCH & DANH MỤC NHIỆM VỤ PHÁT TRIỂN (PROJECT TASKS ROADMAP)
## HỆ THỐNG QUẢN LÝ PHÒNG TẬP THỂ THAO (GYM MANAGEMENT SYSTEM)
**Kiến trúc:** 3 Lớp Chuẩn (3-Tier Architecture) - Client/Server API-First  
**Phạm vi:** Tạo mới toàn bộ mã nguồn trong thư mục `src` theo đúng đặc tả PRD, Pre-Dev Rules và 37 Use Cases.

---

## 1. QUY ƯỚC KÝ HIỆU & CHÚ THÍCH PHÂN LOẠI (TASK NOTATIONS)

Mỗi nhiệm vụ trong tài liệu này đều được gắn nhãn định danh rõ ràng theo chuẩn:  
`[Loại tác vụ / Layer] [Phân hệ / Module] [Mã Use Case liên quan]`

### 1.1. Phân loại Loại tác vụ (Task Types / Layers):
* `[Type: Scaffolding]`: Khởi tạo project, cấu hình build, cài đặt NuGet, thiết lập phụ thuộc.
* `[Type: Database]`: Thiết kế Entity, cấu hình EF Core DbContext, Index, Migrations, Seed Data.
* `[Type: DAL / Repository]`: Triển khai Generic Repository, Unit of Work, Transaction ACID.
* `[Type: Adapter]`: Tích hợp hệ thống bên ngoài (VietQR, Email/SMS, Camera Scanner).
* `[Type: BLL / Service]`: Triển khai Service logic nghiệp vụ, tính toán thời hạn, quy tắc phạt.
* `[Type: API / Controller]`: Xây dựng REST API Controller (`/api/v1/*`), Middleware, Swagger, JWT.
* `[Type: Frontend / Desktop]`: Xây dựng màn hình WinUI 3 Desktop (MVVM) dành cho Admin tại quầy.
* `[Type: Frontend / Web]`: Xây dựng màn hình Blazor Server Web Portal dành cho PT & Hội viên.
* `[Type: Testing]`: Kiểm thử đơn vị (Unit Test), tích hợp (Integration Test), E2E Walkthrough.

### 1.2. Phân loại Phân hệ Nghiệp vụ (Modules):
* `[Module: Auth & RBAC]`: Xác thực, phân quyền và điều hướng vai trò (UC01).
* `[Module: Member & Subscription]`: Quản trị hồ sơ hội viên và vòng đời thẻ tập (UC02, UC09).
* `[Module: Check-In & Access]`: Điểm danh và kiểm soát quyền vào phòng tập (UC03).
* `[Module: PT Management]`: Điều phối PT, đặt/hủy lịch tập, đo InBody, đánh giá sao (UC04, UC08, UC10).
* `[Module: Billing & POS]`: Thu ngân POS, tạo mã VietQR, thanh toán, hoàn tiền (UC05).
* `[Module: Reports & BI]`: Tổng hợp số liệu đa chiều, Dashboard, xuất Excel/PDF (UC06).
* `[Module: System Config]`: Cấu hình tham số vận hành, danh mục gói tập (UC07).
* `[Module: Infrastructure]`: Hạ tầng kỹ thuật chung, DbContext, Transaction, Utilities.

---

## 2. CẤU TRÚC GIẢI PHÁP & PHÂN TẦNG KIẾN TRÚC MỚI (`src/`)

```text
GymManagement.sln
│
├── src/1.Presentation/ (Tầng Giao diện & Cổng Điều hướng)
│   ├── GymManagement.Api               # ASP.NET Core Web API (Controllers /api/v1/*, JWT, Swagger)
│   ├── GymManagement.Desktop           # WinUI 3 Desktop App (Dành riêng cho Quản trị viên - Admin)
│   └── GymManagement.Web               # Blazor Server Web App (Cổng Hội viên & Huấn luyện viên PT)
│
├── src/2.BusinessLogic/ (Tầng Nghiệp vụ Cốt lõi)
│   └── GymManagement.Core              # Class Library (.NET 8 - Interfaces, DTOs, Enums, Services)
│
└── src/3.DataAccess/ (Tầng Truy xuất & Lưu trữ Dữ liệu)
    └── GymManagement.Infrastructure    # Class Library (.NET 8 - EF Core, 9 Bảng SQL Server, Repositories, Adapters)
```

---

## 3. CHECKLIST NHIỆM VỤ CHI TIẾT THEO TỪNG GIAI ĐOẠN

### GIAI ĐOẠN 0: KHỞI TẠO DỰ ÁN & CẤU TRÚC NỀN TẢNG (SCAFFOLDING)
*Mục tiêu: Thiết lập Solution sạch, cài đặt các package cần thiết và thiết lập phụ thuộc một chiều đúng theo Quy tắc 1 của Pre-Dev Rules.*

- [ ] **Task 0.1:** `[Type: Scaffolding]` `[Module: Infrastructure]` Tạo mới `GymManagement.sln` tại thư mục gốc.
- [ ] **Task 0.2:** `[Type: Scaffolding]` `[Module: Infrastructure]` Tạo dự án `GymManagement.Core` (Class Library .NET 8) tại `src/2.BusinessLogic/`.
- [ ] **Task 0.3:** `[Type: Scaffolding]` `[Module: Infrastructure]` Tạo dự án `GymManagement.Infrastructure` (Class Library .NET 8) tại `src/3.DataAccess/` và tham chiếu đến `GymManagement.Core`.
- [ ] **Task 0.4:** `[Type: Scaffolding]` `[Module: Infrastructure]` Tạo dự án `GymManagement.Api` (ASP.NET Core Web API .NET 8) tại `src/1.Presentation/` và tham chiếu đến `GymManagement.Core`, `GymManagement.Infrastructure`.
- [ ] **Task 0.5:** `[Type: Scaffolding]` `[Module: Infrastructure]` Tạo dự án `GymManagement.Desktop` (WinUI 3 .NET 8) tại `src/1.Presentation/` và cài đặt `CommunityToolkit.Mvvm`, `ZXing.Net`.
- [ ] **Task 0.6:** `[Type: Scaffolding]` `[Module: Infrastructure]` Tạo dự án `GymManagement.Web` (Blazor Server .NET 8) tại `src/1.Presentation/`.
- [ ] **Task 0.7:** `[Type: Scaffolding]` `[Module: Infrastructure]` Cài đặt các gói NuGet cốt lõi:
  - `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Tools` (Infrastructure).
  - `BCrypt.Net-Next` (Core).
  - `Microsoft.AspNetCore.Authentication.JwtBearer`, `Swashbuckle.AspNetCore` (Api).

---

### GIAI ĐOẠN 1: TẦNG DỮ LIỆU (DATA ACCESS LAYER - `GymManagement.Infrastructure`)
*Mục tiêu: Hiện thực hóa mô hình cơ sở dữ liệu đúng 9 bảng theo `GymManagement_Physical_Model.sql` và mẫu Unit of Work.*

#### 1.1. Thực thể & Cấu hình Entity Framework Core (Chuẩn hóa 9 bảng vật lý)
- [ ] **Task 1.1.1:** `[Type: Database]` `[Module: Auth & RBAC]` `[UC01, UC07]` Định nghĩa Entity `Role` (RoleID, RoleName).
- [ ] **Task 1.1.2:** `[Type: Database]` `[Module: Auth & RBAC]` `[UC01, UC07]` Định nghĩa Entity `User` (UserID, RoleID, FullName, Phone, Email, PasswordHash, Gender, Status, created_at).
- [ ] **Task 1.1.3:** `[Type: Database]` `[Module: System Config]` `[UC07]` Định nghĩa Entity `PackagePlan` (PlanId, Name, Description, DurationDays, Price, MaxCheckInSessions, is_PtIncluded, TotalPtSessions, is_Active).
- [ ] **Task 1.1.4:** `[Type: Database]` `[Module: PT Management]` `[UC04, UC08]` Định nghĩa Entity `PTAvailability` (AvailabilityId, AvailableDate, StartTime, EndTime, is_Booked, PtID).
- [ ] **Task 1.1.5:** `[Type: Database]` `[Module: PT Management]` `[UC04, UC08, UC10]` Định nghĩa Entity `PTTrainingSession` (PTSessionId, SessionStartTime, SessionEndTime, Status, WorkoutNotes, RatingStars, ReviewComment, completed_at, MemberID, PtID, AvailabilityId).
- [ ] **Task 1.1.6:** `[Type: Database]` `[Module: Member & Subscription]` `[UC02, UC09]` Định nghĩa Entity `MemberSubscription` (SubscriptionId, QrCode, StartDate, EndDate, RemainingCheckInSessions, RemainingPtSessions, FreezeStartDate, FreezeEndDate, Status, created_at, UserID, AssignedPtId, PlanId, PTSessionId).
- [ ] **Task 1.1.7:** `[Type: Database]` `[Module: Check-In & Access]` `[UC03]` Định nghĩa Entity `CheckInLog` (CheckInId, check_in_at, Status, DenialReason, SubscriptionId, VerifiedByAdminId, UserID).
- [ ] **Task 1.1.8:** `[Type: Database]` `[Module: Billing & POS]` `[UC05]` Định nghĩa Entity `Invoice` (InvoiceId, InvoiceCode, TransactionType, SubtotalAmount, TotalAmount, PaymentMethod, Status, RefundAmount, RefundReason, paid_at, UserID, SubscriptionId, CreatedByAdminId, CheckInLogCheckInId).
- [ ] **Task 1.1.9:** `[Type: Database]` `[Module: PT Management]` `[UC08, UC09]` Định nghĩa Entity `MemberBodyMetric` (BodyMetricId, WeightKg, HeightCm, SkeletalMuscleMass, BodyFatPercentage, Bmi, Notes, measured_at, UserID, PTSessionId, RecordedByPtId).
- [ ] **Task 1.1.10:** `[Type: Database]` `[Module: Infrastructure]` Xây dựng `GymDbContext` sử dụng Fluent API thiết lập quan hệ khóa ngoại (Foreign Keys), Index và Unique constraints (Unique QrCode, InvoiceCode, Phone, CCCD).

#### 1.2. Mẫu Repository & Unit of Work (ACID Transactions)
- [ ] **Task 1.2.1:** `[Type: DAL / Repository]` `[Module: Infrastructure]` Hiện thực `GenericRepository<T>` cho các thao tác CRUD cơ bản.
- [ ] **Task 1.2.2:** `[Type: DAL / Repository]` `[Module: Infrastructure]` Xây dựng `IUnitOfWork` và `UnitOfWork` quản lý commit transaction (`IDbContextTransaction`) đảm bảo toàn vẹn giao dịch cho các tác vụ tác động nhiều bảng.
- [ ] **Task 1.2.3:** `[Type: Database]` `[Module: Infrastructure]` Tạo script Seed Data mẫu (Tài khoản Super Admin mặc định, các vai trò Admin, PT, Member, các gói tập cơ bản).

#### 1.3. External Adapters (Tích hợp Ngoại vi)
- [ ] **Task 1.3.1:** `[Type: Adapter]` `[Module: Billing & POS]` `[UC05]` Xây dựng `VietQrAdapter` gọi REST API Napas (`https://api.vietqr.io/v2/generate`) sinh chuỗi mã QR chuyển khoản ngân hàng động.
- [ ] **Task 1.3.2:** `[Type: Adapter]` `[Module: Billing & POS]` `[UC02, UC05, UC10]` Xây dựng `NotificationAdapter` gửi Email hóa đơn điện tử PDF và SMS thông báo lịch tập/chào mừng.
- [ ] **Task 1.3.3:** `[Type: Adapter]` `[Module: Check-In & Access]` `[UC03]` Xây dựng `CameraScannerAdapter` (sử dụng thư viện DirectShow & ZXing.Net) giải mã chuỗi QR từ thiết bị webcam.

---

### GIAI ĐOẠN 2: TẦNG NGHIỆP VỤ CỐT LÕI (BLL - `GymManagement.Core`)
*Mục tiêu: Đóng gói toàn bộ quy tắc nghiệp vụ, tính toán thời hạn, ràng buộc logic theo 10 phân hệ Use Case.*

#### 2.1. Hợp đồng (Contracts) & DTOs Bất biến
- [ ] **Task 2.1.1:** `[Type: BLL / Service]` `[Module: Infrastructure]` Định nghĩa Enums hệ thống (`UserRole`, `AccountStatus`, `SubscriptionStatus`, `SessionStatus`, `PaymentMethod`, `InvoiceStatus`).
- [ ] **Task 2.1.2:** `[Type: BLL / Service]` `[Module: Infrastructure]` Định nghĩa DTOs dạng `record`/`init-only` cho toàn bộ các luồng dữ liệu (Auth, Members, Check-in, Invoices, PT, Reports, Settings).
- [ ] **Task 2.1.3:** `[Type: BLL / Service]` `[Module: Infrastructure]` Định nghĩa các Service Interfaces tại `Contracts/`: `IAuthService`, `IMemberService`, `ICheckInService`, `IPtService`, `IBillingService`, `IReportService`, `ISystemSettingService`.

#### 2.2. Hiện thực Logic Nghiệp vụ Chi tiết (Services)
- [ ] **Task 2.2.1:** `[Type: BLL / Service]` `[Module: Auth & RBAC]` `[UC01]` **Phân hệ Xác thực & RBAC (`AuthService`):**
  - Xác thực thông tin đăng nhập, đối soát mật khẩu băm BCrypt.
  - Kiểm tra trạng thái tài khoản `Status == ACTIVE` (chặn tài khoản `LOCKED`).
  - Sinh mã JWT Token chứa Claims: `UserID`, `Role`, `FullName`, `Email`.
- [ ] **Task 2.2.2:** `[Type: BLL / Service]` `[Module: System Config]` `[UC07]` **Phân hệ Quản trị Hệ thống & Danh mục (`SystemSettingService`):**
  - CRUD tài khoản nhân sự và phân quyền RBAC (chặn Super Admin tự khóa tài khoản của mình).
  - Quản lý danh mục gói tập (kiểm tra ràng buộc khóa ngoại trước khi chuyển sang `DISCONTINUED`).
  - Quản lý tham số vận hành chung (giờ mở/đóng cửa, quy định hủy ca $<2\text{h}$, số ngày bảo lưu tối đa).
- [ ] **Task 2.2.3:** `[Type: BLL / Service]` `[Module: Member & Subscription]` `[UC02, UC09]` **Phân hệ Quản trị Hội viên & Thẻ tập (`MemberService`):**
  - Tiếp nhận hồ sơ hội viên mới, kiểm tra trùng lặp SĐT/CCCD.
  - Tạo thẻ tập `ACTIVE`, tính ngày hết hạn dựa theo gói, sinh mã QR duy nhất.
  - Gia hạn hoặc nâng cấp gói thẻ (cộng dồn thời hạn hoặc tính phí chênh lệch).
  - Bảo lưu thẻ (`FROZEN`): kiểm tra số ngày cho phép và tự động kéo dài ngày hết hạn tương ứng.
  - Khóa hoặc hủy thẻ (`LOCKED` / `CANCELLED`), cập nhật hồ sơ cá nhân hội viên.
- [ ] **Task 2.2.4:** `[Type: BLL / Service]` `[Module: Billing & POS]` `[UC05]` **Phân hệ Thu ngân, Hóa đơn & Hoàn tiền (`BillingService`):**
  - Lập hóa đơn POS đa phương thức (Tiền mặt, VietQR, Thẻ POS).
  - Tích hợp gọi `VietQrAdapter` sinh mã thanh toán động.
  - Xác nhận nhận tiền: Chuyển hóa đơn `PAID` và kích hoạt thẻ `ACTIVE` trong một Transaction.
  - Xử lý hoàn tiền hóa đơn: Kiểm tra điều kiện hoàn tiền, cập nhật hóa đơn `REFUNDED` và chuyển thẻ sang `CANCELLED`.
  - Xuất mẫu in hóa đơn nhiệt và file PDF.
- [ ] **Task 2.2.5:** `[Type: BLL / Service]` `[Module: Check-In & Access]` `[UC03]` **Phân hệ Điểm danh & Kiểm soát Vào phòng (`CheckInService`):**
  - Điểm danh tự động qua QR hoặc nhập SĐT thủ công.
  - Kiểm tra thẻ `ACTIVE`, còn hạn sử dụng, còn số buổi và đúng khung giờ hoạt động.
  - Trừ 1 buổi tập nguyên tử (Atomic decrement) đối với gói theo lượt.
  - Ghi bản ghi lịch sử `CheckInLog` (`SUCCESS` hoặc `DENIED` kèm lý do cụ thể).
- [ ] **Task 2.2.6:** `[Type: BLL / Service]` `[Module: PT Management]` `[UC04, UC08, UC10]` **Phân hệ Huấn luyện viên & Lịch ca dạy (`PtService`):**
  - Quản lý hồ sơ PT, phân công PT cố định cho học viên có gói PT.
  - PT đăng ký khung giờ làm việc rảnh theo tuần (`PTAvailability`).
  - Hội viên tra cứu lịch rảnh và đặt lịch tập (thuật toán chống trùng lịch - Overlapping Check).
  - Hủy ca tập PT: Tính thời gian hủy; nếu $< 2\text{h}$ áp dụng phạt trừ 1 buổi tập; nếu $\ge 2\text{h}$ hoàn lại buổi và mở lại slot rảnh.
  - PT ghi nhận chỉ số thể hình InBody định kỳ cho học viên.
  - Xác nhận hoàn tất ca dạy: Tự động trừ 1 buổi PT trong thẻ và ghi nhận công cho PT.
  - Hội viên chấm điểm sao (1–5 sao) và nhận xét chất lượng ca tập.
- [ ] **Task 2.2.7:** `[Type: BLL / Service]` `[Module: Reports & BI]` `[UC06]` **Phân hệ Báo cáo & Phân tích BI (`ReportService`):**
  - Tổng hợp số liệu đa chiều: Doanh thu thực thu, biến động hội viên mới/tái tục, lưu lượng check-in theo khung giờ, KPI và điểm đánh giá của PT.
  - Đóng gói dữ liệu xuất báo cáo ra định dạng Excel (.xlsx) và PDF.

---

### GIAI ĐOẠN 3: TẦNG API GATEWAY & CONTROLLERS (`GymManagement.Api`)
*Mục tiêu: Xây dựng các REST API endpoint chuẩn `/api/v1/*`, tích hợp Swagger UI, cấu hình JWT Bearer và phân quyền.*

- [ ] **Task 3.1:** `[Type: API / Controller]` `[Module: Infrastructure]` Cấu hình Middleware xử lý lỗi toàn cục (`GlobalExceptionHandlerMiddleware`), trả về định dạng chuẩn `ApiResponse<T>` và `ApiErrorResponse`.
- [ ] **Task 3.2:** `[Type: API / Controller]` `[Module: Auth & RBAC]` Cấu hình xác thực JWT Bearer, CORS policy cho phép Desktop và Web kết nối.
- [ ] **Task 3.3:** `[Type: API / Controller]` `[Module: Infrastructure]` Cấu hình Swagger / OpenAPI hỗ trợ kiểm thử API có đính kèm Bearer Token.
- [ ] **Task 3.4:** `[Type: API / Controller]` `[Module: Auth & RBAC]` `[UC01]` Xây dựng `AuthController` (`POST /api/v1/auth/login`, `POST /api/v1/auth/logout`).
- [ ] **Task 3.5:** `[Type: API / Controller]` `[Module: System Config]` `[UC07]` Xây dựng `AdminUsersController` & `AdminSettingsController` (`/api/v1/admin/*`).
- [ ] **Task 3.6:** `[Type: API / Controller]` `[Module: System Config]` `[UC07]` Xây dựng `PackagesController` (`/api/v1/admin/packages/*`).
- [ ] **Task 3.7:** `[Type: API / Controller]` `[Module: Member & Subscription]` `[UC02, UC09]` Xây dựng `MembersController` (`/api/v1/members/*` và `/api/v1/member/*`).
- [ ] **Task 3.8:** `[Type: API / Controller]` `[Module: Billing & POS]` `[UC05]` Xây dựng `InvoicesController` (`/api/v1/invoices/*`, hỗ trợ xuất PDF).
- [ ] **Task 3.9:** `[Type: API / Controller]` `[Module: Check-In & Access]` `[UC03]` Xây dựng `CheckInController` (`/api/v1/checkin/scan`, `/api/v1/checkin/manual`, `/api/v1/checkin/logs`).
- [ ] **Task 3.10:** `[Type: API / Controller]` `[Module: PT Management]` `[UC04, UC08, UC10]` Xây dựng `PtController` (`/api/v1/pt/*` và `/api/v1/admin/pt/*`).
- [ ] **Task 3.11:** `[Type: API / Controller]` `[Module: Reports & BI]` `[UC06]` Xây dựng `ReportsController` (`/api/v1/reports/dashboard`, `/api/v1/reports/export`).

---

### GIAI ĐOẠN 4: FRONTEND 1 - ỨNG DỤNG WINUI 3 DESKTOP (`GymManagement.Desktop`)
*Mục tiêu: Xây dựng giao diện hiệu năng cao dành riêng cho Quản trị viên (Admin) vận hành tại quầy và quản trị cơ sở theo chuẩn MVVM.*

- [ ] **Task 4.1:** `[Type: Frontend / Desktop]` `[Module: Infrastructure]` Thiết lập kiến trúc MVVM: Cấu hình `App.xaml.cs`, Dependency Injection, `ApiClient` service đóng gói HTTP call.
- [ ] **Task 4.2:** `[Type: Frontend / Desktop]` `[Module: Infrastructure]` Xây dựng `MainWindow.xaml` với `NavigationView` (Shell Navigation) phân chia các menu chức năng.
- [ ] **Task 4.3:** `[Type: Frontend / Desktop]` `[Module: Auth & RBAC]` `[UC01]` **Màn hình Đăng nhập (`LoginPage` / `LoginViewModel`):**
  - Form nhập thông tin đăng nhập, gọi API đăng nhập, lưu trữ token, phân quyền truy cập.
- [ ] **Task 4.4:** `[Type: Frontend / Desktop]` `[Module: Reports & BI]` `[UC06]` **Màn hình Dashboard Tổng quan (`DashboardPage` / `DashboardViewModel`):**
  - Hiển thị các chỉ số nhanh: Doanh thu ngày, số khách đang có mặt, số ca PT hôm nay, biểu đồ tóm tắt.
- [ ] **Task 4.5:** `[Type: Frontend / Desktop]` `[Module: Member & Subscription]` `[UC02]` **Màn hình Quản lý Hội viên & Thẻ tập (`MembersPage` / `MembersViewModel`):**
  - Danh sách hội viên kèm bộ lọc tìm kiếm nhanh (SĐT, Họ tên, Mã thẻ).
  - Wizard biểu mẫu đăng ký hội viên mới 2 bước (Thông tin cá nhân -> Chọn gói & Kích hoạt).
  - Popup thao tác nhanh: Cập nhật hồ sơ, Gia hạn/Nâng cấp thẻ, Bảo lưu thẻ (`FROZEN`), Khóa/Hủy thẻ.
- [ ] **Task 4.6:** `[Type: Frontend / Desktop]` `[Module: Check-In & Access]` `[UC03]` **Màn hình Quầy Điểm danh Check-in (`CheckInPage` / `CheckInViewModel`):**
  - Tích hợp luồng camera webcam thời gian thực, tự động giải mã QR qua ZXing.Net.
  - Ô nhập SĐT / Mã thẻ thủ công phòng trường hợp điện thoại khách hết pin.
  - Hiển thị kết quả điểm danh trực quan (Ảnh nhận diện, Họ tên, Gói tập, Số ngày còn lại) kèm âm thanh thành công/thất bại.
  - Bảng giám sát lịch sử ra/vào cơ sở theo ca làm việc trong ngày.
- [ ] **Task 4.7:** `[Type: Frontend / Desktop]` `[Module: Billing & POS]` `[UC05]` **Màn hình Thu ngân POS & Hóa đơn (`BillingPage` / `BillingViewModel`):**
  - Giao diện lập hóa đơn POS tại quầy: Chọn hội viên, thêm dịch vụ, tính thuế và ưu đãi.
  - Hiển thị mã thanh toán VietQR động để khách quét trực tiếp từ màn hình phụ.
  - Nút xác nhận thanh toán, xử lý hoàn tiền hóa đơn và lệnh in phiếu thu nhiệt K80.
- [ ] **Task 4.8:** `[Type: Frontend / Desktop]` `[Module: PT Management]` `[UC04]` **Màn hình Điều phối Huấn luyện viên (`TrainersPage` & `PtSchedulePage`):**
  - Quản lý danh sách hồ sơ PT, chuyên môn và mức lương.
  - Giao diện phân công PT phụ trách cho hội viên mới mua gói PT.
  - Lịch điều phối trực quan (Calendar View) theo dõi các ca dạy, hỗ trợ xếp lịch và xác nhận hoàn tất ca tập.
- [ ] **Task 4.9:** `[Type: Frontend / Desktop]` `[Module: System Config]` `[UC07]` **Màn hình Danh mục Gói tập & Cấu hình (`SettingsPage` / `SettingsViewModel`):**
  - Quản lý danh mục gói tập (Thêm, sửa, đổi trạng thái sang ngừng kinh doanh).
  - Biểu mẫu điều chỉnh tham số vận hành (Giờ mở/đóng cửa, thời gian hủy ca tối thiểu, số ngày bảo lưu tối đa).
  - Quản trị tài khoản nhân sự và phân quyền.
- [ ] **Task 4.10:** `[Type: Frontend / Desktop]` `[Module: Reports & BI]` `[UC06]` **Màn hình Báo cáo BI & Thống kê (`ReportsPage` / `ReportsViewModel`):**
  - Bộ lọc thời gian (Hôm nay, Tuần này, Tháng này, Tùy chọn).
  - Biểu đồ doanh thu, biểu đồ mật độ check-in theo giờ, thống kê hội viên mới/tái tục.
  - Nút xuất file báo cáo ra định dạng Excel (.xlsx) và PDF.

---

### GIAI ĐOẠN 5: FRONTEND 2 - CỔNG WEB TRỰC TUYẾN BLAZOR (`GymManagement.Web`)
*Mục tiêu: Xây dựng cổng web Responsive dành cho Hội viên (Member) và Huấn luyện viên (PT).*

- [ ] **Task 5.1:** `[Type: Frontend / Web]` `[Module: Infrastructure]` Thiết lập kiến trúc Blazor Server: `MainLayout.razor`, `NavMenu.razor`, Cấu hình `AuthenticationStateProvider` quản lý JWT Token trên trình duyệt.
- [ ] **Task 5.2:** `[Type: Frontend / Web]` `[Module: Auth & RBAC]` `[UC01]` Xây dựng màn hình Đăng nhập Web và chuyển hướng vai trò tự động (`/login`).
- [ ] **Task 5.3: Phân hệ Cổng Hội viên (Member Portal):**
  - [ ] **Task 5.3.1:** `[Type: Frontend / Web]` `[Module: Member & Subscription]` `[UC09]` `Home.razor` (`/`): Tổng quan thông tin thẻ, lịch tập sắp tới, thông báo nhắc nhở.
  - [ ] **Task 5.3.2:** `[Type: Frontend / Web]` `[Module: Member & Subscription]` `[UC09]` `DigitalCard.razor` (`/digital-card`): Thẻ tập điện tử hiển thị mã QR check-in cá nhân, ngày hết hạn và số buổi PT còn lại.
  - [ ] **Task 5.3.3:** `[Type: Frontend / Web]` `[Module: PT Management]` `[UC10]` `PtBooking.razor` (`/pt-booking`): Giao diện tra cứu lịch rảnh của PT phụ trách và đặt lịch ca tập mới.
  - [ ] **Task 5.3.4:** `[Type: Frontend / Web]` `[Module: PT Management]` `[UC10]` `MySchedule.razor` (`/schedule`): Danh sách lịch tập sắp tới, nút Đổi lịch / Hủy lịch hẹn (kèm cảnh báo phạt nếu hủy sát giờ $< 2\text{h}$).
  - [ ] **Task 5.3.5:** `[Type: Frontend / Web]` `[Module: PT Management]` `[UC09]` `InBodyProgress.razor` (`/inbody`): Biểu đồ theo dõi các chỉ số thể hình định kỳ (Cân nặng, % Mỡ, Khối lượng cơ).
  - [ ] **Task 5.3.6:** `[Type: Frontend / Web]` `[Module: Member & Subscription]` `[UC09]` `Profile.razor` (`/profile`): Xem và chỉnh sửa thông tin liên hệ, đổi mật khẩu.
  - [ ] **Task 5.3.7:** `[Type: Frontend / Web]` `[Module: PT Management]` `[UC10]` `ReviewModal.razor`: Popup chấm điểm (1–5 sao) và nhận xét chất lượng ca tập sau khi hoàn tất.
- [ ] **Task 5.4: Phân hệ Cổng Huấn luyện viên (PT Portal):**
  - [ ] **Task 5.4.1:** `[Type: Frontend / Web]` `[Module: PT Management]` `[UC08]` `PtHome.razor` (`/pt`): Bảng tin tổng quan của PT, số ca dạy trong tuần, danh sách học viên phụ trách.
  - [ ] **Task 5.4.2:** `[Type: Frontend / Web]` `[Module: PT Management]` `[UC08]` `PtSchedule.razor` (`/pt/schedule`): Lưới thời khóa biểu tuần, tính năng tích chọn đăng ký các khung giờ làm việc rảnh.
  - [ ] **Task 5.4.3:** `[Type: Frontend / Web]` `[Module: PT Management]` `[UC08]` `PtStudents.razor` (`/pt/students`): Danh sách học viên phụ trách, form nhập số đo InBody định kỳ.
  - [ ] **Task 5.4.4:** `[Type: Frontend / Web]` `[Module: PT Management]` `[UC08]` Nút bấm xác nhận hoàn tất ca dạy trực tiếp trên điện thoại để trừ buổi học viên và ghi nhận KPI.
  - [ ] **Task 5.4.5:** `[Type: Frontend / Web]` `[Module: PT Management]` `[UC08]` `PtProfile.razor` (`/pt/profile`): Xem thông tin cá nhân và điểm đánh giá sao trung bình của học viên.

---

### GIAI ĐOẠN 6: KIỂM THỬ TÍCH HỢP, BẢO MẬT & NGHIỆM THU (TESTING & VERIFICATION)
*Mục tiêu: Kiểm thử toàn diện sự phối hợp giữa 3 tầng kiến trúc và xác thực quy tắc nghiệp vụ.*

- [ ] **Task 6.1:** `[Type: Testing]` `[Module: BLL]` **Kiểm thử Đơn vị Nghiệp vụ (Unit Testing - BLL):**
  - Test thuật toán kiểm tra chống trùng lịch dạy PT (Overlapping Check).
  - Test quy tắc phạt hủy ca PT: Hủy trước 2h được bảo lưu buổi, hủy sát giờ $< 2\text{h}$ bị trừ buổi.
  - Test quy tắc tính ngày hết hạn khi gia hạn thẻ và bảo lưu thẻ (`FROZEN`).
  - Test quy tắc kiểm soát check-in: Từ chối thẻ hết hạn, hết buổi, bị khóa, sai khung giờ.
- [ ] **Task 6.2:** `[Type: Testing]` `[Module: Infrastructure]` **Kiểm thử Tích hợp Giao dịch (Integration Testing - ACID):**
  - Test giao dịch Lập hóa đơn + Kích hoạt thẻ: Đảm bảo Rollback toàn bộ nếu tạo thẻ thất bại.
  - Test giao dịch Check-in: Đảm bảo trừ số buổi và ghi log điểm danh diễn ra trong 1 transaction duy nhất.
- [ ] **Task 6.3:** `[Type: Testing]` `[Module: Auth & RBAC]` **Kiểm thử Bảo mật & Phân quyền (RBAC Testing):**
  - Đảm bảo Hội viên không thể gọi các API thuộc quyền Admin hoặc PT.
  - Đảm bảo PT chỉ thao tác được trên lịch dạy và học viên của chính mình.
  - Đảm bảo tài khoản bị khóa (`LOCKED`) không thể gọi bất kỳ API nào sau khi bị khóa.
- [ ] **Task 6.4:** `[Type: Testing]` `[Module: Adapter]` **Kiểm thử Tích hợp Ngoại vi (External Integration Testing):**
  - Test sinh mã VietQR chuẩn với cổng thanh toán ngân hàng thực tế.
  - Test camera quét mã QR với các điều kiện ánh sáng và góc quét khác nhau.
  - Test gửi email hóa đơn điện tử tự động.
- [ ] **Task 6.5:** `[Type: Testing]` `[Module: Integration]` **Kiểm thử Luồng Vận hành Xuyên suốt (End-to-End E2E Walkthrough):**
  - Luồng 1: Admin tạo gói tập $\rightarrow$ Đăng ký hội viên mới $\rightarrow$ Thanh toán VietQR $\rightarrow$ Kích hoạt thẻ và sinh mã QR.
  - Luồng 2: Hội viên dùng điện thoại mở thẻ QR $\rightarrow$ Admin quét webcam check-in $\rightarrow$ Hệ thống tự trừ buổi và mở cửa.
  - Luồng 3: Hội viên đặt lịch tập PT trên Web $\rightarrow$ PT xem lịch $\rightarrow$ Hoàn tất ca tập, PT nhập InBody $\rightarrow$ Hội viên đánh giá 5 sao.
  - Luồng 4: Admin vào màn hình Reports $\rightarrow$ Xem Dashboard đa chiều $\rightarrow$ Xuất file báo cáo Excel/PDF.
