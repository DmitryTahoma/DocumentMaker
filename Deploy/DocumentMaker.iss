; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "DocumentMaker"
#define MyAppVersion "1.1.0.0"
#define MyAppId "0001"
#define MyAppPublisher "Five-BN"
#define MyAppURL "https://five-bn.com/"
#define MyAppExeName "DocumentMaker.exe"
#define MyAppAssocName "DCMK"
#define MyAppAssocExt ".dcmk"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={#MyAppName}{#MyAppPublisher}{#MyAppId}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName=D:\{#MyAppPublisher}\{#MyAppName}
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=Setup
OutputBaseFilename=DocumentMakerSetup
SetupIconFile=..\DocumentMaker\icon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";

[Dirs]
Name: "{app}"; Permissions: everyone-full

[Files]
Source: "..\DocumentMaker\bin\x64\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\DevelopmentTypes.xlsx"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\Dml.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\DocumentFormat.OpenXml.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\DocumentFormat.OpenXml.Framework.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\DocumentMakerModelLibrary.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\MaterialDesignColors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\MaterialDesignThemes.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\projectnames.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\SupportTypes.xlsx"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\UpdaterAPI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\UpdaterAPI.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\WinSCP.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\WinSCPnet.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\updatetext.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\Contract\*"; DestDir: "{app}\Contract\"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Registry]
Root: HKCR; Subkey: "{#MyAppAssocExt}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}{#MyAppAssocExt}"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "{#MyAppName}{#MyAppAssocExt}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "{#MyAppName}{#MyAppAssocExt}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKCR; Subkey: "{#MyAppName}{#MyAppAssocExt}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

