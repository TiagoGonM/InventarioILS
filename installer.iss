; --- ARCHIVO DE CONFIGURACIÓN INNO SETUP ---

#define AppName "InventarioILS"
#define AppExeName "InventarioILS.exe"
; La versión se toma de la variable de entorno que definimos en el Workflow de GitHub
#define AppVersion GetEnv('APP_VERSION')

[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Instituto La Salette - Practicantes 2025-2026

SetupIconFile=InventarioILS\Resources\logo.ico
UninstallDisplayIcon={app}\{#AppExeName}

DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}

Compression=lzma2
SolidCompression=yes

OutputDir=.\installer_output
OutputBaseFilename=InventarioILS_Setup_v{#AppVersion}

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
; Opción para que el usuario elija si quiere icono en el escritorio
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "startmenuicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
; Aquí le decimos que agarre todo lo que genere el "dotnet publish"
Source: "publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: startmenuicon
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
; Para que el usuario pueda abrir la app apenas termine el wizard
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,InventarioILS}"; Flags: nowait postinstall skipifsilent
