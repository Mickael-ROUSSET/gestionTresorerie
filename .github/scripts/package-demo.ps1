param()

$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$bin = Join-Path $root 'gestionTresorerie/bin'
$exe = Get-ChildItem -Path $bin -Filter 'gestionTresorerie.exe' -File -Recurse |
    Where-Object { $_.FullName -match '[\\/]Demo[\\/]net8\.0-windows[\\/]gestionTresorerie\.exe$' } |
    Select-Object -First 1
if ($null -eq $exe) { throw "Exécutable de démonstration absent sous : $bin" }
$source = Split-Path -Path $exe.FullName -Parent
$artifacts = Join-Path $root 'artifacts'
$sha = (git -C $root rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $sha.Length -ne 40) { throw 'Commit Git introuvable.' }
$shortSha = $sha.Substring(0, 8)
$stage = Join-Path $artifacts ('gestionTresorerie-demo-' + $shortSha)
$zip = Join-Path $artifacts ('gestionTresorerie-demo-' + $shortSha + '.zip')

# Refuser les restes d'un conditionnement precedent, pour eviter toute fuite.
if (Test-Path -LiteralPath $stage) {
    throw "Dossier de conditionnement deja present : $stage. Utiliser un espace propre."
}
if (Test-Path -LiteralPath $zip) {
    throw "Archive deja presente : $zip. Refus de l'ecraser."
}

if (-not (Test-Path (Join-Path $source 'gestionTresorerie.exe'))) {
    throw "Exécutable de démonstration absent : $source"
}
New-Item -ItemType Directory -Path $stage -Force | Out-Null

# Liste blanche : aucun App.config, paramètre utilisateur, SQL, JSON de secret,
# document comptable ou fichier présent ailleurs dans le dépôt.
$files = @(Get-ChildItem -Path $source -Recurse -File | Where-Object {
    # Ne jamais embarquer d'autre programme executable que l'application de demonstration.
    $_.Name -eq 'gestionTresorerie.exe' -or
    $_.Extension -eq '.dll' -or
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
# Verification finale de tous les fichiers du dossier de staging.
$support = @('LIRE_AVANT_DEMARRER.md', 'LANCER-DEMONSTRATION.cmd', 'COMMIT-DEMONSTRATION.txt')
$unexpected = @(Get-ChildItem -LiteralPath $stage -Recurse -File | Where-Object {
    ($_.Name -notin $support) -and
    ($_.Name -ne 'gestionTresorerie.exe') -and
    ($_.Extension -ne '.dll') -and
    ($_.Name -notmatch '\.(deps|runtimeconfig)\.json$')
})
if ($unexpected.Count -gt 0) {
    throw ('Fichiers hors liste blanche : ' + ($unexpected.FullName -join ', '))
}
Compress-Archive -Path (Join-Path $stage '*') -DestinationPath $zip -CompressionLevel Optimal

# Verifier le ZIP final, et pas seulement le repertoire de staging.
# L'archive publiee doit rester autonome, tracable et sans autre executable.
Add-Type -AssemblyName System.IO.Compression
$archive = [System.IO.Compression.ZipFile]::OpenRead($zip)
try {
    $entries = @($archive.Entries | Where-Object {
        -not [string]::IsNullOrEmpty($_.Name)
    } | ForEach-Object { $_.FullName.Replace('\', '/') })
    $required = @(
        'gestionTresorerie.exe',
        'gestionTresorerie.deps.json',
        'gestionTresorerie.runtimeconfig.json',
        'LANCER-DEMONSTRATION.cmd',
        'LIRE_AVANT_DEMARRER.md',
        'COMMIT-DEMONSTRATION.txt'
    )
    foreach ($name in $required) {
        if ($name -notin $entries) { throw "Fichier requis absent du ZIP : $name" }
    }
    # Controler aussi le contenu du ZIP final : aucune entree hors liste blanche,
    # meme si le dossier de staging etait correct avant la compression.
    $seen = New-Object 'System.Collections.Generic.HashSet[string]' ([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($name in $entries) {
        if ($name.StartsWith('/') -or $name -match '(^|/)\.\.(/|$)' -or
            $name -match '^[A-Za-z]:' -or -not $seen.Add($name)) {
            throw "Entree interdite ou dupliquee dans le ZIP : $name"
        }
        $isSupport = $name -in $support
        $isExecutable = $name -ceq 'gestionTresorerie.exe'
        $isLibrary = $name -match '(?i)\.dll
}
finally {
    $archive.Dispose()
}
Write-Output "Archive verifiee : $($entries.Count) fichiers, executables limites a gestionTresorerie.exe."
Write-Output "Archive de démonstration candidate : $zip"
Write-Output "Commit : $sha"
Write-Output 'Contrôle graphique manuel : non effectué.'

        $isManifest = $name -match '(?i)\.(deps|runtimeconfig)\.json
}
finally {
    $archive.Dispose()
}
Write-Output "Archive verifiee : $($entries.Count) fichiers, executables limites a gestionTresorerie.exe."
Write-Output "Archive de démonstration candidate : $zip"
Write-Output "Commit : $sha"
Write-Output 'Contrôle graphique manuel : non effectué.'

        if (-not ($isSupport -or $isExecutable -or $isLibrary -or $isManifest)) {
            throw "Fichier hors liste blanche dans le ZIP : $name"
        }
    }
}
finally {
    $archive.Dispose()
}
Write-Output "Archive verifiee : $($entries.Count) fichiers, executables limites a gestionTresorerie.exe."
Write-Output "Archive de démonstration candidate : $zip"
Write-Output "Commit : $sha"
Write-Output 'Contrôle graphique manuel : non effectué.'
