# ===================================================================
# DOCKERFILE CHO BLAZOR WEB PORTAL (GymManagement.Web)
# Multi-stage build tối ưu cho ứng dụng Blazor Server trên .NET 8
# ===================================================================

# Giai đoạn 1: Build & Publish ứng dụng với .NET 8 SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sao chép csproj để tận dụng cache
COPY ["src/Server/GymManagement.Server.Core/GymManagement.Server.Core.csproj", "src/Server/GymManagement.Server.Core/"]
COPY ["src/Client/GymManagement.Web/GymManagement.Web.csproj", "src/Client/GymManagement.Web/"]

# Khôi phục dependencies
RUN dotnet restore "src/Client/GymManagement.Web/GymManagement.Web.csproj"

# Sao chép mã nguồn liên quan
COPY ["src/Server/GymManagement.Server.Core/", "src/Server/GymManagement.Server.Core/"]
COPY ["src/Client/GymManagement.Web/", "src/Client/GymManagement.Web/"]

# Biên dịch và xuất bản binaries ở chế độ Release
WORKDIR "/src/src/Client/GymManagement.Web"
RUN dotnet publish "GymManagement.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Giai đoạn 2: Runtime image tinh gọn
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8081
EXPOSE 8081

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "GymManagement.Web.dll"]
