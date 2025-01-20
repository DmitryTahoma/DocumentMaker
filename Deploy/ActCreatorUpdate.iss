; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "ActCreator"
#define MyAppVersion "1.1.1.5"
#define MyAppId "0001"
#define MyAppPublisher "Five-BN"
#define MyAppURL "https://five-bn.com/"
#define MyAppExeName "ActCreator.exe"
#define MyAppAssocName "DMX"
#define MyAppAssocExt ".dmx"

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
DefaultDirName={commonpf64}\{#MyAppPublisher}\{#MyAppName}
VersionInfoDescription={#MyAppName} InnoSetup
VersionInfoCopyright={#MyAppPublisher}
VersionInfoVersion=1.0.0.0
DisableProgramGroupPage=yes
; Uncomment the following line to run in non administrative install mode (install for current user only.)
;PrivilegesRequired=lowest
OutputDir=Install\{#MyAppName}\Update\{#MyAppVersion}
OutputBaseFilename=ActCreatorUpdate
SetupIconFile=..\ActCreator\icon.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Dirs]
Name: "{app}"; Permissions: everyone-full

[Files]
Source: "..\ActCreator\bin\x64\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\Dml.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\MaterialDesignColors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\MaterialDesignThemes.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\ProgramSettings.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\SendException.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\SpecialSymbolReplacer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\System.ComponentModel.Annotations.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\Telegram.Bot.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\employees.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActCreator\bin\x64\Release\projectnames.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\UpdaterAPI.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\UpdaterAPI.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\WinSCP.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\WinSCPnet.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\DocumentMaker\bin\x64\Release\Updater.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Install\{#MyAppName}\updatetext.txt"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Registry]
Root: HKCR; Subkey: "{#MyAppAssocExt}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppName}{#MyAppAssocExt}"; Flags: uninsdeletevalue
Root: HKCR; Subkey: "{#MyAppName}{#MyAppAssocExt}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKCR; Subkey: "{#MyAppName}{#MyAppAssocExt}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKCR; Subkey: "{#MyAppName}{#MyAppAssocExt}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""

[Run]
Filename: "{app}\{#MyAppExeName}"; Parameters: "/afterUpdate"; Flags: 64bit nowait postinstall

