; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppID={{8220E838-DFF3-45AA-B338-EC34684489EA}
AppName=Arduino Enhanced Release for Windows
AppVersion=v1.0.1f
AppPublisher=Erwin Ried
AppPublisherURL=http://servicios.ried.cl/
AppSupportURL=http://servicios.ried.cl/
AppUpdatesURL=http://servicios.ried.cl/
DefaultDirName={pf}\Arduino\Arduino ERW 1.0.1f
DefaultGroupName=Arduino
OutputDir=setup
;OutputBaseFilename=setup
;Compression=none
Compression=lzma2/ultra64
SolidCompression=true
DisableProgramGroupPage=yes
ChangesAssociations=true
InternalCompressLevel=Fast
UsePreviousAppDir=false
ChangesEnvironment=true
ShowLanguageDialog=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"
Name: "inofiletype"; Description: "Associate .ino files"; GroupDescription: "File Associations:"
Name: "pdefiletype"; Description: "Associate .pde files"; GroupDescription: "File Associations:"
Name: "installdrivers"; Description: "Show me options to Configure Board Drivers"; GroupDescription: "Setup Board Drivers:"

[Files]
Source: "files\arduino-1.0.1\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Arduino ERW 1.0.1f"; Filename: {app}\arduino.exe; 
Name: "{group}\Configure Board Drivers"; Filename: {app}\drivers\DriverHelper.exe; Flags: excludefromshowinnewinstall; 
Name: "{userdesktop}\Arduino ERW 1.0.1f"; Filename: {app}\arduino.exe; Tasks: desktopicon; 

;[Registry]
;Root: HKCR; SubKey: xapfile\shell\open\; ValueType: string; ValueName: command; ValueData: "{app}\wp7-deploy.exe"; Flags: UninsDeleteKey; Tasks: xapfiletype;
;Root: HKCR; SubKey: .xap; ValueType: string; ValueName: ""; ValueData: xapfile; Flags: UninsDeleteKey; Tasks: xapfiletype;
;Root: HKCU; SubKey: software\classes\xapfile\shell\open\; ValueType: string; ValueName: Command; ValueData: "{app}\wp7-deploy.exe"; Flags: UninsDeleteKey; Tasks: xapfiletype;
;Root: HKCU; SubKey: software\classes\.xap; ValueType: string; ValueName: ""; ValueData: xapfile; Flags: UninsDeleteKey; Tasks: xapfiletype;
;Root: HKLM; SubKey: software\classes\xapfile\shell\open\; ValueType: string; ValueName: Command; ValueData: "{app}\wp7-deploy.exe"; Flags: UninsDeleteKey; Tasks: xapfiletype;
;Root: HKLM; SubKey: software\classes\.xap; ValueType: string; ValueName: ""; ValueData: xapfile; Flags: UninsDeleteKey; Tasks: xapfiletype;

[Registry]
Root: HKCR; Subkey: ".ino"; ValueType: string; ValueName: ""; ValueData: "inofile"; Flags: UninsDeleteKey; Tasks: inofiletype; 
Root: HKCR; Subkey: "inofile"; ValueType: string; ValueName: ""; ValueData: ""; Flags: UninsDeleteKey; Tasks: inofiletype; 
;Root: HKCR; Subkey: "xapfile\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\wp7-deploy.exe"; Flags: UninsDeleteKey; Tasks: xapfiletype;
Root: HKCR; Subkey: "inofile\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\arduino.exe"" ""%1"""; Flags: UninsDeleteKey; Tasks: inofiletype;
Root: HKCR; Subkey: ".pde"; ValueType: string; ValueName: ""; ValueData: "inofile"; Flags: UninsDeleteKey; Tasks: pdefiletype; 
Root: HKCR; Subkey: "pdefile"; ValueType: string; ValueName: ""; ValueData: ""; Flags: UninsDeleteKey; Tasks: pdefiletype; 
;Root: HKCR; Subkey: "xapfile\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\wp7-deploy.exe"; Flags: UninsDeleteKey; Tasks: xapfiletype;
Root: HKCR; Subkey: "pdefile\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\arduino.exe"" ""%1"""; Flags: UninsDeleteKey; Tasks: pdefiletype;


[code]
function IsDotNetDetected(version: string; service: cardinal): boolean;
// Indicates whether the specified version and service pack of the .NET Framework is installed.
//
// version -- Specify one of these strings for the required .NET Framework version:
//    'v1.1.4322'     .NET Framework 1.1
//    'v2.0.50727'    .NET Framework 2.0
//    'v3.0'          .NET Framework 3.0
//    'v3.5'          .NET Framework 3.5
//    'v4\Client'     .NET Framework 4.0 Client Profile
//    'v4\Full'       .NET Framework 4.0 Full Installation
//
// service -- Specify any non-negative integer for the required service pack level:
//    0               No service packs required
//    1, 2, etc.      Service pack 1, 2, etc. required
var
    key: string;
    install, serviceCount: cardinal;
    success: boolean;
begin
    key := 'SOFTWARE\Microsoft\NET Framework Setup\NDP\' + version;
    // .NET 3.0 uses value InstallSuccess in subkey Setup
    if Pos('v3.0', version) = 1 then begin
        success := RegQueryDWordValue(HKLM, key + '\Setup', 'InstallSuccess', install);
    end else begin
        success := RegQueryDWordValue(HKLM, key, 'Install', install);
    end;
    // .NET 4.0 uses value Servicing instead of SP
    if Pos('v4', version) = 1 then begin
        success := success and RegQueryDWordValue(HKLM, key, 'Servicing', serviceCount);
    end else begin
        success := success and RegQueryDWordValue(HKLM, key, 'SP', serviceCount);
    end;
    result := success and (install = 1) and (serviceCount >= service);
end;

function InitializeSetup(): Boolean;
begin
    if not IsDotNetDetected('v4\Client', 0) then begin
        MsgBox('This program requires Microsoft .NET Framework 4.0 Client Profile.'#13#13
            'Please use Windows Update, look for the required Framework and run this setup again.', mbInformation, MB_OK);
        result := false;
    end else
        result := true;
end;

[Run]
Tasks: installdrivers; Filename: {app}\drivers\DriverHelper.exe; 
Filename: {app}\erw\SetPath.exe; Parameters: "-a ""{app}\hardware\tools\avr\bin"" -r hardware\tools\avr\bin"; Flags: RunMinimized; 
Filename: {app}\Arduino.exe; Flags: PostInstall; Description: "Open Arduino Enhanced Release for Windows"; 

[UninstallRun]
Filename: {app}\erw\SetPath.exe; Parameters: "-r hardware\tools\avr\bin"; Flags: RunHidden; 