# ===================================================================
# SCRIPT TRIỂN KHAI 1-CLICK TỰ ĐỘNG CHO GYM MANAGEMENT (POWERSHELL)
# ===================================================================

$ErrorActionPreference = "Stop"

Write-Host "🚀 [Gym Management] Bắt đầu quy trình triển khai Production..." -ForegroundColor Cyan

# 1. Kiểm tra file cấu hình môi trường .env
if (-not (Test-Path ".env")) {
    if (Test-Path ".env.example") {
        Write-Host "⚠️ Chưa tìm thấy file .env, tự động sao chép từ .env.example..." -ForegroundColor Yellow
        Copy-Item ".env.example" ".env"
        Write-Host "ℹ️ Vui lòng kiểm tra lại mật khẩu và cấu hình trong file .env nếu cần." -ForegroundColor Gray
    } else {
        Write-Error "❌ Không tìm thấy file .env hoặc .env.example! Hủy triển khai."
        exit 1
    }
}

# 2. Pull các container image mới nhất từ GitHub Container Registry
Write-Host "📥 Kéo Docker Images mới nhất từ Registry..." -ForegroundColor Cyan
docker compose -f docker-compose.prod.yml pull

# 3. Khởi động / Nâng cấp các dịch vụ ngầm (Detached mode)
Write-Host "🔄 Khởi động hệ thống dịch vụ (SQL Server, Server API, Web Portal)..." -ForegroundColor Cyan
docker compose -f docker-compose.prod.yml up -d --remove-orphans

# 4. Kiểm tra trạng thái các container
Write-Host "⏳ Đang kiểm tra trạng thái hoạt động của các dịch vụ..." -ForegroundColor Gray
Start-Sleep -Seconds 5
docker compose -f docker-compose.prod.yml ps

Write-Host "✅ Triển khai hoàn tất thành công!" -ForegroundColor Green
Write-Host "📍 Server API:   http://localhost:5000 (Swagger: /swagger, Health: /health)" -ForegroundColor Yellow
Write-Host "📍 Web Portal:   http://localhost:5001" -ForegroundColor Yellow
