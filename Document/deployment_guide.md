# HƯỚNG DẪN TRIỂN KHAI HỆ THỐNG GYM MANAGEMENT (DEPLOYMENT GUIDE)

> **Tài liệu tham chiếu:** [`Document/dev_rule.md`](dev_rule.md) | [`Document/database_setup.md`](database_setup.md) | [`.github/workflows/cd.yml`](../.github/workflows/cd.yml)

---

## 1. TỔNG QUAN HỆ THỐNG PHÂN PHỐI & TRIỂN KHAI (CI/CD)

Hệ thống Gym Management áp dụng mô hình phân phối & triển khai tự động hóa kép:
1. **Dịch vụ Server & Web (Backend + Frontend Web):**
   - Đóng gói dưới dạng Docker Container đa tầng (Multi-stage build).
   - Tự động biên dịch, gắn tag và phát hành lên **GitHub Container Registry (GHCR)**.
   - Triển khai trên máy chủ thông qua **Docker Compose** kết nối SQL Server 2022.
2. **Ứng dụng Desktop Lễ tân (Admin WinUI 3):**
   - Tự động biên dịch ở chế độ Release trên Windows Runner.
   - Đóng gói file `.zip` hoàn chỉnh kèm tất cả dependencies runtime.
   - Tự động đính kèm vào mục **GitHub Releases** khi phát hành phiên bản mới (Git Tag `v*.*.*`).

---

## 2. QUY TRÌNH KÍCH HOẠT CONTINUOUS DEPLOYMENT (CD)

### Cách 1: Phát hành phiên bản chính thức (Production Release)
Khi hoàn tất một cột mốc hoặc tính năng mới, tạo Git Tag theo định dạng chuẩn ngữ nghĩa (`vX.Y.Z`) và push lên GitHub:
```bash
git checkout main
git pull origin main
git tag v1.0.0
git push origin v1.0.0
```
* **Hành động tự động của Pipeline CD:**
  1. Build Docker image `server-api` với tag `:v1.0.0` và `:latest`.
  2. Build Docker image `web-portal` với tag `:v1.0.0` và `:latest`.
  3. Đóng gói ứng dụng Desktop thành file `GymManagement-Desktop-v1.0.0-win-x64.zip`.
  4. Tạo trang **GitHub Release v1.0.0**, tạo Release Notes tự động và đính kèm file zip cài đặt Desktop.

### Cách 2: Tự động khi Merge vào nhánh `main`
Khi bất kỳ Pull Request nào được merge vào nhánh `main`:
* Docker image `server-api` và `web-portal` được cập nhật tag `:latest` và `:sha-<commit_hash>`.
* Bản build Desktop mới nhất được upload dưới dạng Artifact của workflow để kiểm thử nội bộ.

### Cách 3: Kích hoạt thủ công (Manual Trigger via GitHub Actions UI)
1. Truy cập tab **Actions** trên repository GitHub.
2. Chọn workflow **Continuous Deployment & Delivery (CD)** ở cột bên trái.
3. Bấm nút **Run workflow**, chọn nhánh và môi trường (`production` hoặc `staging`).

---

## 3. HƯỚNG DẪN TRIỂN KHAI TRÊN MÁY CHỦ (SERVER / VPS)

### Bước 1: Chuẩn bị môi trường máy chủ
Máy chủ (Ubuntu 22.04 LTS / Debian / Windows Server) cần cài đặt sẵn:
* **Docker Engine** (phiên bản 24.0 trở lên)
* **Docker Compose** (phiên bản v2 trở lên)

### Bước 2: Tải file cấu hình triển khai
Trên máy chủ, chỉ cần tải về 3 file cấu hình cốt lõi (không cần clone toàn bộ mã nguồn):
* `docker-compose.prod.yml`
* `.env.example`
* `scripts/deploy.sh` (hoặc `scripts/deploy.ps1` trên Windows)

