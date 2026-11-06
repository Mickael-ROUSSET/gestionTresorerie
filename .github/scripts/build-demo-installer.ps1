param()
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$sha = (git -C $root rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $sha -notmatch '^[0-9a-f]{40}$') { throw 'Commit Git introuvable.' }
$short = $sha.Substring(0, 8)
$payload = Join-Path $root "artifacts/gestionTresorerie-demo-$short"
$trace = Join-Path $payload 'COMMIT-DEMONSTRATION.txt'
if (-not (Test-Path -LiteralPath $trace -PathType Leaf)) { throw 'Payload de démonstration absent.' }
$lines = @(Get-Content -LiteralPath $trace)
foreach ($expected in @("COMMIT=$sha", 'CONFIGURATION=Demo', 'CONSTANTE=DEMO_BUILD')) {
    if ($lines -cnotcontains $expected) { throw "Traçabilité du payload incorrecte : $expected" }
}
$support = @('LIRE_AVANT_DEMARRER.md', 'LANCER-DEMONSTRATION.cmd', 'COMMIT-DEMONSTRATION.txt')
$entries = @(Get-ChildItem -LiteralPath $payload -Recurse -Force)
foreach ($entry in $entries) {
    if (($entry.Attributes -band [IO.FileAttributes]::ReparsePoint) -ne 0) {
        throw 'Un lien est présent dans le payload.'
    }
    if (-not $entry.PSIsContainer -and
        $entry.Name -notin $support -and $entry.Name -cne 'gestionTresorerie.exe' -and
        $entry.Extension -ine '.dll' -and $entry.Name -notmatch '\.(deps|runtimeconfig)\.json$') {
        throw "Fichier hors liste blanche du payload : $($entry.Name)"
    }
}
$compilerFolder = [Environment]::GetEnvironmentVariable('ProgramFiles(x86)')
$compiler = Join-Path $compilerFolder 'Inno Setup 6/ISCC.exe'
if (-not (Test-Path -LiteralPath $compiler -PathType Leaf)) {
    $command = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($null -eq $command) { throw 'Compilateur Inno Setup 6 absent du runner Windows.' }
    $compiler = $command.Source
}
$output = Join-Path $root 'artifacts/installer'
if (Test-Path -LiteralPath $output) { throw 'Dossier installateur déjà présent. Utiliser un espace propre.' }
New-Item -ItemType Directory -Path $output | Out-Null
Write-Output "Compilateur Inno Setup : $((Get-Item -LiteralPath $compiler).VersionInfo.FileVersion)"
& $compiler "/DPayloadDir=$payload" "/DBuildSha=$sha" "/DShortSha=$short" "/DOutputDir=$output" "/DSourceRoot=$root" (Join-Path $root 'installer/demo.iss')
if ($LASTEXITCODE -ne 0) { throw "Compilation Inno Setup échouée : $LASTEXITCODE" }
$installer = Join-Path $output "gestionTresorerie-demo-setup-$short.exe"
if (-not (Test-Path -LiteralPath $installer -PathType Leaf)) { throw 'Installateur compilé absent.' }
$hash = (Get-FileHash -LiteralPath $installer -Algorithm SHA256).Hash.ToLowerInvariant()
"$hash  $([IO.Path]::GetFileName($installer))" |
    Set-Content -LiteralPath "$installer.sha256" -Encoding ASCII
Copy-Item -LiteralPath (Join-Path $root 'docs/installateur-demo.txt') -Destination (Join-Path $output 'LIRE_INSTALLATION.txt')
@(
    "COMMIT=$sha",
    'CONFIGURATION=Demo',
    'CONSTANTE=DEMO_BUILD',
    'PLATEFORME=Windows-x64',
    'PREREQUIS=Microsoft.NET.DesktopRuntime-8-x64',
    "INSTALLER_SHA256=$hash",
    'SIGNATURE=NON_SIGNE',
    'RECETTE_MANUELLE=NON_EFFECTUEE'
) | Set-Content -LiteralPath (Join-Path $output 'INSTALLATEUR-INFO.txt') -Encoding UTF8
Write-Output "Installateur candidat : $installer"
Write-Output "SHA-256 : $hash"
