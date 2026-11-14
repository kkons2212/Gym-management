# ===================================================================
# DOCKERFILE CHO BACKEND WEB API (GymManagement.Server.Api)
# Multi-stage build tối ưu kích thước và bảo mật theo chuẩn .NET 8
# ===================================================================

# Giai đoạn 1: Build & Publish ứng dụng với .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sao chép file định nghĩa csproj của các tầng để tối ưu Docker Layer Caching
COPY ["src/Server/GymManagement.Server.Core/GymManagement.Server.Core.csproj", "src/Server/GymManagement.Server.Core/"]
COPY ["src/Server/GymManagement.Server.Data/GymManagement.Server.Data.csproj", "src/Server/GymManagement.Server.Data/"]
COPY ["src/Server/GymManagement.Server.Api/GymManagement.Server.Api.csproj", "src/Server/GymManagement.Server.Api/"]

# Khôi phục dependencies
RUN dotnet restore "src/Server/GymManagement.Server.Api/GymManagement.Server.Api.csproj"

# Sao chép toàn bộ mã nguồn của Server
COPY ["src/Server/GymManagement.Server.Core/", "src/Server/GymManagement.Server.Core/"]
COPY ["src/Server/GymManagement.Server.Data/", "src/Server/GymManagement.Server.Data/"]
COPY ["src/Server/GymManagement.Server.Api/", "src/Server/GymManagement.Server.Api/"]

# Biên dịch và xuất bản binaries ở chế độ Release
WORKDIR "/src/src/Server/GymManagement.Server.Api"
RUN dotnet publish "GymManagement.Server.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn 2: Runtime image tinh gọn
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Thiết lập cổng lắng nghe HTTP chuẩn trong container .NET 8
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

# Sao chép sản phẩm đã build từ giai đoạn build
COPY --from=build /app/publish .

# Kiểm tra trạng thái sức khỏe định kỳ (Healthcheck) qua endpoint /health
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "GymManagement.Server.Api.dll"]