### Bước 3: Cấu hình biến môi trường
Sao chép file mẫu `.env.example` thành `.env`:
```bash
cp .env.example .env
```
Mở file `.env` và điều chỉnh các thông số bảo mật:
```ini
# Mật khẩu quản trị CSDL SQL Server (cần đổi sang mật khẩu mạnh thực tế)
MSSQL_SA_PASSWORD=SqlServer@2026_SecureProduction!
MSSQL_PORT=1433

# Khóa bí mật JWT (tối thiểu 32 ký tự ngẫu nhiên)
JWT_SIGNING_KEY=GymManagement_Super_Secret_Jwt_Signing_Key_2026_Secure!

# Cổng truy cập ngoài Internet / Mạng nội bộ
API_PORT=5000
WEB_PORT=5001
```

### Bước 4: Đăng nhập GitHub Container Registry (Nếu repo là Private)
Nếu repository ở chế độ Private, tạo một GitHub Personal Access Token (PAT) có quyền `read:packages` và đăng nhập trên máy chủ:
```bash
echo "YOUR_GITHUB_PAT" | docker login ghcr.io -u YOUR_GITHUB_USERNAME --password-stdin
```

### Bước 5: Chạy lệnh triển khai 1-Click
* **Trên Linux / macOS:**
  ```bash
  chmod +x scripts/deploy.sh
  ./scripts/deploy.sh
  ```
* **Trên Windows PowerShell:**
  ```powershell
  .\scripts\deploy.ps1
  ```
* **Hoặc bằng lệnh Docker Compose thuần:**
  ```bash
  docker compose -f docker-compose.prod.yml pull
  docker compose -f docker-compose.prod.yml up -d
  ```

---

## 4. HƯỚNG DẪN CÀI ĐẶT ỨNG DỤNG DESKTOP CHO NHÂN VIÊN LỄ TÂN & QUẢN TRỊ

Truy cập mục **Releases** trên GitHub repository: `https://github.com/kkons2212/Gym-management/releases` (hoặc mục **Artifacts** trong tab Actions đối với các bản build nội bộ). Hệ thống cung cấp sẵn 2 định dạng:

### Cách 1: Sử dụng Bộ cài đặt tự động (Khuyến nghị cho Lễ tân)
1. Tải về file cài đặt: **`GymManagement-Setup-vX.Y.Z-win-x64.exe`**.
2. Nháy đúp chuột vào file `.exe` vừa tải về để khởi động trình cài đặt.
3. Bấm **Next** $\rightarrow$ chọn tùy chọn tạo lối tắt trên màn hình (**Create a desktop shortcut**) $\rightarrow$ **Install**.
4. Trình cài đặt sẽ tự động:
   - Cài đặt toàn bộ phần mềm và dependencies vào máy.
   - Tạo biểu tượng lối tắt ngoài màn hình Desktop và trong Start Menu.
   - Tích hợp mục gỡ cài đặt sạch sẽ trong *Windows Settings > Installed apps*.
5. Khởi chạy phần mềm ngay từ biểu tượng ngoài Desktop.

### Cách 2: Sử dụng Bản Portable nén (.ZIP) (Dành cho Kỹ thuật viên)
1. Tải về file nén: **`GymManagement-Desktop-vX.Y.Z-win-x64.zip`**.
2. Giải nén vào thư mục tùy ý trên máy tính Windows.
3. Khởi chạy trực tiếp file thực thi `GymManagement.Desktop.exe` mà không cần cài đặt.
4. Trong trường hợp cần trỏ tới máy chủ API khác, mở file `appsettings.json` trong thư mục giải nén và điều chỉnh giá trị `ApiBaseUrl` tương ứng (mặc định: `http://localhost:5000`).

---

## 5. BẢO TRÌ & NÂNG CẤP PHIÊN BẢN (ZERO-DOWNTIME UPDATE)

Khi có bản cập nhật mới trên GitHub, thực hiện lệnh sau trên máy chủ để nâng cấp hệ thống mà không làm mất dữ liệu:
```bash
./scripts/deploy.sh
```
Lệnh trên sẽ:
1. Kéo image mới nhất từ GHCR về máy chủ.
2. Khởi tạo container mới và thay thế container cũ.
3. Dữ liệu CSDL SQL Server được lưu trong Docker Named Volume `gym_mssql_prod_data` nên được **bảo toàn nguyên vẹn 100%**.
