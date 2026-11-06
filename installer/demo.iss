#ifndef PayloadDir
  #error PayloadDir must point to the validated DEMO_BUILD payload.
#endif
#ifndef BuildSha
  #error BuildSha is required.
#endif
#ifndef ShortSha
  #error ShortSha is required.
#endif
#ifndef OutputDir
  #error OutputDir is required.
#endif
#ifndef SourceRoot
  #error SourceRoot is required.
#endif

[Setup]
AppId=gestionTresorerie.Demo
AppName=gestionTresorerie - Démonstration
AppVersion=0.1.0
AppVerName=gestionTresorerie - Démonstration ({#ShortSha})
AppPublisher=AGUMAAA
DefaultDirName={localappdata}\Programs\gestionTresorerie-Demo
DefaultGroupName=gestionTresorerie - Demonstration
DisableDirPage=yes
DisableProgramGroupPage=yes
UsePreviousAppDir=no
PrivilegesRequired=lowest
ArchitecturesAllowed=x64os
ArchitecturesInstallIn64BitMode=x64os
MinVersion=10.0.17763
OutputDir={#OutputDir}
OutputBaseFilename=gestionTresorerie-demo-setup-{#ShortSha}
Compression=lzma2/fast
SolidCompression=yes
WizardStyle=modern
UninstallDisplayName=gestionTresorerie - Démonstration
UninstallDisplayIcon={app}\gestionTresorerie.exe
InfoBeforeFile={#SourceRoot}\docs\installateur-demo.txt
CloseApplications=yes
RestartApplications=no
VersionInfoVersion=0.1.0.0
VersionInfoDescription=gestionTresorerie - Demonstration
VersionInfoTextVersion=0.1.0-{#ShortSha}

[Languages]
Name: "french"; MessagesFile: "compiler:Languages\French.isl"

[Tasks]
Name: "desktopicon"; Description: "Créer un raccourci sur le Bureau"; Flags: unchecked

[Files]
Source: "{#PayloadDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#SourceRoot}\docs\installateur-demo.txt"; DestDir: "{app}"; DestName: "LIRE_INSTALLATION.txt"; Flags: ignoreversion

[Icons]
Name: "{group}\gestionTresorerie - Demonstration"; Filename: "{app}\gestionTresorerie.exe"; Parameters: "--demo"; WorkingDir: "{app}"
Name: "{userdesktop}\gestionTresorerie - Demonstration"; Filename: "{app}\gestionTresorerie.exe"; Parameters: "--demo"; WorkingDir: "{app}"; Tasks: desktopicon
Name: "{group}\Desinstaller la demonstration"; Filename: "{uninstallexe}"

[Run]
Filename: "{app}\gestionTresorerie.exe"; Parameters: "--demo"; Description: "Démarrer la démonstration"; Flags: nowait postinstall skipifsilent

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

function PrepareToInstall(var NeedsRestart: Boolean): String;
var
  ExpectedDir, SelectedDir: String;
  TraceLines: TArrayOfString;
  I: Integer;
  IsDemo: Boolean;
begin
  Result := '';
  ExpectedDir := ExpandFileName(ExpandConstant('{localappdata}\Programs\gestionTresorerie-Demo'));
  SelectedDir := ExpandFileName(ExpandConstant('{app}'));
  if CompareText(SelectedDir, ExpectedDir) <> 0 then
  begin
    Result := 'Le dossier dédié à la démonstration est imposé pour cette candidate.';
    Exit;
  end;
  if DirExists(SelectedDir) then
  begin
    IsDemo := False;
    if LoadStringsFromFile(AddBackslash(SelectedDir) + 'COMMIT-DEMONSTRATION.txt', TraceLines) then
      for I := 0 to GetArrayLength(TraceLines) - 1 do
        if TraceLines[I] = 'CONSTANTE=DEMO_BUILD' then
          IsDemo := True;
    if not IsDemo then
      Result := 'Le dossier existant ne contient pas une démonstration reconnue. Installation arrêtée.';
  end;
end;
