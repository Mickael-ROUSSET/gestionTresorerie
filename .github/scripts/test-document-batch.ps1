param(
    [string]$InputDirectory,
    [string]$StateDirectory,
    [switch]$SelfTest
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'DocumentBatch.Test.psm1') -Force

if ($SelfTest) {
    $root = Join-Path ([IO.Path]::GetTempPath()) ('gt-batch-' + [guid]::NewGuid().ToString('N'))
    $inputPath = Join-Path $root 'input'
    $statePath = Join-Path $root 'state'
    try {
        [void](New-Item -ItemType Directory -Path $inputPath -Force)
        # Never create a journal in the scan directory or its ancestors.
        $nestedState = Join-Path $inputPath 'state'
        $nestedRejected = $false
        try { [void](Invoke-FictitiousBatch $inputPath $nestedState) } catch { $nestedRejected = $true }
        if (-not $nestedRejected -or (Test-Path -LiteralPath $nestedState)) { throw 'Nested state directory was accepted or created' }
        $parentRejected = $false
        try { [void](Invoke-FictitiousBatch $inputPath $root) } catch { $parentRejected = $true }
        if (-not $parentRejected -or (Test-Path -LiteralPath (Join-Path $root 'journal.json'))) { throw 'Parent state directory was accepted or journal created' }
        [IO.File]::WriteAllBytes((Join-Path $inputPath 'a.pdf'), [byte[]](37,80,68,70,45,49))
        Copy-Item -LiteralPath (Join-Path $inputPath 'a.pdf') -Destination (Join-Path $inputPath 'b.pdf')
        [IO.File]::WriteAllBytes((Join-Path $inputPath 'invalid.pdf'), [byte[]]@())
        [IO.File]::WriteAllBytes((Join-Path $inputPath 'wrong-section.pdf'), [byte[]](37,80,68,70,45,50))
        Set-Content -LiteralPath (Join-Path $inputPath 'wrong-section.pdf.analysis.json') -Encoding utf8 -Value '{"type_document":"Facture","donnees_extraites":{"identite":{"montant_ttc":42},"transaction":{"fournisseur_nom":"Fictif"}}}'
        $json = '{"type_document":"Facture","donnees_extraites":{"identite":{"fournisseur_nom":"Entreprise fictive"},"transaction":{"numero_facture":"F-001","date_facture":"2026-11-14","montant_ttc":12.50}}}'
        Set-Content -LiteralPath (Join-Path $inputPath 'a.pdf.analysis.json') -Value $json -Encoding utf8
        Set-Content -LiteralPath (Join-Path $inputPath 'b.pdf.analysis.json') -Value $json -Encoding utf8
        # Partial structured responses are sent to review rather than archived as valid.
        [IO.File]::WriteAllBytes((Join-Path $inputPath 'partial.pdf'), [byte[]](37,80,68,70,45,51))
        Set-Content -LiteralPath (Join-Path $inputPath 'partial.pdf.analysis.json') -Encoding utf8 -Value '{"type_document":"Facture","donnees_extraites":{"identite":{"fournisseur_nom":"Entreprise fictive"},"transaction":{"montant_ttc":15}}}'
        # Complete JSON with only nulls is not a successful extraction.
        [IO.File]::WriteAllBytes((Join-Path $inputPath 'blank.pdf'), [byte[]](37,80,68,70,45,52))
        Set-Content -LiteralPath (Join-Path $inputPath 'blank.pdf.analysis.json') -Encoding utf8 -Value '{"type_document":"Facture","donnees_extraites":{"identite":{"fournisseur_nom":null},"transaction":{"numero_facture":null,"date_facture":null,"montant_ttc":null}}}'
        $first = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($first | Where-Object status -eq 'valid').Count -ne 1 -or @($first | Where-Object status -eq 'skipped').Count -ne 1 -or @($first | Where-Object status -eq 'review').Count -ne 4) { throw 'First pass failed' }
        if (@($first | Where-Object { $_.name -eq 'partial.pdf' -and $_.reason -eq 'invalid-analysis' }).Count -ne 1) { throw 'Partial analysis was accepted' }
        if (@($first | Where-Object { $_.name -eq 'blank.pdf' -and $_.reason -eq 'invalid-analysis' }).Count -ne 1) { throw 'Empty extraction was accepted' }
        $second = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($second | Where-Object status -eq 'skipped').Count -ne 2 -or @($second | Where-Object status -eq 'review').Count -ne 4) { throw 'Resume failed' }
        if (@($first | Where-Object { $_.name -eq 'wrong-section.pdf' -and $_.reason -eq 'invalid-analysis' }).Count -ne 1) { throw 'Wrong section was accepted' }
        # A previously rejected document must be retryable once the simulated analysis is corrected.
        Set-Content -LiteralPath (Join-Path $inputPath 'wrong-section.pdf.analysis.json') -Encoding utf8 -Value $json
        $third = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($third | Where-Object status -eq 'valid').Count -ne 1 -or @($third | Where-Object status -eq 'skipped').Count -ne 2 -or @($third | Where-Object status -eq 'review').Count -ne 3) { throw 'Corrected analysis was not retried' }
        $fourth = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($fourth | Where-Object status -eq 'skipped').Count -ne 3 -or @($fourth | Where-Object status -eq 'review').Count -ne 3) { throw 'Corrected analysis was not persisted' }
        # A truncated journal must stop processing without destroying the recovery point.
        $journalPath = Join-Path $statePath 'journal.json'
        $goodJournal = Get-Content -LiteralPath $journalPath -Raw
        $badJournal = '[{"hash":"broken","status":"valid"}]'
        Set-Content -LiteralPath $journalPath -Encoding utf8 -Value $badJournal
        $rejected = $false
        try { [void](Invoke-FictitiousBatch $inputPath $statePath) } catch { $rejected = $true }
        if (-not $rejected) { throw 'Corrupt journal was accepted' }
        if ((Get-Content -LiteralPath $journalPath -Raw).Trim() -ne $badJournal) { throw 'Corrupt journal was overwritten' }
        Set-Content -LiteralPath $journalPath -Encoding utf8 -Value $goodJournal
        $fifth = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($fifth | Where-Object status -eq 'skipped').Count -ne 3) { throw 'Recovery after restoring journal failed' }
        Write-Output 'PASS: fictitious batch, SHA256 duplicate, strict fields, retry, restart and corrupt-journal protection'
    } finally { if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force } }
} else {
    if (-not $InputDirectory -or -not $StateDirectory) { throw 'InputDirectory and StateDirectory are required' }
    Invoke-FictitiousBatch $InputDirectory $StateDirectory | ConvertTo-Json -Depth 4
}
