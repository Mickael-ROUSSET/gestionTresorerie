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

function Assert-SqlCatalogs([string]$Mode) {
    # Windows PowerShell fournit le client SQL .NET Framework utilisé par ce contrôle.
    $check = {
        param([string]$Mode)
        $ErrorActionPreference = 'Stop'
        $connection = [System.Data.SqlClient.SqlConnection]::new(
            'Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30')
        try {
            $connection.Open()
            $command = $connection.CreateCommand()
            foreach ($catalog in @('bddAgumaaaTest', 'bddAgumaaa')) {
                $connection.ChangeDatabase('master')
                $command.CommandText = "SELECT DB_ID(N'$catalog');"
                $exists = $command.ExecuteScalar() -ne [DBNull]::Value
                if ($Mode -eq 'absent') {
                    if ($exists) { throw "Catalogue préexistant sur le runner : $catalog." }
                    continue
                }
                if (-not $exists) { throw "Catalogue introuvable : $catalog." }
                $connection.ChangeDatabase($catalog)
                $expectedTables = @(
                    'Categorie', 'Communes', 'Comptes', 'Coordonnees', 'Documents', 'Evenements',
                    'Exercices', 'Films', 'Mouvements', 'ParticipantMdN', 'ReglesAffectationReleve',
                    'Seances', 'SousCategorie', 'Tarifs', 'Tiers', 'Type', 'TypeDoc', 'TypeMouvement')
                foreach ($table in $expectedTables) {
                    $command.CommandText = "SELECT OBJECT_ID(N'dbo.$table', N'U');"
                    if ($Mode -eq 'fixtures' -and $catalog -eq 'bddAgumaaaTest' -and $table -eq 'Communes') { continue }
                    if ($command.ExecuteScalar() -eq [DBNull]::Value) { throw "Table $catalog.dbo.$table absente." }
                }
                $command.CommandText = "SELECT COUNT(*) FROM sys.foreign_keys WHERE name=N'FK_Seances_Films' AND is_disabled=0 AND is_not_trusted=0;"
                if ($command.ExecuteScalar() -ne 1) { throw "Cle etrangere Films-Seances absente : $catalog." }
                $command.CommandText = "SELECT COUNT(*) FROM sys.default_constraints WHERE parent_object_id=OBJECT_ID(N'dbo.Seances');"
                if ($command.ExecuteScalar() -ne 3) { throw "Valeurs par defaut Seances absentes : $catalog." }
                # Le mode fixtures doit prouver que la migration reste à effectuer.
                # Sinon un scénario qui n'a pas réellement vieilli le schéma passerait à tort.
                if ($Mode -eq 'fixtures' -and $catalog -eq 'bddAgumaaaTest') {
                    $command.CommandText = "SELECT OBJECT_ID(N'dbo.Communes', N'U');"
                    if ($command.ExecuteScalar() -ne [DBNull]::Value) {
                        throw 'Fixture TEST invalide : table Communes encore présente avant migration.'
                    }
                    $command.CommandText = "SELECT COL_LENGTH(N'dbo.Documents', N'dateModif');"
                    if ($command.ExecuteScalar() -ne [DBNull]::Value) {
                        throw 'Fixture TEST invalide : dateModif encore présente avant migration.'
                    }
                }
                else {
                    $command.CommandText = "SELECT COL_LENGTH(N'dbo.Documents', N'dateModif');"
                    if ($command.ExecuteScalar() -eq [DBNull]::Value) { throw "Colonne dateModif absente : $catalog." }
                }
                if ($Mode -in @('verify', 'fixtures') -and $catalog -eq 'bddAgumaaa') {
                    # Les tiers fictifs du scénario TEST ne doivent jamais apparaître en PROD.
                    $command.CommandText = @'
SELECT COUNT(*) FROM dbo.Tiers
WHERE raisonSociale IN (N'CI-Tiers-Fictif-A', N'CI-Tiers-Fictif-B');
'@
                    if ($command.ExecuteScalar() -ne 0) { throw 'Isolation PROD violée : tiers fictifs TEST présents.' }
                    # Les coordonnées et le document sentinelle doivent également rester confinés à TEST,
                    # même si un défaut d'import n'a pas créé les lignes Tiers correspondantes.
                    $command.CommandText = @'
SELECT COUNT(*) FROM dbo.Coordonnees
WHERE Email IN (N'a@example.invalid',N'b@example.invalid',N'b-corrige@example.invalid');
'@
                    if ($command.ExecuteScalar() -ne 0) { throw 'Isolation PROD violée : coordonnées fictives TEST présentes.' }
                    $command.CommandText = "SELECT COUNT(*) FROM dbo.Documents WHERE cheminDoc=N'CI-Install-Sentinel';"
                    if ($command.ExecuteScalar() -ne 0) { throw 'Isolation PROD violée : document sentinelle TEST présent.' }
                }
                if ($Mode -in @('verify', 'fixtures') -and $catalog -eq 'bddAgumaaaTest') {
                    $command.CommandText = "SELECT COUNT(*) FROM dbo.Documents WHERE cheminDoc=N'CI-Install-Sentinel';"
                    if ($command.ExecuteScalar() -ne 1) { throw 'Ligne TEST perdue.' }
                    # Deux tiers et leurs coordonnées doivent rester distincts après réinstallation et désinstallation.
                    $command.CommandText = @'
SELECT COUNT(*) FROM dbo.Tiers t
JOIN dbo.Coordonnees c ON c.IdTiers=t.Id
WHERE (t.raisonSociale=N'CI-Tiers-Fictif-A' AND c.Email=N'a@example.invalid' AND c.Ville=N'Ville A')
   OR (t.raisonSociale=N'CI-Tiers-Fictif-B' AND c.Email=N'b-corrige@example.invalid' AND c.Ville=N'Ville B corrigée');
'@
                    if ($command.ExecuteScalar() -ne 2) { throw 'Coordonnées des deux tiers TEST perdues ou mélangées.' }
                    $command.CommandText = @'
SELECT COUNT(*) FROM dbo.Coordonnees c
JOIN dbo.Tiers t ON t.Id=c.IdTiers
WHERE t.raisonSociale IN (N'CI-Tiers-Fictif-A',N'CI-Tiers-Fictif-B');
'@
                    if ($command.ExecuteScalar() -ne 2) { throw 'Coordonnées TEST dupliquées.' }
                    # Le total de deux lignes ne suffit pas : exiger exactement un tiers et une coordonnée par nom.
                    $command.CommandText = @'
SELECT COUNT(*) FROM (
    SELECT t.raisonSociale
    FROM dbo.Tiers t
    LEFT JOIN dbo.Coordonnees c ON c.IdTiers=t.Id
    WHERE t.raisonSociale IN (N'CI-Tiers-Fictif-A',N'CI-Tiers-Fictif-B')
    GROUP BY t.raisonSociale
    HAVING COUNT(DISTINCT t.Id)=1 AND COUNT(c.IdTiers)=1
) AS valides;
'@
                    if ($command.ExecuteScalar() -ne 2) { throw 'Cardinalité tiers/coordonnées TEST invalide.' }
                    # Vérifier aussi l'intégrité référentielle de toutes les coordonnées,
                    # pas seulement celles qui apparaissent dans la jointure des deux tiers.
                    $command.CommandText = @'
SELECT COUNT(*) FROM dbo.Coordonnees c
LEFT JOIN dbo.Tiers t ON t.Id=c.IdTiers
WHERE t.Id IS NULL;
'@
                    if ($command.ExecuteScalar() -ne 0) { throw 'Coordonnées orphelines détectées dans le catalogue TEST.' }
                }
                if ($Mode -eq 'stamp') {
                    $command.CommandText = "EXEC sys.sp_addextendedproperty @name=N'CI-Install-Sentinel', @value=N'Conserver';"
                    [void]$command.ExecuteNonQuery()
                }
                $command.CommandText = "SELECT CONVERT(nvarchar(20),value) FROM sys.extended_properties WHERE class=0 AND name=N'CI-Install-Sentinel';"
                if ($command.ExecuteScalar() -cne 'Conserver') { throw "Sentinelle SQL perdue : $catalog." }
                if ($Mode -eq 'stamp' -and $catalog -eq 'bddAgumaaaTest') {
                    # Simuler une installation TEST ancienne sur le runner jetable.
                    # Données exclusivement fictives sur le runner Windows jetable, jamais en PROD.
                    $command.CommandText = @'
BEGIN TRY
    BEGIN TRANSACTION;
-- Refuser de contaminer le scénario avec un jeu de données déjà présent.
-- Une seconde exécution doit échouer sans modifier les données fictives existantes.
IF EXISTS (SELECT 1 FROM dbo.Tiers WHERE raisonSociale IN (N'CI-Tiers-Fictif-A', N'CI-Tiers-Fictif-B'))
    THROW 51004, 'Tiers fictifs deja presents avant initialisation', 1;
IF EXISTS (SELECT 1 FROM dbo.Documents WHERE cheminDoc=N'CI-Install-Sentinel')
    THROW 51005, 'Sentinelle document deja presente avant initialisation', 1;
DECLARE @a int, @b int;
INSERT dbo.Tiers (raisonSociale,dateCreation,dateModification) VALUES (N'CI-Tiers-Fictif-A',GETDATE(),GETDATE());
SET @a=CONVERT(int,SCOPE_IDENTITY());
INSERT dbo.Tiers (raisonSociale,dateCreation,dateModification) VALUES (N'CI-Tiers-Fictif-B',GETDATE(),GETDATE());
SET @b=CONVERT(int,SCOPE_IDENTITY());
IF @a=@b OR @a<=0 OR @b<=0 THROW 51001, 'Identifiants TEST invalides', 1;
INSERT dbo.Coordonnees (IdTiers,Email,Ville) VALUES
(@a,N'a@example.invalid',N'Ville A'),(@b,N'b@example.invalid',N'Ville B');
-- Corriger les coordonnées du seul tiers B, sans toucher au tiers A.
UPDATE dbo.Coordonnees SET Email=N'b-corrige@example.invalid', Ville=N'Ville B corrigée'
WHERE IdTiers=@b AND Email=N'b@example.invalid' AND Ville=N'Ville B';
IF @@ROWCOUNT<>1 THROW 51002, 'Correction du tiers B non unique', 1;
IF EXISTS (SELECT 1 FROM dbo.Coordonnees WHERE IdTiers=@a AND (Email<>N'a@example.invalid' OR Ville<>N'Ville A'))
    THROW 51003, 'Le tiers A a été modifié', 1;
INSERT dbo.Documents (dateDoc,cheminDoc) VALUES (GETDATE(),N'CI-Install-Sentinel');
DROP TABLE dbo.Communes;
ALTER TABLE dbo.Documents DROP COLUMN dateModif;
    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    THROW;
END CATCH;
'@
                    [void]$command.ExecuteNonQuery()
                }
            }
        }
        finally { $connection.Dispose() }
    }
    $source = "& {" + $check.ToString() + "} '" + $Mode + "'"
    $encoded = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($source))
    & powershell.exe -NoProfile -NonInteractive -EncodedCommand $encoded
    if ($LASTEXITCODE -ne 0) { throw "Contrôle des catalogues SQL échoué ($Mode)." }
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

