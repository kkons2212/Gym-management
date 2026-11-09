$targetDir = "$PSScriptRoot\src\Client\GymManagement.Desktop\bin\Debug\net8.0-windows10.0.19041.0\win-x64"
Start-Process -FilePath "$targetDir\GymManagement.Desktop.exe" -WorkingDirectory $targetDir
