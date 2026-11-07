param()
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
$sha = (git -C $root rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $sha -notmatch '^[0-9a-f]{40}$') { throw 'Commit Git introuvable.' }
$short = $sha.Substring(0, 8)
$installer = Join-Path $root "artifacts/installer/gestionTresorerie-demo-setup-$short.exe"
$payload = Join-Path $root "artifacts/gestionTresorerie-demo-$short"
$localData = [Environment]::GetFolderPath('LocalApplicationData')
$installDir = Join-Path $localData 'Programs/gestionTresorerie-Demo'
$testData = Join-Path $localData 'gestionTresorerie/TEST-hors-ligne'
$registryPath = 'Registry::HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Uninstall\gestionTresorerie.Demo_is1'
$desktopLink = Join-Path ([Environment]::GetFolderPath('DesktopDirectory')) 'gestionTresorerie - Demonstration.lnk'
$startGroup = Join-Path ([Environment]::GetFolderPath('Programs')) 'gestionTresorerie - Demonstration'
$startLink = Join-Path $startGroup 'gestionTresorerie - Demonstration.lnk'
$logs = Join-Path $root 'artifacts/installer-test-logs'
New-Item -ItemType Directory -Path $logs | Out-Null

function Invoke-Setup([string]$File, [string[]]$Arguments) {
    $child = Start-Process -FilePath $File -ArgumentList $Arguments -PassThru
    try {
        if (-not $child.WaitForExit(90000)) {
            $child.Kill($true)
            throw "Délai dépassé pour $([IO.Path]::GetFileName($File))."
        }
        return $child.ExitCode
    }
    finally { $child.Dispose() }
}

function Assert-Payload {
    foreach ($source in Get-ChildItem -LiteralPath $payload -Recurse -File) {
        $relative = [IO.Path]::GetRelativePath($payload, $source.FullName)
        $installedFile = Join-Path $installDir $relative
        if (-not (Test-Path -LiteralPath $installedFile -PathType Leaf)) {
            throw "Fichier installé absent : $relative"
        }
        if ((Get-FileHash -LiteralPath $source.FullName).Hash -cne
            (Get-FileHash -LiteralPath $installedFile).Hash) {
            throw "Octets installés différents du payload validé : $relative"
        }
    }
    $lines = @(Get-Content -LiteralPath (Join-Path $installDir 'COMMIT-DEMONSTRATION.txt'))
    if ($lines -cnotcontains "COMMIT=$sha" -or $lines -cnotcontains 'CONSTANTE=DEMO_BUILD') {
        throw 'Traçabilité installée incorrecte.'
    }
}

# Ce test s'exécute uniquement dans le runner Windows jetable.
foreach ($existing in @($installDir, $testData, $desktopLink, $startGroup, $registryPath)) {
    if (Test-Path -LiteralPath $existing) { throw "État préexistant, test arrêté : $existing" }
}
if (-not (Test-Path -LiteralPath $installer -PathType Leaf)) { throw 'Installateur absent.' }
$expectedHash = ((Get-Content -LiteralPath "$installer.sha256" -Raw).Trim() -split '\s+')[0]
if ((Get-FileHash -LiteralPath $installer).Hash -ine $expectedHash) { throw 'SHA-256 installateur incorrect.' }

$refusedDir = Join-Path $env:RUNNER_TEMP ('GT-install-refused-' + [Guid]::NewGuid().ToString('N'))
$rejectLog = Join-Path $logs '01-rejected-directory.log'
$exitCode = Invoke-Setup $installer @(
    '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-',
    "/DIR=""$refusedDir""", "/LOG=""$rejectLog""")
if ($exitCode -eq 0 -or (Test-Path -LiteralPath $refusedDir) -or
    (Test-Path -LiteralPath $installDir) -or (Test-Path -LiteralPath $registryPath)) {
    throw 'Le refus du dossier personnalisé ne fonctionne pas.'
}
Write-Output "Dossier personnalisé refusé avant installation (code $exitCode)."

