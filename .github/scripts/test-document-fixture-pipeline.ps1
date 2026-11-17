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
    return [pscustomobject]@{
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
        if ($first.total -ne 3 -or $first.valid -ne 1 -or $first.review -ne 1 -or $first.skipped -ne 1) { throw 'first-pass-counts' }
        $second = Invoke-FixturePipeline $manifest $workspace
        if ($second.total -ne 3 -or $second.skipped -ne 2 -or $second.review -ne 1 -or $second.valid -ne 0) { throw 'resume-counts' }
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
        if ($recovered.total -ne 3 -or $recovered.valid -ne 1 -or $recovered.review -ne 0 -or $recovered.skipped -ne 2) { throw 'review-recovery-counts' }
        $final = Invoke-FixturePipeline $manifest $workspace
        if ($final.total -ne 3 -or $final.skipped -ne 3 -or $final.valid -ne 0 -or $final.review -ne 0) { throw 'review-recovery-resume' }

        Write-Output 'PASS: synthetic intake -> typed validation -> review -> SHA256 resume'
    } finally {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
    }
} else {
    if (-not $ManifestPath -or -not $WorkspacePath) { throw 'ManifestPath and WorkspacePath required' }
    Invoke-FixturePipeline $ManifestPath $WorkspacePath | ConvertTo-Json -Depth 12
}
