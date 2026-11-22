$ErrorActionPreference = 'Stop'
$root = (Resolve-Path "$PSScriptRoot/../..").Path
$exe = @(Get-ChildItem "$root/gestionTresorerie/bin" -Recurse -Filter gestionTresorerie.exe |
    Where-Object { $_.FullName -match '[\\/]Release[\\/]net8\.0-windows10\.0\.19041\.0[\\/]gestionTresorerie\.exe$' })
if ($exe.Count -ne 1) { throw 'Binaire Release absent ou ambigu.' }
$source = $exe[0].DirectoryName
$stage = New-Item -Type Directory "$root/artifacts/payload-comptabilite"
foreach ($f in Get-ChildItem $source -File -Recurse) {
    if ($f.Extension -ine '.dll' -and $f.Name -notin @('gestionTresorerie.exe','gestionTresorerie.deps.json','gestionTresorerie.runtimeconfig.json','PromptGemini.txt')) { continue }
    $dest = Join-Path $stage.FullName ([IO.Path]::GetRelativePath($source, $f.FullName))
    [IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($dest)) | Out-Null
    Copy-Item -LiteralPath $f.FullName -Destination $dest
}
$sha = (git rev-parse --short=8 HEAD).Trim()
$output = New-Item -Type Directory "$root/artifacts/installer-comptabilite"
$compiler = Join-Path ([Environment]::GetEnvironmentVariable('ProgramFiles(x86)')) 'Inno Setup 6/ISCC.exe'
& $compiler "/DPayloadDir=$($stage.FullName)" "/DShortSha=$sha" "/DOutputDir=$($output.FullName)" "/DOcrDir=$root/artifacts/ocr" "/DCodexDir=$root/artifacts/codex" "$root/installer/comptabilite.iss"
if ($LASTEXITCODE) { throw "Erreur Inno Setup $LASTEXITCODE" }
$setup = Get-Item "$output/gestionTresorerie-comptabilite-setup-$sha.exe"
$p = Start-Process $setup.FullName -ArgumentList '/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART' -Wait -PassThru
if ($p.ExitCode) { throw "Installation Release en échec : $($p.ExitCode)" }
$link = Get-ChildItem ([Environment]::GetFolderPath('Programs')) -Recurse -Filter 'AGUMAAA - Comptabilité.lnk' | Select-Object -First 1
if (!$link) { throw 'Raccourci de comptabilité absent.' }
$shortcut = (New-Object -ComObject WScript.Shell).CreateShortcut($link.FullName)
if ($shortcut.Arguments -ne '--comptabilite') { throw 'Arguments incorrects.' }
$installed = Join-Path (Split-Path $shortcut.TargetPath) 'gestionTresorerie.dll'
if ((Get-FileHash $installed).Hash -ne (Get-FileHash "$source/gestionTresorerie.dll").Hash) { throw 'Le binaire installé diffère de Release.' }
$app = Start-Process $shortcut.TargetPath -ArgumentList $shortcut.Arguments -PassThru
try {
    $limit = (Get-Date).AddSeconds(50)
    do {
        Start-Sleep -Milliseconds 400
        $app.Refresh()
        if ($app.HasExited) { throw 'Fermeture prématurée au démarrage.' }
    } until ($app.MainWindowTitle -like '*TEST SQL*' -or (Get-Date) -gt $limit)
    if ($app.MainWindowTitle -notlike '*TEST SQL*') { throw "Accueil TEST absent : $($app.MainWindowTitle)" }
} finally {
    if (!$app.HasExited) { $app.Kill(); $app.WaitForExit() }
}