# Ne jamais installer par-dessus un dossier non reconnu, meme dans le chemin impose.
# Le dossier et la sentinelle sont exclusivement crees dans ce runner jetable.
$foreignMarker = Join-Path $installDir 'CI-foreign-directory-sentinel.txt'
New-Item -ItemType Directory -Path $installDir | Out-Null
try {
    Set-Content -LiteralPath $foreignMarker -Value 'Dossier non DEMO_BUILD : ne pas modifier.' -Encoding UTF8
    $foreignHash = (Get-FileHash -LiteralPath $foreignMarker -Algorithm SHA256).Hash
    $foreignLog = Join-Path $logs '01b-rejected-unmarked-directory.log'
    $exitCode = Invoke-Setup $installer @(
        '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-',
        "/LOG=""$foreignLog""")
    if ($exitCode -ne 7 -or
        -not (Test-Path -LiteralPath $foreignLog -PathType Leaf) -or
        -not (Test-Path -LiteralPath $foreignMarker -PathType Leaf) -or
        (Get-FileHash -LiteralPath $foreignMarker -Algorithm SHA256).Hash -cne $foreignHash -or
        (Test-Path -LiteralPath (Join-Path $installDir 'gestionTresorerie.exe')) -or
        (Test-Path -LiteralPath $registryPath)) {
        throw 'Un dossier utilisateur non marque a ete modifie ou accepte.'
    }
    Write-Output "Dossier existant sans marqueur DEMO_BUILD refuse (code $exitCode)."
}
finally {
    if (Test-Path -LiteralPath $foreignMarker) {
        Remove-Item -LiteralPath $foreignMarker -Force
    }
    if ((Test-Path -LiteralPath $installDir) -and
        @(Get-ChildItem -LiteralPath $installDir -Force).Count -eq 0) {
        Remove-Item -LiteralPath $installDir
    }
}

