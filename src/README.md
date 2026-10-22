# 🏋️ GYM HOUSE – Hệ thống Quản lý Phòng Tập

> Khung giao diện (UI Skeleton) chuẩn kiến trúc 3 tầng cho hệ thống quản lý phòng tập thể thao  
> **Admin/Staff Portal** → WinUI 3 (Desktop) | **Member Portal** → Blazor Server (Web/Mobile)

---

## 📁 Cấu trúc thư mục

```
src/
├── GymManagement.slnx                          ← Solution file
├── Presentation/                               ← Tầng 1: Giao diện
│   ├── GymManagement.Desktop/                  ← WinUI 3 (Admin)
│   │   ├── Views/          ← 7 trang XAML
│   │   └── ViewModels/     ← 7 ViewModel (MVVM + CommunityToolkit)
│   └── GymManagement.Web/                      ← Blazor Server (Member)
│       ├── Components/
│       │   ├── Layout/     ← Shell + Bottom Navigation
│       │   └── Pages/      ← 5 trang Razor
│       └── wwwroot/app.css ← Design system (dark theme)
├── BusinessLogic/                              ← Tầng 2: Logic nghiệp vụ
│   └── GymManagement.Core/
│       ├── DTOs/           ← Data Transfer Objects
│       ├── Contracts/      ← Service Interfaces
│       ├── Enums/          ← Enumerations
│       └── Services/       ← MockGymService (thay bằng EF Core sau)
└── DataAccess/                                 ← Tầng 3: Truy cập dữ liệu
    └── GymManagement.Infrastructure/           ← Placeholder (EF Core sau)
```

---

## 🚀 Hướng dẫn chạy

### Yêu cầu hệ thống

| Thành phần | Phiên bản |
|-----------|-----------|
| .NET SDK | 8.0+ |
| Windows App SDK | 1.7+ (chỉ Desktop) |
| OS | Windows 10 1903+ |

### Cài đặt một lần

```powershell
# Restore toàn bộ NuGet packages
dotnet restore .\src\GymManagement.slnx
```

---

### 🌐 Blazor Member Portal (Web/Mobile)

#### Chạy local (trình duyệt máy tính)

```powershell
dotnet run --project .\src\Presentation\GymManagement.Web\GymManagement.Web.csproj
```

Mở trình duyệt: **http://localhost:5220**

#### Chạy + Port ra điện thoại (qua Cloudflare Tunnel)

**Bước 1** – Chạy Blazor với profile `tunnel` (bind `0.0.0.0`):

```powershell
dotnet run --project .\src\Presentation\GymManagement.Web\GymManagement.Web.csproj --launch-profile tunnel
```

**Bước 2** – Mở terminal mới, chạy cloudflared:

```powershell
.\cloudflared.exe tunnel --url http://localhost:5220
```

**Bước 3** – Copy URL `trycloudflare.com` xuất hiện trong output, mở trên điện thoại:

```
INF | https://xxxx-xxxx-xxxx.trycloudflare.com |
```

> ⚠️ URL thay đổi mỗi lần restart cloudflared (Quick Tunnel tạm thời).  
> Để có URL cố định, đăng ký [Cloudflare Zero Trust](https://one.dash.cloudflare.com) miễn phí.

---

### 🖥️ WinUI 3 Desktop (Admin/Staff)

```powershell
# Build
dotnet build .\src\Presentation\GymManagement.Desktop\GymManagement.Desktop.csproj

# Chạy (cần Windows App SDK đã cài)
dotnet run --project .\src\Presentation\GymManagement.Desktop\GymManagement.Desktop.csproj
```

---

### 🔨 Build toàn bộ Solution

```powershell
dotnet build .\src\GymManagement.slnx
```

---

## 📱 Trang Blazor Member Portal

| Route | Trang | UC tương ứng |
|-------|-------|-------------|
| `/` | 🏠 Dashboard / Trang chủ | UC02, UC03, UC05 |
| `/digital-card` | 🪪 Gói tập số (QR check-in) | UC03, UC06 |
| `/pt-booking` | 🏋️ Đặt lịch PT (3 bước) | UC10 |
| `/schedule` | 🗓️ Lịch tập của tôi | UC10 |
| `/inbody` | 📊 Chỉ số InBody | UC11 |
| `/profile` | 👤 Hồ sơ cá nhân | UC01, UC09 |
| `/pt/schedule` | 🧑‍🏫 PT – Lịch dạy theo tuần & Đăng ký lịch rảnh | UC 8.1.1, UC 8.1.2 |
| `/pt/students` | 👥 PT – Học viên phụ trách (InBody, xác nhận ca tập) | UC 8.2.1, UC 8.2.2, UC 8.2.3 |

---

## 🖥️ Trang WinUI 3 Desktop (Admin)

| Trang | UC tương ứng |
|-------|-------------|
| Dashboard | Báo cáo tổng quan |
| Members | UC02 – Quản lý hội viên |
| Check-In | UC03 – Check-in QR |
| PT Schedule | UC04/10 – Lịch PT |
| Billing | UC05 – Thu tiền/hóa đơn |
| Settings | UC07 – Cài đặt hệ thống |

---

## 🏗️ Kiến trúc

```
┌────────────────────────────────────────────────────┐
│                  Presentation Layer                 │
│   ┌──────────────┐        ┌──────────────────────┐ │
│   │  WinUI 3     │        │   Blazor Server      │ │
│   │  (Admin)     │        │   (Member Portal)    │ │
│   └──────┬───────┘        └──────────┬───────────┘ │
└──────────┼────────────────────────────┼─────────────┘
           │                            │
┌──────────▼────────────────────────────▼─────────────┐
│              Business Logic Layer                    │
│          GymManagement.Core                          │
│   DTOs · Contracts (Interfaces) · MockService        │
└──────────────────────────┬──────────────────────────┘
                           │
┌──────────────────────────▼──────────────────────────┐
│              Data Access Layer                       │
│          GymManagement.Infrastructure                │
│          [Placeholder → EF Core + SQL Server]        │
└─────────────────────────────────────────────────────┘
```

---

## 🔜 Phase 2 – Các bước tiếp theo

- [ ] **Login** – Màn hình đăng nhập (UC01) cho cả Desktop và Web
- [ ] **EF Core** – Thay `MockGymService` bằng SQL Server (Infrastructure layer)
- [ ] **Authentication** – JWT / Cookie auth cho Blazor
- [ ] **QR thực** – Tích hợp `ZXing.Net.Blazor` thay QR giả SVG
- [ ] **Notifications** – Trang `/notifications` Blazor
- [ ] **WinUI 3 partial props** – Chuyển `[ObservableProperty]` sang `partial property` (fix MVVMTK0045)
- [ ] **Cloudflare Tunnel cố định** – Đăng ký Named Tunnel thay Quick Tunnel

---

## 📝 Ghi chú kỹ thuật

### Cách mở rộng (thêm logic xử lý)

1. **Thêm method vào interface** trong `BusinessLogic/GymManagement.Core/Contracts/GymContracts.cs`
2. **Implement trong MockGymService** (hoặc Infrastructure Service sau này)
3. **Gọi từ ViewModel/Razor page** – chỉ inject interface, không phụ thuộc implementation

### Liên kết tài liệu

- [User Stories chi tiết](./Document/User_Stories_Chi_Tiet.md)
- [Tổng quan hệ thống](./Document/Home.md)
