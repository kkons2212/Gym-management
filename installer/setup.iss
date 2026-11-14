; ===================================================================
; INNO SETUP SCRIPT CHO ỨNG DỤNG GYM MANAGEMENT (DESKTOP CLIENT)
; Đóng gói ứng dụng WinUI 3 thành 1 file cài đặt GymManagement-Setup.exe
; ===================================================================

#ifndef AppVersion
  #define AppVersion "1.0.0"
#endif

#ifndef SourceDir
  #define SourceDir "..\publish\desktop"
#endif

#ifndef OutputDir
  #define OutputDir "..\publish"
#endif

#ifndef OutputBaseFilename
  #define OutputBaseFilename "GymManagement-Setup-win-x64"
#endif

#ifndef IconFile
  #define IconFile "..\src\Client\GymManagement.Desktop\Assets\AppIcon.ico"
#endif

[Setup]
AppId={{8B264E2A-5D21-4A59-8653-C8B99D854F2E}
AppName=Gym Management System
AppVersion={#AppVersion}
AppVerName=Gym Management System {#AppVersion}
AppPublisher=Gym Management Team
DefaultDirName={autopf}\GymManagement
DefaultGroupName=Gym Management
AllowNoIcons=yes
OutputDir={#OutputDir}
OutputBaseFilename={#OutputBaseFilename}
SetupIconFile={#IconFile}
UninstallDisplayIcon={app}\GymManagement.Desktop.exe
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Gym Management System"; Filename: "{app}\GymManagement.Desktop.exe"
Name: "{group}\{cm:UninstallProgram,Gym Management System}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Gym Management System"; Filename: "{app}\GymManagement.Desktop.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\GymManagement.Desktop.exe"; Description: "{cm:LaunchProgram,Gym Management System}"; Flags: nowait postinstall skipifsilent