$installed = $false
$process = $null
$testSentinel = Join-Path $testData 'CI-installation-sentinel.txt'
$userSentinel = Join-Path $installDir 'FICHIER_UTILISATEUR_TEST.txt'
$uninstaller = $null
try {
    $installLog = Join-Path $logs '02-install.log'
    $exitCode = Invoke-Setup $installer @(
        '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-', '/TASKS=desktopicon',
        "/LOG=""$installLog""")
    if ($exitCode -ne 0) { throw "Installation échouée, code $exitCode." }
    $installed = $true
    Assert-Payload
    if (-not (Test-Path -LiteralPath $registryPath)) { throw 'Entrée Applications installées absente.' }
    $registration = Get-ItemProperty -LiteralPath $registryPath
    if ($registration.DisplayVersion -ne '0.1.0') { throw 'Version de désinstallation incorrecte.' }
    foreach ($shortcutPath in @($desktopLink, $startLink)) {
        if (-not (Test-Path -LiteralPath $shortcutPath -PathType Leaf)) { throw "Raccourci absent : $shortcutPath" }
        $shell = New-Object -ComObject WScript.Shell
        $shortcut = $shell.CreateShortcut($shortcutPath)
        try {
            if ($shortcut.TargetPath -ine (Join-Path $installDir 'gestionTresorerie.exe') -or
                $shortcut.Arguments -ne '--demo' -or $shortcut.WorkingDirectory -ine $installDir) {
                throw 'Cible, arguments ou dossier de travail du raccourci incorrects.'
            }
        }
        finally {
            [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($shortcut)
            [void][Runtime.InteropServices.Marshal]::FinalReleaseComObject($shell)
        }
    }
    New-Item -ItemType Directory -Path $testData | Out-Null
    Set-Content -LiteralPath $testSentinel -Value 'Données TEST fictives à conserver.' -Encoding UTF8
    Set-Content -LiteralPath $userSentinel -Value 'Fichier utilisateur fictif à conserver.' -Encoding UTF8
    $testHash = (Get-FileHash -LiteralPath $testSentinel).Hash
    $userHash = (Get-FileHash -LiteralPath $userSentinel).Hash

    # Lancer le véritable raccourci, puis retrouver uniquement le binaire installé.
    Start-Process -FilePath $desktopLink
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        Start-Sleep -Milliseconds 500
        $appProcesses = @(Get-Process -Name gestionTresorerie -ErrorAction SilentlyContinue |
            Where-Object { $_.Path -ieq (Join-Path $installDir 'gestionTresorerie.exe') })
        if ($appProcesses.Count -gt 1) { throw 'Plusieurs instances installées sont apparues.' }
        if ($appProcesses.Count -eq 1) {
            $process = $appProcesses[0]
            $process.Refresh()
            if ($process.MainWindowTitle -like '*DÉMONSTRATION HORS LIGNE*') { break }
        }
    }
    if ($null -eq $process -or $process.HasExited -or
        $process.MainWindowTitle -notlike '*DÉMONSTRATION HORS LIGNE*') {
        throw 'Le raccourci ne démarre pas la démonstration installée.'
    }
    Write-Output "Raccourci exécuté et titre de démonstration confirmé."
    $process.CloseMainWindow() | Out-Null
    if (-not $process.WaitForExit(10000)) { $process.Kill($true) }
    $process.Dispose()
    $process = $null

    $reinstallLog = Join-Path $logs '03-reinstall.log'
    $exitCode = Invoke-Setup $installer @(
        '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-', '/TASKS=desktopicon',
        "/LOG=""$reinstallLog""")
    if ($exitCode -ne 0) { throw "Réinstallation échouée, code $exitCode." }
    Assert-Payload
    if ((Get-FileHash -LiteralPath $testSentinel).Hash -cne $testHash -or
        (Get-FileHash -LiteralPath $userSentinel).Hash -cne $userHash) {
        throw 'Une sentinelle a été modifiée pendant la réinstallation.'
    }
    $uninstallers = @(Get-ChildItem -LiteralPath $installDir -Filter 'unins*.exe' -File)
    if ($uninstallers.Count -ne 1) { throw 'Désinstalleur installé ambigu ou absent.' }
    $uninstaller = $uninstallers[0].FullName
    $uninstallLog = Join-Path $logs '04-uninstall.log'
    $exitCode = Invoke-Setup $uninstaller @(
        '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', "/LOG=""$uninstallLog""")
    if ($exitCode -ne 0) { throw "Désinstallation échouée, code $exitCode." }
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        if (-not (Test-Path -LiteralPath (Join-Path $installDir 'gestionTresorerie.exe')) -and
            -not (Test-Path -LiteralPath $registryPath)) { break }
        Start-Sleep -Milliseconds 500
    }
    $remainingPayload = @(Get-ChildItem -LiteralPath $payload -Recurse -File | Where-Object {
        $relative = [IO.Path]::GetRelativePath($payload, $_.FullName)
        Test-Path -LiteralPath (Join-Path $installDir $relative)
    })
    if ($remainingPayload.Count -gt 0 -or
        (Test-Path -LiteralPath $registryPath) -or
        (Test-Path -LiteralPath $desktopLink) -or (Test-Path -LiteralPath $startGroup)) {
        throw 'Fichiers applicatifs, entrée Windows ou raccourcis conservés après désinstallation.'
    }
    if ((Get-FileHash -LiteralPath $testSentinel).Hash -cne $testHash -or
        (Get-FileHash -LiteralPath $userSentinel).Hash -cne $userHash) {
        throw 'Une sentinelle a été supprimée ou modifiée pendant la désinstallation.'
    }
    $installed = $false
    Write-Output 'Installation, raccourcis, lancement, réinstallation et désinstallation vérifiés.'
    Write-Output 'Sessions TEST et fichier ajouté au programme conservés (sentinelles fictives).'
    Write-Output "Signature de la candidate : $((Get-AuthenticodeSignature -LiteralPath $installer).Status)"
    Write-Output "Recette interactive manuelle de l'assistant : non effectuée."
}
finally {
    if ($null -ne $process) {
        if (-not $process.HasExited) { $process.Kill($true) }
        $process.Dispose()
    }
    if ($installed) {
        $cleanup = @(Get-ChildItem -LiteralPath $installDir -Filter 'unins*.exe' -File -ErrorAction SilentlyContinue)
        if ($cleanup.Count -eq 1) {
            try { [void](Invoke-Setup $cleanup[0].FullName @('/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART')) }
            catch { Write-Warning "Nettoyage du runner impossible : $($_.Exception.Message)" }
        }
    }
    # Supprimer uniquement les sentinelles créées par ce test.
    foreach ($file in @($testSentinel, $userSentinel)) {
        if (Test-Path -LiteralPath $file) { Remove-Item -LiteralPath $file -Force }
    }
    foreach ($folder in @($testData, $installDir)) {
        if ((Test-Path -LiteralPath $folder) -and
            @(Get-ChildItem -LiteralPath $folder -Force).Count -eq 0) {
            Remove-Item -LiteralPath $folder
        }
    }
}
