[Setup]
#define MyAppSetupName 'KermoCompanyUpdater'
#define MyAppVersion '1.0'
#define MyAppPublisher 'Kermo Company'
#define MyAppCopyright 'Copyright © Kermo Company'
#define MyAppURL 'http://51.38.131.66/api/'

AppName={#MyAppSetupName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppSetupName} {#MyAppVersion}
AppCopyright={#MyAppCopyright}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
OutputBaseFilename={#MyAppSetupName}
DefaultGroupName={#MyAppSetupName}
DefaultDirName={autopf64}
OutputDir={#SourcePath}\bin
AllowNoIcons=yes
PrivilegesRequired=admin

ArchitecturesInstallIn64BitMode=x64

[Languages]
Name: en; MessagesFile: "compiler:Default.isl"
Name: pl; MessagesFile: "compiler:Languages\Polish.isl"
Name: fr; MessagesFile: "compiler:Languages\French.isl"
Name: it; MessagesFile: "compiler:Languages\Italian.isl"
Name: de; MessagesFile: "compiler:Languages\German.isl"
Name: es; MessagesFile: "compiler:Languages\Spanish.isl"

[Files]
Source: "bin\Release\net8.0-windows\KermoCompanyUpdater.exe"; DestDir: "{app}"; DestName: "KermoCompanyUpdater.exe";
Source: "bin\Release\net8.0-windows\KermoCompanyUpdater.deps.json"; DestDir: "{app}";
Source: "bin\Release\net8.0-windows\KermoCompanyUpdater.dll"; DestDir: "{app}";
Source: "bin\Release\net8.0-windows\KermoCompanyUpdater.runtimeconfig.json"; DestDir: "{app}";
Source: "bin\Release\net8.0-windows\Newtonsoft.Json.dll"; DestDir: "{app}";
Source: "bin\Release\net8.0-windows\dotnet_installer.exe"; DestDir: "{tmp}"; Flags: deleteafterinstall

[Icons]
Name: "{group}\{#MyAppSetupName}"; Filename: "{app}\KermoCompanyUpdater.exe"

[Run]
Filename: "{tmp}\dotnet_installer.exe"; Parameters: "/quiet /norestart"; Check: IsDotNet8Installed; StatusMsg: "Installing .NET 8..."

[Code]
function IsDotNet8Installed: Boolean;
var
  success: Boolean;
  releaseValue: Cardinal;
begin
  // Domyślnie zakładamy, że .NET 8 nie jest zainstalowany
  Result := False;

  // Sprawdzanie wartości klucza 'Release' dla .NET 8 w rejestrze
  success := RegQueryDWordValue(HKLM, 'SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\', 'Release', releaseValue);
  
  // Jeśli klucz 'Release' istnieje i jego wartość wskazuje na .NET 8 lub nowszy, uznajemy, że .NET 8 jest zainstalowany
  if success and (releaseValue >= 82348) then
  begin
    Result := True;
  end;
end;
