; --- ARCHIVO DE CONFIGURACIÓN INNO SETUP ---

#define AppName "InventarioILS"
#define AppExeName "InventarioILS.exe"
; La versión se toma de la variable de entorno que definimos en el Workflow de GitHub
#define AppVersion GetEnv('APP_VERSION')
#define AppVersionClean GetEnv('CLEAN_APP_VERSION')

[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
VersionInfoVersion={#AppVersionClean}
VersionInfoDescription="Inventario ILS - Version {#AppVersion}"
AppPublisher=Instituto La Salette - Practicantes 2025-2026

SetupIconFile="InventarioILS\Resources\logo.ico"
UninstallDisplayIcon={app}\{#AppExeName}

DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}

Compression=lzma2
SolidCompression=yes

OutputDir=.\installer_output
OutputBaseFilename=InventarioILS_Setup_v{#AppVersionClean}

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

[Code]
procedure CurUninstallStepChanged(UninstallStep: TUninstallStep);
var
  DataPath: string;
begin
  // Verificamos que estamos en el paso final de la desinstalación
  if UninstallStep = usPostUninstall then
  begin
    // Construimos la ruta a la carpeta de AppData
    DataPath := ExpandConstant('{userappdata}\InventarioILS');

    // Si la carpeta existe, preguntamos al usuario
    if DirExists(DataPath) then
    begin
      if MsgBox('¿Desea eliminar permanentemente la base de datos y sus datos? Ten en cuenta que esta acción es irreversible.', 
                mbConfirmation, MB_YESNO) = IDYES then
      begin
        // Borra la carpeta y todo su contenido (True = subcarpetas, True = archivos)
        DelTree(DataPath, True, True, True);
      end;
    end;
  end;
end;
