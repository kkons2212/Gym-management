<#
.SYNOPSIS
    Script khởi tạo và nạp cơ sở dữ liệu GymManagementDb vào SQL Server trên Docker.
.DESCRIPTION
    Thực thi file Document/init_docker_db.sql bao gồm DDL 9 bảng vật lý và Seed Data
    (Role, User admin băm BCrypt, PackagePlan) vào máy chủ SQL Server.
.PARAMETER Server
    Địa chỉ máy chủ SQL Server (Mặc định: localhost,1433).
.PARAMETER User
    Tên đăng nhập SQL Server (Mặc định: sa).
.PARAMETER Password
    Mật khẩu quản trị viên SQL Server.
.PARAMETER ContainerName
    (Tùy chọn) Tên container Docker nếu muốn thực thi trực tiếp qua docker exec.
.EXAMPLE
    .\scripts\setup-docker-db.ps1 -Password "GymPass@2026Secure!"
.EXAMPLE
    .\scripts\setup-docker-db.ps1 -Server "192.168.1.50,1433" -Password "MyPassword123!"
.EXAMPLE
    .\scripts\setup-docker-db.ps1 -ContainerName "my-sqlserver" -Password "MyPassword123!"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$Server = "localhost,1433",

    [Parameter(Mandatory = $false)]
    [string]$User = "sa",

    [Parameter(Mandatory = $false)]
    [string]$Password = "SqlServer@2026",

    [Parameter(Mandatory = $false)]
    [string]$ContainerName = "",

    [Parameter(Mandatory = $false)]
    [string]$SqlFile = "Document/init_docker_db.sql"
)

$ErrorActionPreference = "Stop"

Write-Host "====================================================================" -ForegroundColor Cyan
Write-Host " HỆ THỐNG QUẢN LÝ PHÒNG TẬP - SETUP DOCKER SQL SERVER DATABASE" -ForegroundColor Cyan
Write-Host "====================================================================" -ForegroundColor Cyan

# 1. Kiểm tra tồn tại của file SQL
if (-not (Test-Path $SqlFile)) {
    Write-Error "❌ Không tìm thấy file SQL tại đường dẫn: $SqlFile"
    exit 1
}

Write-Host "📄 Sử dụng file SQL: $SqlFile" -ForegroundColor Green

# 2. Thực thi qua Docker Exec nếu có chỉ định ContainerName
if ($ContainerName -ne "") {
    Write-Host "🐳 Đang thực thi qua Docker Container: $ContainerName..." -ForegroundColor Yellow
    
    # Copy file SQL vào container
    docker cp $SqlFile "${ContainerName}:/tmp/init_docker_db.sql"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Lỗi copy file SQL vào Docker container $ContainerName"
        exit 1
    }

    # Chạy sqlcmd trong container (hỗ trợ cả mssql-tools18 và mssql-tools)
    $sqlCmdCmd = "if [ -f /opt/mssql-tools18/bin/sqlcmd ]; then /opt/mssql-tools18/bin/sqlcmd -S localhost -U $User -P '$Password' -C -i /tmp/init_docker_db.sql; else /opt/mssql-tools/bin/sqlcmd -S localhost -U $User -P '$Password' -i /tmp/init_docker_db.sql; fi"
    docker exec -i $ContainerName bash -c $sqlCmdCmd

    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Khởi tạo CSDL qua Docker container thành công 100%!" -ForegroundColor Green
    } else {
        Write-Error "❌ Lỗi thực thi sqlcmd bên trong Docker container!"
        exit 1
    }
}
else {
    # 3. Thực thi qua công cụ sqlcmd máy trạm hoặc .NET SqlClient
    Write-Host "📡 Đang kết nối tới SQL Server tại: $Server (User: $User)..." -ForegroundColor Yellow

    $hasSqlCmd = Get-Command "sqlcmd" -ErrorAction SilentlyContinue
    if ($hasSqlCmd) {
        Write-Host "⚙️ Phát hiện công cụ sqlcmd máy trạm, đang thực thi..." -ForegroundColor Cyan
        sqlcmd -S $Server -U $User -P $Password -C -i $SqlFile
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Khởi tạo CSDL hoàn tất thành công 100%!" -ForegroundColor Green
        } else {
            Write-Error "❌ sqlcmd trả về mã lỗi: $LASTEXITCODE"
            exit 1
        }
    }
    else {
        Write-Host "ℹ️ Máy trạm chưa cài sqlcmd CLI. Đang chạy qua .NET SqlClient..." -ForegroundColor Cyan
        $connStr = "Server=$Server;Database=master;User Id=$User;Password=$Password;TrustServerCertificate=True;MultipleActiveResultSets=true"
        $sqlContent = Get-Content -Path $SqlFile -Raw -Encoding UTF8

        # Tách các khối GO
        $batches = $sqlContent -split "(?im)^\s*GO\s*$"

        $connection = New-Object Microsoft.Data.SqlClient.SqlConnection($connStr)
        try {
            $connection.Open()
            Write-Host "🔗 Đã kết nối thành công tới SQL Server." -ForegroundColor Green

            foreach ($batch in $batches) {
                $trimmed = $batch.Trim()
                if ($trimmed.Length -gt 0) {
                    $command = $connection.CreateCommand()
                    $command.CommandText = $trimmed
                    $command.CommandTimeout = 120
                    [void]$command.ExecuteNonQuery()
                }
            }
            Write-Host "✅ Khởi tạo CSDL qua .NET SqlClient hoàn tất thành công 100%!" -ForegroundColor Green
        }
        catch {
            Write-Error "❌ Lỗi kết nối hoặc thực thi SQL: $_"
            exit 1
        }
        finally {
            if ($connection.State -eq 'Open') {
                $connection.Close()
            }
        }
    }
}

Write-Host "====================================================================" -ForegroundColor Cyan
Write-Host " SẴN SÀNG KHỞI ĐỘNG SERVER API & CLIENT!" -ForegroundColor Cyan
Write-Host "====================================================================" -ForegroundColor Cyan
