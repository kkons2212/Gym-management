# HƯỚNG DẪN THIẾT LẬP DOCKER SQL SERVER CHO DỰ ÁN GYM MANAGEMENT
**Tài liệu tham chiếu:** [`Document/dev_rule.md`](dev_rule.md) | [`Document/dev_mapping.md`](dev_mapping.md) | [`Document/init_docker_db.sql`](init_docker_db.sql)

---

## 1. TỔNG QUAN

Tài liệu này cung cấp hướng dẫn từng bước để cài đặt, khởi chạy và quản trị container Docker Microsoft SQL Server 2022 phục vụ cho toàn bộ hệ thống Quản lý Phòng tập Thể thao (Gym Management System).

### Thông tin cấu hình chuẩn của hệ thống:
* **Hệ quản trị CSDL:** Microsoft SQL Server 2022 (Developer Edition)
* **Docker Image:** `mcr.microsoft.com/mssql/server:2022-latest`
* **Tên Container:** `mssql`
* **Cổng dịch vụ (Port):** `1433`
* **Tên Cơ sở dữ liệu:** `GymManagementDb`
* **Tài khoản Quản trị:** `sa`
* **Mật khẩu SA mặc định:** `SqlServer@2026`
* **Mã hóa chuỗi:** Hỗ trợ đầy đủ Unicode tiếng Việt có dấu (`NVARCHAR`)

---

## 2. YÊU CẦU MÔI TRƯỜNG & CHUẨN BỊ

Trước khi tiến hành, đảm bảo máy tính đã cài đặt:
1. **Docker Desktop** (trên Windows, macOS hoặc Linux) đã khởi động và đang chạy ở chế độ Linux containers.
2. **Tài nguyên tối thiểu cấp cho Docker:**
   - RAM: Tối thiểu 2 GB (khuyến nghị 4 GB trở lên để SQL Server hoạt động mượt mà).
   - Ổ đĩa trống: Tối thiểu 4 GB.
3. **Thư mục làm việc trên máy host:**
   - Thư mục gắn ổ đĩa chia sẻ: `D:\ws` (nếu máy bạn chưa có ổ `D:`, bạn có thể tạo thư mục tương ứng hoặc điều chỉnh đường dẫn mount).

---

## 3. CÁC BƯỚC KHỞI CHẠY CONTAINER

### Bước 1: Tải Docker Image chính thức từ Microsoft
Mở Terminal (PowerShell hoặc Command Prompt) và chạy lệnh sau để tải image SQL Server 2022 bản mới nhất:

```bash
docker pull mcr.microsoft.com/mssql/server:2022-latest
```

---

### Bước 2: Khởi chạy Container SQL Server
Thực thi lệnh `docker run` với các tham số cấu hình chuẩn xác của dự án:

```bash
docker run -d --name mssql \
  -e "ACCEPT_EULA=Y" \
  -v "D:\ws:/ws" \
  -v mssql_data:/var/opt/mssql \
  -e "MSSQL_SA_PASSWORD=SqlServer@2026" \
  -e "MSSQL_PID=Developer" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest
```

#### Giải thích chi tiết các tham số lệnh:
* `-d`: Chạy container ở chế độ nền (Detached mode).
* `--name mssql`: Đặt tên cố định cho container là `mssql` để tiện quản lý và gọi trong các script tự động.
* `-e "ACCEPT_EULA=Y"`: Đồng ý với điều khoản sử dụng bản quyền của Microsoft (bắt buộc).
* `-v "D:\ws:/ws"`: Gắn thư mục `D:\ws` trên máy host vào thư mục `/ws` trong container để dễ dàng trao đổi file sao lưu (`.bak`), file kịch bản SQL (`.sql`).
* `-v mssql_data:/var/opt/mssql`: Docker Named Volume giúp lưu trữ bền vững dữ liệu CSDL (`.mdf`, `.ldf`). Khi dừng hoặc xóa container, toàn bộ dữ liệu CSDL vẫn được bảo toàn.
* `-e "MSSQL_SA_PASSWORD=SqlServer@2026"`: Thiết lập mật khẩu cho tài khoản `sa` của dự án.
* `-e "MSSQL_PID=Developer"`: Chọn phiên bản Developer (đầy đủ mọi tính năng cao cấp cấp doanh nghiệp của SQL Server, miễn phí cho môi trường dev/test).
* `-p 1433:1433`: Ánh xạ cổng tiêu chuẩn 1433 của máy host vào cổng 1433 của container.
* `mcr.microsoft.com/mssql/server:2022-latest`: Tên image Docker nguồn từ Microsoft Container Registry.

---

### Bước 3: Khởi tạo CSDL GymManagementDb và Nạp Dữ Liệu Mẫu

Dự án đã cung cấp sẵn script DDL 9 bảng vật lý và dữ liệu hạt giống (Seed Data) tại [`Document/init_docker_db.sql`](init_docker_db.sql).

Bạn có thể chọn 1 trong 2 cách sau để khởi tạo:

#### Cách 1: Sử dụng PowerShell Script tự động (Khuyến nghị)
Chạy script PowerShell có sẵn trong thư mục `scripts/`:

```powershell
.\scripts\setup-docker-db.ps1 -ContainerName "mssql" -Password "SqlServer@2026"
```

#### Cách 2: Thực thi thủ công qua lệnh Docker & sqlcmd
Nếu muốn chạy trực tiếp bằng dòng lệnh:

```bash
# 1. Sao chép file SQL vào trong container
docker cp Document/init_docker_db.sql mssql:/tmp/init_docker_db.sql

# 2. Thực thi script khởi tạo qua sqlcmd
docker exec -it mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "SqlServer@2026" -C -i /tmp/init_docker_db.sql
```

