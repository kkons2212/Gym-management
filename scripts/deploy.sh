#!/usr/bin/env bash
# ===================================================================
# SCRIPT TRIỂN KHAI 1-CLICK TỰ ĐỘNG CHO GYM MANAGEMENT (LINUX VPS)
# ===================================================================

set -e

echo "🚀 [Gym Management] Bắt đầu quy trình triển khai Production..."

# 1. Kiểm tra file cấu hình môi trường .env
if [ ! -f ".env" ]; then
  if [ -f ".env.example" ]; then
    echo "⚠️ Chưa tìm thấy file .env, tự động sao chép từ .env.example..."
    cp .env.example .env
    echo "ℹ️ Vui lòng kiểm tra lại mật khẩu và cấu hình trong file .env nếu cần."
  else
    echo "❌ Không tìm thấy file .env hoặc .env.example! Hủy triển khai."
    exit 1
  fi
fi

# 2. Pull các container image mới nhất từ GitHub Container Registry
echo "📥 Kéo Docker Images mới nhất từ Registry..."
docker compose -f docker-compose.prod.yml pull

# 3. Khởi động / Nâng cấp các dịch vụ ngầm (Detached mode)
echo "🔄 Khởi động hệ thống dịch vụ (SQL Server, Server API, Web Portal)..."
docker compose -f docker-compose.prod.yml up -d --remove-orphans

# 4. Kiểm tra trạng thái các container
echo "⏳ Đang kiểm tra trạng thái hoạt động của các dịch vụ..."
sleep 5
docker compose -f docker-compose.prod.yml ps

echo "✅ Triển khai hoàn tất thành công!"
echo "📍 Server API:   http://localhost:5000 (Swagger: /swagger, Health: /health)"
echo "📍 Web Portal:   http://localhost:5001"
