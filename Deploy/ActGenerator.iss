; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "ActGenerator"
#define MyAppVersion "0.0.1"
#define MyAppPublisher "Five-BN"
#define MyAppURL "https://five-bn.com/"
#define MyAppExeName "ActGenerator.exe"

[Setup]
AppId={{F3C01151-4656-4181-865D-15600C9C88E8}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppPublisher}\{#MyAppName}
DisableProgramGroupPage=yes
OutputDir=Setup
OutputBaseFilename=ActGeneratorSetup
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
SetupIconFile=Icons\{#MyAppName}Setup.ico

[Languages]
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\ActGenerator\bin\Release\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\Dml.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\DocumentFormat.OpenXml.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\DocumentMakerModelLibrary.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\DocumentMakerThemes.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\EntityFramework.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\EntityFramework.SqlServer.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\MaterialDesignColors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\MaterialDesignThemes.Wpf.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\Microsoft.Xaml.Behaviors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\Mvvm.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\ProjectEditorLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\ProjectsDb.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "..\ActGenerator\bin\Release\Security.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "Properties\ActGenerator\session_act_generator.xml"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent runascurrentuser