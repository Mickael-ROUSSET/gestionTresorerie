param()

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$source = Join-Path $root 'gestionTresorerie/bin/Demo/net8.0-windows'
$artifacts = Join-Path $root 'artifacts'
$sha = (git -C $root rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $sha.Length -ne 40) { throw 'Commit Git introuvable.' }
$shortSha = $sha.Substring(0, 8)
$stage = Join-Path $artifacts ('gestionTresorerie-demo-' + $shortSha)
$zip = Join-Path $artifacts ('gestionTresorerie-demo-' + $shortSha + '.zip')

if (-not (Test-Path (Join-Path $source 'gestionTresorerie.exe'))) {
    throw "Exécutable de démonstration absent : $source"
}
New-Item -ItemType Directory -Path $stage -Force | Out-Null

# Liste blanche : aucun App.config, paramètre utilisateur, SQL, JSON de secret,
# document comptable ou fichier présent ailleurs dans le dépôt.
$files = @(Get-ChildItem -Path $source -Recurse -File | Where-Object {
    $_.Extension -in @('.exe', '.dll') -or
    $_.Name -match '\.(deps|runtimeconfig)\.json$'
})
if ($files.Count -lt 3) { throw 'Dépendances insuffisantes pour préparer une archive.' }
foreach ($file in $files) {
    $relative = $file.FullName.Substring($source.Length).TrimStart('\', '/')
    $target = Join-Path $stage $relative
    $parent = Split-Path $target -Parent
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
    Copy-Item -LiteralPath $file.FullName -Destination $target -Force
}
Copy-Item -LiteralPath (Join-Path $root 'docs/demo-windows.md') -Destination (Join-Path $stage 'LIRE_AVANT_DEMARRER.md')
@"
@echo off
cd /d "%~dp0"
start "" "%~dp0gestionTresorerie.exe" --demo
"@ | Set-Content -LiteralPath (Join-Path $stage 'LANCER-DEMONSTRATION.cmd') -Encoding ASCII
("COMMIT=" + $sha + [Environment]::NewLine +
 "CONFIGURATION=Demo" + [Environment]::NewLine +
 "CONSTANTE=DEMO_BUILD" + [Environment]::NewLine +
 "CONTROLE_GRAPHIQUE=NON_EFFECTUE" + [Environment]::NewLine) |
    Set-Content -LiteralPath (Join-Path $stage 'COMMIT-DEMONSTRATION.txt') -Encoding UTF8

$unsafe = @(Get-ChildItem -Path $stage -Recurse -File | Where-Object {
    $_.Name -match '(?i)(client_secret|\.config$|\.mdf$|\.pfx$|\.key$|\.env$)'
})
if ($unsafe.Count -gt 0) { throw ('Fichiers interdits : ' + ($unsafe.Name -join ', ')) }
if (Test-Path $zip) { Remove-Item $zip -Force }
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -CompressionLevel Optimal
Write-Output "Archive de démonstration candidate : $zip"
Write-Output "Commit : $sha"
Write-Output 'Contrôle graphique manuel : non effectué.'
