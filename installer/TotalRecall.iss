; ============================================================================
;  TotalRecall - Inno Setup script
;  Produces a single per-user setup.exe (no admin/UAC prompt) that installs
;  the WinForms app and the bundled MCP server.
;
;  Build:   "%LOCALAPPDATA%\Programs\Inno Setup 6\ISCC.exe" /Qp installer\TotalRecall.iss
;  Output:  installer\Output\TotalRecall-1.0.1-Setup.exe
;
;  This script consumes the already-published self-contained binaries staged at:
;    artifacts\staging\TotalRecall-1.0.1-win-x64\TotalRecall\        (WinForms)
;    artifacts\staging\TotalRecall-1.0.1-win-x64\TotalRecall.Mcp\    (MCP server)
; ============================================================================

#define MyAppName        "TotalRecall"
#define MyAppVersion     "1.0.1"
#define MyAppPublisher   "Ilya Fainberg"
#define MyAppURL         "https://github.com/ilyafainberg/TotalRecall"
#define MyAppExeName     "TotalRecall.exe"
#define MyAppId          "{{A8F6B2C4-3D9E-4F1A-B8C7-1E5D2A9F4B6C}"
#define StageRoot        "..\artifacts\staging\TotalRecall-" + MyAppVersion + "-win-x64"

[Setup]
AppId={#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}/issues
AppUpdatesURL={#MyAppURL}/releases
AppCopyright=Copyright (C) 2026 {#MyAppPublisher}
VersionInfoVersion={#MyAppVersion}.0
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription={#MyAppName} Setup
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}.0

; Per-user install: no admin, no UAC prompt
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
DisableDirPage=no
ShowLanguageDialog=no

LicenseFile=..\LICENSE
OutputDir=Output
OutputBaseFilename={#MyAppName}-{#MyAppVersion}-Setup
SetupIconFile=..\TotalRecall\app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName} {#MyAppVersion}

Compression=lzma2/ultra64
SolidCompression=yes
LZMAUseSeparateProcess=yes

WizardStyle=modern
WizardResizable=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0

; Code-signing hook: configured via /S"signtool=..." switch when calling ISCC,
; e.g. SignTool=signtool sign /fd sha256 /f cert.pfx $f
; SignedUninstaller=yes
; SignTool=mysigntool

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon";   Description: "{cm:CreateDesktopIcon}";        GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startmenuicon"; Description: "Create a Start Menu shortcut";  GroupDescription: "{cm:AdditionalIcons}"
Name: "startup";       Description: "Start {#MyAppName} when I sign in to Windows (in the system tray)"; GroupDescription: "Background mode"; Flags: unchecked

[Files]
; --- WinForms app + .NET 10 self-contained runtime + tessdata ---
Source: "{#StageRoot}\TotalRecall\*";     DestDir: "{app}";           Flags: ignoreversion recursesubdirs createallsubdirs

; --- MCP server, kept in its own subfolder ---
Source: "{#StageRoot}\TotalRecall.Mcp\*"; DestDir: "{app}\McpServer"; Flags: ignoreversion recursesubdirs createallsubdirs

; --- Docs at install root ---
Source: "..\README.md";                   DestDir: "{app}";           Flags: ignoreversion
Source: "..\LICENSE";                     DestDir: "{app}";           Flags: ignoreversion
Source: "..\CHANGELOG.md";                DestDir: "{app}";           Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}";                          Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{group}\Project page on GitHub";                Filename: "{#MyAppURL}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}";    Filename: "{uninstallexe}"
Name: "{autoprograms}\{#MyAppName}";                   Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: startmenuicon
Name: "{autodesktop}\{#MyAppName}";                    Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Registry]
; "Start when I sign in" task → per-user Run key, launches into the tray.
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; \
    ValueType: string; ValueName: "{#MyAppName}"; \
    ValueData: """{app}\{#MyAppExeName}"" --tray"; \
    Flags: uninsdeletevalue; Tasks: startup

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; The app writes settings + db.key + recall.db to %LOCALAPPDATA%\TotalRecall.
; Don't nuke it on uninstall - users may want to keep their captured history.
; To wipe everything: uninstall, then delete %LOCALAPPDATA%\TotalRecall manually.
