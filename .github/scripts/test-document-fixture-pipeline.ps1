# TEST ONLY: orchestrates synthetic Gmail fixtures and the shared resumable document batch.
# No Gmail network, SQL connection, real accounting documents or production writes.
param([string]$ManifestPath, [string]$WorkspacePath, [switch]$SelfTest)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-FixturePipeline([string]$Manifest, [string]$Workspace) {
    if (-not (Test-Path -LiteralPath $Manifest -PathType Leaf)) { throw 'manifest-missing' }
    $manifestFull = [IO.Path]::GetFullPath((Resolve-Path -LiteralPath $Manifest).Path)
    $workspaceFull = [IO.Path]::GetFullPath($Workspace)
    # Never allow an intake to write its inbox/state directly at a volume root.
    if ($workspaceFull.TrimEnd([char[]]@([char]92,[char]47)).Equals(
        ([IO.Path]::GetPathRoot($workspaceFull)).TrimEnd([char[]]@([char]92,[char]47)),
        [StringComparison]::OrdinalIgnoreCase)) { throw 'workspace-volume-root-forbidden' }
    $sep = [string][IO.Path]::DirectorySeparatorChar
    if ($workspaceFull.Equals($manifestFull, [StringComparison]::OrdinalIgnoreCase) -or
        $manifestFull.StartsWith($workspaceFull.TrimEnd([char[]]@('\','/')) + $sep, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'workspace-overlaps-manifest'
    }
    $inbox = Join-Path $workspaceFull 'inbox'
    $state = Join-Path $workspaceFull 'state'
    # Import performs a full manifest preflight before writing attachments.
    [void](& (Join-Path $PSScriptRoot 'test-simulated-gmail-intake.ps1') -ManifestPath $manifestFull -OutputDirectory $inbox)
    Import-Module (Join-Path $PSScriptRoot 'DocumentBatch.Test.psm1') -Force
    $items = @(Invoke-FictitiousBatch $inbox $state)
    $staged = @(ConvertTo-FictitiousBatchImportManifest $items)
    return [pscustomobject]@{
        staged = $staged
        staged_count = $staged.Count
        total = $items.Count
        valid = @($items | Where-Object status -eq 'valid').Count
        skipped = @($items | Where-Object status -eq 'skipped').Count
        review = @($items | Where-Object status -eq 'review').Count
        documents = $items
    }
}

if ($SelfTest) {
    $root = Join-Path ([IO.Path]::GetTempPath()) ('gt-pipeline-' + [guid]::NewGuid().ToString('N'))
    try {
        [void](New-Item -ItemType Directory -Path $root -Force)
        $manifest = Join-Path $root 'manifest.json'
        $workspace = Join-Path $root 'workspace'
        $analysis = @{
            type_document='Facture'
            donnees_extraites=@{
                identite=@{fournisseur_nom='Fournisseur fictif'}
                transaction=@{numero_facture='F-0001';date_facture='2026-11-15';montant_ttc=14.5}
            }
        }
        $payload = @(
            @{message_id='synthetic01';attachment_id='a01';filename='facture.pdf';content_base64=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('fictive-facture'));analysis=$analysis},
            @{message_id='synthetic02';attachment_id='a02';filename='scan.pdf';content_base64=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('fictive-unknown'))},
            @{message_id='synthetic03';attachment_id='a03';filename='facture-copie.pdf';content_base64=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('fictive-facture'));analysis=$analysis}
        )
        ConvertTo-Json -InputObject $payload -Depth 12 | Set-Content -LiteralPath $manifest -Encoding utf8
        # A volume root is not a safe destination for synthetic attachments.
        $volumeRoot = [IO.Path]::GetPathRoot($workspace)
        $rootRejected = $false
        try { [void](Invoke-FixturePipeline $manifest $volumeRoot) } catch {
            if ($_.Exception.Message -eq 'workspace-volume-root-forbidden') { $rootRejected = $true } else { throw }
        }
        if (-not $rootRejected) { throw 'volume-root-not-rejected' }
        # Reject a workspace containing the manifest before any synthetic inbox/state is created.
        $overlapRejected = $false
        try { [void](Invoke-FixturePipeline $manifest $root) } catch {
            if ($_.Exception.Message -eq 'workspace-overlaps-manifest') { $overlapRejected = $true } else { throw }
        }
        if (-not $overlapRejected) { throw 'manifest-overlap-not-rejected' }
        if ((Test-Path -LiteralPath (Join-Path $root 'inbox')) -or
            (Test-Path -LiteralPath (Join-Path $root 'state'))) { throw 'manifest-overlap-created-files' }
        $first = Invoke-FixturePipeline $manifest $workspace
        if ($first.total -ne 3 -or $first.valid -ne 1 -or $first.review -ne 1 -or $first.skipped -ne 1 -or $first.staged_count -ne 1) { throw 'first-pass-counts' }
        $second = Invoke-FixturePipeline $manifest $workspace
        if ($second.total -ne 3 -or $second.skipped -ne 2 -or $second.review -ne 1 -or $second.valid -ne 0 -or $second.staged_count -ne 0) { throw 'resume-counts' }
        if (-not (Test-Path -LiteralPath (Join-Path $workspace 'state/journal.json'))) { throw 'missing-resume-journal' }
        # A review-only scan is retryable: adding a valid simulated engine result
        # on a later pass must promote it to valid without reprocessing the invoice.
        $payload[1].analysis = @{
            type_document='Facture'
            donnees_extraites=@{
                identite=@{fournisseur_nom='Fournisseur scan fictif'}
                transaction=@{numero_facture='F-0002';date_facture='2026-11-15';montant_ttc=7.25}
            }
        }
        ConvertTo-Json -InputObject $payload -Depth 12 | Set-Content -LiteralPath $manifest -Encoding utf8
        $recovered = Invoke-FixturePipeline $manifest $workspace
        if ($recovered.total -ne 3 -or $recovered.valid -ne 1 -or $recovered.review -ne 0 -or $recovered.skipped -ne 2 -or $recovered.staged_count -ne 1) { throw 'review-recovery-counts' }
        $final = Invoke-FixturePipeline $manifest $workspace
        if ($final.total -ne 3 -or $final.skipped -ne 3 -or $final.valid -ne 0 -or $final.review -ne 0 -or $final.staged_count -ne 0) { throw 'review-recovery-resume' }

        # Exercise all five supported document types through the same synthetic Gmail intake.
        # No OCR, network, SQL or real documents are involved.
        $allTypesManifest = Join-Path $root 'all-types.json'
        $allTypesWorkspace = Join-Path $root 'all-types-workspace'
        $cases = @(
            @{ kind='Cheque'; identity=@{emetteur_numero_cheque='00042';emetteur_du_cheque='Personne fictive';beneficiaire_nom='AGUMAAA fictive'}; transaction=@{date_emission='2026-11-15';montant_numerique=12.50} },
            @{ kind='Facture'; identity=@{fournisseur_nom='Entreprise fictive'}; transaction=@{numero_facture='F-0003';date_facture='2026-11-15';montant_ttc=22.50} },
            @{ kind='FormulaireAdhesion'; identity=@{nom='Fictif';prenom='Camille';adresse='Adresse fictive'}; transaction=@{date_document='2026-11-15'} },
            @{ kind='QuestionnaireMedical'; identity=@{nom_complet='Camille Fictif'}; transaction=@{date_document='2026-11-15'} },
            @{ kind='Recu'; identity=@{nom_complet='Camille Fictif'}; transaction=@{reference='R-0004';date_document='2026-11-15';montant_numerique=9.25} }
        )
        $allTypesPayload = @(
            for ($i = 0; $i -lt $cases.Count; $i++) {
                $case = $cases[$i]
                @{
                    message_id=('synthetic-type-' + $i)
                    attachment_id=('attachment-' + $i)
                    filename=('document-' + $i + '.pdf')
                    content_base64=[Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('fictional-content-' + $i))
                    analysis=@{
                        type_document=$case.kind
                        donnees_extraites=@{identite=$case.identity;transaction=$case.transaction}
                    }
                }
            }
        )
        ConvertTo-Json -InputObject $allTypesPayload -Depth 12 | Set-Content -LiteralPath $allTypesManifest -Encoding utf8
        $typedFirst = Invoke-FixturePipeline $allTypesManifest $allTypesWorkspace
        if ($typedFirst.total -ne 5 -or $typedFirst.valid -ne 5 -or $typedFirst.review -ne 0 -or $typedFirst.staged_count -ne 5 -or
            @($typedFirst.documents | Where-Object { $_.type_document -notin @('Cheque','Facture','FormulaireAdhesion','QuestionnaireMedical','Recu') }).Count -ne 0) {
            throw 'five-types-validation-failed'
        }
        foreach ($row in $typedFirst.staged) {
            if (@($row.PSObject.Properties.Name | Where-Object { $_ -cnotin @('name','hash','type_document') }).Count -ne 0) { throw 'staging-leaks-extracted-fields' }
            if ($row.hash -cnotmatch '^[A-Fa-f0-9]{64}$') { throw 'staging-invalid-hash' }
        }
        # Verify staging metadata against actual fictional attachment bytes.
        $expectedTypes = @{}
        for ($i = 0; $i -lt $cases.Count; $i++) {
            $expectedTypes['document-' + $i + '.pdf'] = $cases[$i].kind
        }
        foreach ($row in $typedFirst.staged) {
            if (-not $expectedTypes.ContainsKey([string]$row.name)) { throw 'staging-unexpected-filename' }
            if ($row.type_document -cne $expectedTypes[[string]$row.name]) { throw 'staging-type-mismatch' }
            $attachmentPath = Join-Path (Join-Path $allTypesWorkspace 'inbox') ([string]$row.name)
            if (-not (Test-Path -LiteralPath $attachmentPath -PathType Leaf)) { throw 'staging-file-missing' }
            $actualHash = (Get-FileHash -LiteralPath $attachmentPath -Algorithm SHA256).Hash
            if ($row.hash -cne $actualHash) { throw 'staging-content-hash-mismatch' }
        }
        $typedAgain = Invoke-FixturePipeline $allTypesManifest $allTypesWorkspace
        if ($typedAgain.total -ne 5 -or $typedAgain.skipped -ne 5 -or $typedAgain.valid -ne 0 -or $typedAgain.review -ne 0 -or $typedAgain.staged_count -ne 0) {
            throw 'five-types-resume-failed'
        }
        Write-Output 'PASS: synthetic intake -> typed validation -> review -> SHA256 resume'
    } finally {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
    }
} else {
    if (-not $ManifestPath -or -not $WorkspacePath) { throw 'ManifestPath and WorkspacePath required' }
    Invoke-FixturePipeline $ManifestPath $WorkspacePath | ConvertTo-Json -Depth 12
}
