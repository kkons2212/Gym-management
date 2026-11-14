# HƯỚNG DẪN THIẾT LẬP DOCKER SQL SERVER CHO DỰ ÁN GYM MANAGEMENT

> 📄 **Tài liệu đầy đủ và cập nhật chi tiết:** Vui lòng xem tại [`Document/database_setup.md`](Document/database_setup.md).

---

## TÓM TẮT CÂU LỆNH KHỞI TẠO NHANH

### 1. Tải Docker Image chính thức
```bash
docker pull mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Khởi chạy Docker Container SQL Server
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

### 3. Nạp 9 Bảng CSDL & Dữ Liệu Mẫu (Seed Data)
```powershell
.\scripts\setup-docker-db.ps1 -ContainerName "mssql" -Password "SqlServer@2026"
```

### 4. Chuỗi kết nối ứng dụng (Connection String)
```text
Server=localhost,1433;Database=GymManagementDb;User Id=sa;Password=SqlServer@2026;TrustServerCertificate=True;MultipleActiveResultSets=true
```

Xem hướng dẫn chi tiết về cấu hình, tài khoản mẫu và xử lý lỗi tại: [`Document/database_setup.md`](Document/database_setup.md).