---

## 4. XÁC MINH VÀ KIỂM TRA TRẠNG THÁI CSDL

### 1. Kiểm tra trạng thái hoạt động của Container
```bash
docker ps --filter name=mssql
```
*Kết quả mong muốn: Container có trạng thái `Up ... (healthy)` hoặc `Up ...`.*

### 2. Kiểm tra log khởi động của SQL Server
```bash
docker logs mssql --tail 20
```
*Kết quả mong muốn: Dòng thông báo `SQL Server is now ready for client connections`.*

### 3. Kiểm tra dữ liệu tiếng Việt có dấu (Unicode) trong 9 bảng
Thực hiện truy vấn mẫu kiểm tra dữ liệu hạt giống:

```bash
docker exec -it mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "SqlServer@2026" -C -d GymManagementDb -Q "SELECT RoleID, RoleName FROM dbo.Role; SELECT PlanId, Name FROM dbo.PackagePlan; SELECT UserName, FullName FROM dbo.[User];"
```

*Kết quả mong muốn: Hiển thị đầy đủ tiếng Việt có dấu (`Quản trị viên`, `Gói 1 Tháng Tiêu Chuẩn`, `Quản Trị Hệ Thống`), không bị lỗi font hoặc dấu hỏi `?`.*

---

## 5. THÔNG TIN KẾT NỐI VÀO ỨNG DỤNG

### 1. Chuỗi kết nối (Connection String) chuẩn
```text
Server=localhost,1433;Database=GymManagementDb;User Id=sa;Password=SqlServer@2026;TrustServerCertificate=True;MultipleActiveResultSets=true
```

### 2. Cấu hình trong Backend Server (`GymManagement.Server.Api`)
Chuỗi kết nối trên đã được cấu hình sẵn trong:
* [`src/Server/GymManagement.Server.Api/appsettings.json`](file:///c:/Users/nguye/Desktop/Gym-management/src/Server/GymManagement.Server.Api/appsettings.json)
* [`src/Server/GymManagement.Server.Api/appsettings.Development.json`](file:///c:/Users/nguye/Desktop/Gym-management/src/Server/GymManagement.Server.Api/appsettings.Development.json)

Hoặc cấu hình qua biến môi trường (Ví dụ trong file `.env`):
```env
ConnectionStrings__DefaultConnection="Server=localhost,1433;Database=GymManagementDb;User Id=sa;Password=SqlServer@2026;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

### 3. Tài khoản đăng nhập kiểm thử có sẵn
* **Tài khoản Admin:**
  - Tên đăng nhập: `admin`
  - Mật khẩu: `admin123`
* **Tài khoản Huấn luyện viên (PT):**
  - Tên đăng nhập: `pt01`
  - Mật khẩu: `admin123`
* **Tài khoản Hội viên (Member):**
  - Tên đăng nhập: `member01`
  - Mật khẩu: `admin123`

---

## 6. CÁC LỆNH QUẢN TRỊ CONTAINER THƯỜNG DÙNG

| Thao tác | Câu lệnh Docker |
| :--- | :--- |
| **Dừng container** | `docker stop mssql` |
| **Khởi động lại container** | `docker start mssql` |
| **Khởi động lại khi lỗi** | `docker restart mssql` |
| **Xem tài nguyên (RAM, CPU)** | `docker stats mssql --no-stream` |
| **Tự động khởi động cùng Docker** | `docker update --restart unless-stopped mssql` |
| **Xem log trực tiếp** | `docker logs -f mssql` |
| **Xóa container (Dữ liệu vẫn giữ trong volume)** | `docker rm -f mssql` |
| **Tạo bản sao lưu CSDL (.bak)** | `docker exec -it mssql /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "SqlServer@2026" -C -Q "BACKUP DATABASE GymManagementDb TO DISK = '/ws/GymManagementDb.bak'"` |

---

## 7. XỬ LÝ SỰ CỐ THƯỜNG GẶP (TROUBLESHOOTING)

### 1. Lỗi `Login failed for user 'sa' (Error Number: 18456)`
* **Nguyên nhân:** Mật khẩu trong chuỗi kết nối của `appsettings.json` không khớp với mật khẩu `MSSQL_SA_PASSWORD` lúc khởi tạo container.
* **Khắc phục:** Đảm bảo cả hai đều là `SqlServer@2026`. Nếu container cũ dùng mật khẩu khác, hãy dừng và tạo lại container hoặc đổi mật khẩu SA qua lệnh `ALTER LOGIN sa WITH PASSWORD = 'SqlServer@2026'`.

### 2. Lỗi `Port 1433 is already in use`
* **Nguyên nhân:** Cổng 1433 trên máy host đang bị chiếm dụng bởi một dịch vụ SQL Server Local cài trực tiếp trên Windows hoặc một container khác.
* **Khắc phục:**
  - Kiểm tra tiến trình đang chiếm cổng: `netstat -ano | findstr :1433`
  - Tắt dịch vụ SQL Server trên máy host qua `services.msc` (tìm `SQL Server (MSSQLSERVER)` và bấm Stop).

### 3. Lỗi container tự thoát ngay sau khi chạy (Exit code 1)
* **Nguyên nhân:** Thiếu tham số `-e "ACCEPT_EULA=Y"`, mật khẩu SA không đủ độ phức tạp (yêu cầu chữ hoa, chữ thường, số và ký tự đặc biệt), hoặc Docker không đủ RAM.
* **Khắc phục:** Kiểm tra log bằng lệnh `docker logs mssql` để xem chi tiết lý do thoát và cấp thêm RAM cho Docker Desktop trong phần Settings > Resources.
