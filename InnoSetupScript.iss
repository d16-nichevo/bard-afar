; This is an Inno Setup script.
; https://jrsoftware.org/isinfo.php

; This script assumes publishing to a single file.

; You will need to change these #defines to point to
; the correct location on your file system.

; BinPath: where to source compiled files from:
#define BinPath "C:\Documents\Programs\BardAfar\bin\Release\net6.0-windows\publish\win-x64"

; Output path: where the installer file will go:
#define OutputPath "C:\Documents\Programs\BardAfar\bin\Release\net6.0-windows\publish\win-x64"

; AppVer: point this to the BardAfar exe:
#define AppVer GetVersionNumbersString("C:\Documents\Programs\BardAfar\bin\Release\net6.0-windows\publish\win-x64\BardAfar.exe")

; Default ports for HttpListener and WebSocket
#define PortHttp "58470"
#define PortWebSocket "58471"

; You should not need to change anything below.

[Setup]
AppName=Bard Afar
AppVersion={#AppVer}
AppVerName=Bard Afar v{#AppVer}
AppPublisher=Deck16
AppPublisherURL=https://github.com/d16-nichevo/bard-afar#readme
AppSupportURL=https://github.com/d16-nichevo/bard-afar#readme
AppUpdatesURL=https://github.com/d16-nichevo/bard-afar#readme
DefaultDirName={autopf}\BardAfar
DefaultGroupName=BardAfar
SourceDir={#BinPath}
OutputDir={#OutputPath}
OutputBaseFilename=BardAfar-Installer
;WizardImageBackColor=clBlack
;WizardImageStretch=false
;WizardImageFile=D:\Documents\Programs\Inno Setup\PS2DJ\wizard-large.bmp
;WizardSmallImageFile=D:\Documents\Programs\Inno Setup\PS2DJ\wizard-small.bmp
AllowNoIcons=true
UninstallDisplayIcon={app}\BardAfar.exe
PrivilegesRequired=admin
MissingRunOnceIdsWarning=no
ArchitecturesInstallIn64BitMode=x64

[Tasks]
Name: firewallrules; Description: "Add &firewall exceptions for ports {#PortHttp} and {#PortWebSocket}"
Name: desktopicons; Description: "Create desktop &icons"

[Files]
Source: "BardAfar.exe"; DestDir: "{app}";
Source: "BardAfar.dll.config"; DestDir: "{app}";

[Icons]
Name: "{group}\BardAfar"; Filename: "{app}\BardAfar.exe.exe"; WorkingDir: "{app}";
Name: "{group}\Uninstall BardAfar"; Filename: "{uninstallexe}"
Name: "{commondesktop}\BardAfar"; Filename: "{app}\BardAfar.exe"; WorkingDir: "{app}"; Tasks: desktopicons

[Run]
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall add rule name= ""BardAfar"" dir=in action=allow protocol=TCP localport={#PortHttp},{#PortWebSocket}"; StatusMsg: "Adding firewall rule (ports)"; Flags: runhidden; Tasks: firewallrules
Filename: "{sys}\netsh.exe"; Parameters: "http add urlacl url=http://*:{#PortHttp}/ user=""Everyone""" ; StatusMsg: "Adding firewall rule (urlacl)"; Flags: runhidden; Tasks: firewallrules

[UninstallRun]
Filename: "{sys}\netsh.exe"; Parameters: "advfirewall firewall delete rule name=""BardAfar"""; StatusMsg: "Removing firewall rule (ports)"; Flags: runhidden; Tasks: firewallrules
Filename: "{sys}\netsh.exe"; Parameters: "http delete urlacl url=http://*:{#PortHttp}/" ; StatusMsg: "Removing firewall rule (urlacl)"; Flags: runhidden; Tasks: firewallrules