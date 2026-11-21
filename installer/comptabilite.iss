[Setup]
AppId=AGUMAAA.GestionTresorerie.Comptabilite
AppName=AGUMAAA - Comptabilité
AppVersion=1.0
AppVerName=AGUMAAA - Comptabilité ({#ShortSha})
AppPublisher=AGUMAAA
DefaultDirName={localappdata}\Programs\AGUMAAA-Comptabilite
DisableDirPage=yes
DefaultGroupName=AGUMAAA
PrivilegesRequired=lowest
ArchitecturesAllowed=x64os
ArchitecturesInstallIn64BitMode=x64os
OutputDir={#OutputDir}
OutputBaseFilename=gestionTresorerie-comptabilite-setup-{#ShortSha}
Compression=lzma2/fast
SolidCompression=yes
UninstallDisplayIcon={app}\gestionTresorerie.exe

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Files]
Source: "{#PayloadDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#CodexDir}\*"; DestDir: "{app}\analyse"; Flags: ignoreversion
Source: "{#OcrDir}\*"; DestDir: "{app}\ocr"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\AGUMAAA - Comptabilité"; Filename: "{app}\gestionTresorerie.exe"; Parameters: "--comptabilite"; WorkingDir: "{app}"

[Run]
Filename: "{app}\gestionTresorerie.exe"; Parameters: "--comptabilite"; Description: "Ouvrir AGUMAAA - Comptabilité"; Flags: postinstall nowait skipifsilent

[Code]

function DesktopRuntime8Available(): Boolean;
var
  DotNetRoot, DotNetExe, Line, Version: String;
  Output: TExecOutput;
  ExitCode, I, SpaceAt: Integer;
begin
  Result := False;
  if not RegQueryStringValue(HKLM64,
      'SOFTWARE\dotnet\Setup\InstalledVersions\x64', 'InstallLocation', DotNetRoot) then
    DotNetRoot := ExpandConstant('{commonpf64}\dotnet');
  DotNetExe := AddBackslash(DotNetRoot) + 'dotnet.exe';
  if not FileExists(DotNetExe) then
    Exit;
  if not ExecAndCaptureOutput(DotNetExe, '--list-runtimes', '', SW_SHOWNORMAL,
      ewWaitUntilTerminated, ExitCode, Output) then
    Exit;
  if (ExitCode <> 0) or Output.Error then
    Exit;
  for I := 0 to GetArrayLength(Output.StdOut) - 1 do
  begin
    Line := Trim(Output.StdOut[I]);
    if Pos('Microsoft.WindowsDesktop.App ', Line) = 1 then
    begin
      Version := Copy(Line, Length('Microsoft.WindowsDesktop.App ') + 1, Length(Line));
      SpaceAt := Pos(' ', Version);
      if SpaceAt > 0 then
        Version := Copy(Version, 1, SpaceAt - 1);
      if (Pos('8.0.', Version) = 1) and (Pos('-', Version) = 0) then
      begin
        Result := True;
        Exit;
      end;
    end;
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := DesktopRuntime8Available();
  if not Result then
    SuppressibleMsgBox(
      'Microsoft .NET Desktop Runtime 8 x64 est nécessaire.' + #13#10 +
      'Installez-le depuis https://dotnet.microsoft.com/download/dotnet/8.0 puis relancez cet installateur.',
      mbError, MB_OK, IDOK);
end;

