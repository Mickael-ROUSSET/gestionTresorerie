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
# Préparer un moteur privé pour l’application sans installer un programme sur le runner.
$ocr = Join-Path $root 'artifacts/ocr'
$ocrDownload = Join-Path $root 'artifacts/tesseract-5.5.3.exe'
$ocrExtract = Join-Path $root 'artifacts/tesseract-extract'
$ocrUrl = 'https://github.com/tesseract-ocr/tesseract/releases/download/5.5.3/tesseract-ocr-w64-setup-5.5.3.20260724.exe'
Invoke-WebRequest -Uri $ocrUrl -OutFile $ocrDownload
$ocrSha256 = 'bee9e3434bd94fd65387d9be28cd467a41f61b1275383b55b0f59a1331270ae4'
if ((Get-FileHash -LiteralPath $ocrDownload -Algorithm SHA256).Hash -ine $ocrSha256) {
    throw 'Empreinte SHA-256 de Tesseract inattendue.'
}
$sevenZip = Join-Path $env:ProgramFiles '7-Zip/7z.exe'
& $sevenZip x $ocrDownload "-o$ocrExtract" -y | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'Extraction de Tesseract impossible.' }
$engine = @(Get-ChildItem -LiteralPath $ocrExtract -Recurse -File -Filter 'tesseract.exe')
if ($engine.Count -ne 1) { throw 'Exécutable Tesseract introuvable ou ambigu.' }
$tessdata = Join-Path $ocr 'tessdata'
New-Item -ItemType Directory -Path $tessdata -Force | Out-Null
Copy-Item -LiteralPath $engine[0].FullName -Destination $ocr
Get-ChildItem -LiteralPath $engine[0].DirectoryName -File -Filter '*.dll' |
    Copy-Item -Destination $ocr
# Conserver les avis de licence distribués avec le moteur et ses dépendances.
Get-ChildItem -LiteralPath $engine[0].DirectoryName -File |
    Where-Object { $_.Name -match '^(COPYING|LICENSE|NOTICE|AUTHORS|README)' } |
    Copy-Item -Destination $ocr
$modelBase = 'https://raw.githubusercontent.com/tesseract-ocr/tessdata_fast/4.1.0'
$model = Join-Path $tessdata 'fra.traineddata'
Invoke-WebRequest -Uri "$modelBase/fra.traineddata" -OutFile $model
$modelBlob = git -C $root hash-object --no-filters -- $model
if ($LASTEXITCODE -ne 0 -or $modelBlob -cne 'd9e2b2160be0d1ca3b8f1bf2730fae476ef3b4a6') {
    throw 'Empreinte du modèle français de Tesseract inattendue.'
}
Invoke-WebRequest -Uri "$modelBase/LICENSE" -OutFile (Join-Path $tessdata 'LICENSE.txt')
$languages = & (Join-Path $ocr 'tesseract.exe') --tessdata-dir $tessdata --list-langs
if ($LASTEXITCODE -ne 0 -or $languages -cnotcontains 'fra') {
    throw 'Le moteur OCR préparé ne charge pas la langue française.'
}

$output = Join-Path $root 'artifacts/installer'
if (Test-Path -LiteralPath $output) { throw 'Dossier installateur déjà présent. Utiliser un espace propre.' }
New-Item -ItemType Directory -Path $output | Out-Null
Write-Output "Compilateur Inno Setup : $((Get-Item -LiteralPath $compiler).VersionInfo.FileVersion)"
& $compiler "/DPayloadDir=$payload" "/DBuildSha=$sha" "/DShortSha=$short" "/DOutputDir=$output" "/DSourceRoot=$root" "/DOcrDir=$ocr" (Join-Path $root 'installer/demo.iss')
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