Assert-SqlCatalogs 'absent'

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
    Assert-SqlCatalogs 'stamp'
    # Vérifier les fixtures avant migration, sans exiger encore la table Communes
    # ni la colonne Documents.dateModif supprimées volontairement par 'stamp'.
    Assert-SqlCatalogs 'fixtures'
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
    # Le simple lancement de la démonstration ne doit ni migrer implicitement
    # le catalogue TEST vieilli, ni altérer les deux tiers fictifs.
    Assert-SqlCatalogs 'fixtures'
    $process.CloseMainWindow() | Out-Null
    if (-not $process.WaitForExit(10000)) {
        # La fermeture graphique peut rester bloquée sur un runner sans session interactive.
        # Attendre aussi la fin de l'arrêt forcé avant de contrôler LocalDB.
        $process.Kill($true)
        if (-not $process.WaitForExit(10000)) {
            throw 'Le processus Windows ne se termine pas après arrêt forcé.'
        }
    }
    $process.Refresh()
    if (-not $process.HasExited) { throw 'Application encore active avant contrôle SQL après fermeture.' }
    $process.Dispose()
    $process = $null
    # Contrôler aussi l'état après fermeture : les handlers de sortie ne doivent
    # ni écrire dans TEST, ni déclencher une migration implicite.
    Assert-SqlCatalogs 'fixtures'

    $reinstallLog = Join-Path $logs '03-reinstall.log'
    $exitCode = Invoke-Setup $installer @(
        '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-', '/TASKS=desktopicon',
        "/LOG=""$reinstallLog""")
    if ($exitCode -ne 0) { throw "Réinstallation échouée, code $exitCode." }
    Assert-SqlCatalogs 'verify'
    Assert-Payload
    # Une seconde réinstallation sur un schéma déjà migré doit être idempotente :
    # conserver les deux tiers fictifs, leurs coordonnées et les sentinelles.
    $repeatInstallLog = Join-Path $logs '03b-repeat-reinstall.log'
    $exitCode = Invoke-Setup $installer @(
        '/VERYSILENT', '/SUPPRESSMSGBOXES', '/NORESTART', '/SP-', '/TASKS=desktopicon',
        "/LOG=`"$repeatInstallLog`"")
    if ($exitCode -ne 0) { throw "Seconde réinstallation échouée, code $exitCode." }
    Assert-SqlCatalogs 'verify'
    Assert-Payload
    # Reouvrir l'application apres les deux migrations : un demarrage sur schema
    # TEST deja migre ne doit ni modifier les tiers ni effacer leurs coordonnees.
    Start-Process -FilePath $desktopLink
    $process = $null
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        Start-Sleep -Milliseconds 500
        $appProcesses = @(Get-Process -Name gestionTresorerie -ErrorAction SilentlyContinue |
            Where-Object { $_.Path -ieq (Join-Path $installDir 'gestionTresorerie.exe') })
        if ($appProcesses.Count -gt 1) { throw 'Plusieurs instances apres migration.' }
        if ($appProcesses.Count -eq 1) {
            $process = $appProcesses[0]
            $process.Refresh()
            if ($process.MainWindowTitle -like '*DÉMONSTRATION HORS LIGNE*') { break }
        }
    }
    if ($null -eq $process -or $process.HasExited -or
        $process.MainWindowTitle -notlike '*DÉMONSTRATION HORS LIGNE*') {
        throw 'Reouverture de la demonstration apres migration echouee.'
    }
    Assert-SqlCatalogs 'verify'
    $process.CloseMainWindow() | Out-Null
    if (-not $process.WaitForExit(10000)) {
        $process.Kill($true)
        if (-not $process.WaitForExit(10000)) {
            throw 'Application non arretee apres reouverture.'
        }
    }
    $process.Refresh()
    if (-not $process.HasExited) { throw 'Application encore active apres reouverture.' }
    $process.Dispose()
    $process = $null
    Assert-SqlCatalogs 'verify'
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
    Assert-SqlCatalogs 'verify'
    $installed = $false
    Write-Output 'Création des deux catalogues SQL et conservation à la réinstallation et à la désinstallation vérifiées.'
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
