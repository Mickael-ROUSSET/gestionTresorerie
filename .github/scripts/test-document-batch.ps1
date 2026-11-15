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
        # A Gmail-like nested attachment with identical bytes is deduplicated across folders.
        $attachmentDir = Join-Path $inputPath 'gmail-fixtures'
        [void](New-Item -ItemType Directory -Path $attachmentDir -Force)
        Copy-Item -LiteralPath (Join-Path $inputPath 'a.pdf') -Destination (Join-Path $attachmentDir 'attachment.pdf')
        Set-Content -LiteralPath (Join-Path $attachmentDir 'attachment.pdf.analysis.json') -Value '{"type_document":"Facture","donnees_extraites":{"identite":{"fournisseur_nom":"Entreprise fictive"},"transaction":{"numero_facture":"F-001","date_facture":"2026-11-14","montant_ttc":12.50}}}' -Encoding utf8
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
        # Type validation: negative amounts, prose amounts and impossible dates remain in review.
        foreach ($case in @(
            @{ Name='negative'; Json='{"type_document":"Facture","donnees_extraites":{"identite":{"fournisseur_nom":"Fictif"},"transaction":{"numero_facture":"F-2","date_facture":"2026-11-14","montant_ttc":-5}}}' },
            @{ Name='text-amount'; Json='{"type_document":"Facture","donnees_extraites":{"identite":{"fournisseur_nom":"Fictif"},"transaction":{"numero_facture":"F-3","date_facture":"2026-11-14","montant_ttc":"12 euros"}}}' },
            @{ Name='bad-date'; Json='{"type_document":"Facture","donnees_extraites":{"identite":{"fournisseur_nom":"Fictif"},"transaction":{"numero_facture":"F-4","date_facture":"2026-02-30","montant_ttc":12}}}' }
        )) {
            $filePath = Join-Path $inputPath ($case.Name + '.pdf')
            [IO.File]::WriteAllBytes($filePath, [Text.Encoding]::UTF8.GetBytes('fictitious-' + $case.Name))
            Set-Content -LiteralPath ($filePath + '.analysis.json') -Encoding utf8 -Value $case.Json
        }
        $first = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($first | Where-Object status -eq 'valid').Count -ne 1 -or @($first | Where-Object status -eq 'skipped').Count -ne 2 -or @($first | Where-Object status -eq 'review').Count -ne 7) { throw 'First pass failed' }
        if (@($first | Where-Object { $_.name -eq (Join-Path 'gmail-fixtures' 'attachment.pdf') -and $_.status -eq 'skipped' }).Count -ne 1) { throw 'Nested attachment was not found or deduplicated' }
        if (@($first | Where-Object { $_.name -eq 'partial.pdf' -and $_.reason -eq 'invalid-analysis' }).Count -ne 1) { throw 'Partial analysis was accepted' }
        if (@($first | Where-Object { $_.name -eq 'blank.pdf' -and $_.reason -eq 'invalid-analysis' }).Count -ne 1) { throw 'Empty extraction was accepted' }
        foreach ($name in @('negative.pdf','text-amount.pdf','bad-date.pdf')) {
            if (@($first | Where-Object { $_.name -eq $name -and $_.reason -eq 'invalid-analysis' }).Count -ne 1) { throw "Invalid typed field was accepted: $name" }
        }
        # A retry with no newly valid documents must leave the existing journal untouched.
        $resumeJournal = Join-Path $statePath 'journal.json'
        $sentinelTime = [datetime]::SpecifyKind([datetime]'2020-01-02T03:04:05', [DateTimeKind]::Utc)
        [IO.File]::SetLastWriteTimeUtc($resumeJournal, $sentinelTime)
        $second = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($second | Where-Object status -eq 'skipped').Count -ne 3 -or @($second | Where-Object status -eq 'review').Count -ne 7) { throw 'Resume failed' }
        if ([IO.File]::GetLastWriteTimeUtc($resumeJournal) -ne $sentinelTime) { throw 'Unchanged journal was rewritten on resume' }
        if (@($first | Where-Object { $_.name -eq 'wrong-section.pdf' -and $_.reason -eq 'invalid-analysis' }).Count -ne 1) { throw 'Wrong section was accepted' }
        # A previously rejected document must be retryable once the simulated analysis is corrected.
        Set-Content -LiteralPath (Join-Path $inputPath 'wrong-section.pdf.analysis.json') -Encoding utf8 -Value $json
        $third = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($third | Where-Object status -eq 'valid').Count -ne 1 -or @($third | Where-Object status -eq 'skipped').Count -ne 3 -or @($third | Where-Object status -eq 'review').Count -ne 6) { throw 'Corrected analysis was not retried' }
        $fourth = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($fourth | Where-Object status -eq 'skipped').Count -ne 4 -or @($fourth | Where-Object status -eq 'review').Count -ne 6) { throw 'Corrected analysis was not persisted' }
        # Cheque identifiers must remain text: leading zeros are significant.
        $chequePath = Join-Path $inputPath 'cheque-leading-zero.pdf'
        [IO.File]::WriteAllBytes($chequePath, [Text.Encoding]::UTF8.GetBytes('fictitious-cheque-leading-zero'))
        $chequeJson = '{"type_document":"Cheque","donnees_extraites":{"identite":{"emetteur_numero_cheque":"00001234","emetteur_du_cheque":"Association fictive","beneficiaire_nom":"Tiers fictif"},"transaction":{"date_emission":"2026-11-14","montant_numerique":15.25}}}'
        Set-Content -LiteralPath ($chequePath + '.analysis.json') -Value $chequeJson -Encoding utf8
        $chequePass = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($chequePass | Where-Object { $_.name -eq 'cheque-leading-zero.pdf' -and $_.status -eq 'valid' }).Count -ne 1) { throw 'Cheque with text identifier was rejected' }
        $chequeResume = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($chequeResume | Where-Object { $_.name -eq 'cheque-leading-zero.pdf' -and $_.status -eq 'skipped' }).Count -ne 1) { throw 'Cheque was not resumed by SHA256' }
        Remove-Item -LiteralPath $chequePath, ($chequePath + '.analysis.json') -Force
        # Numeric cheque numbers and invoice references must not be silently coerced to text.
        foreach ($case in @(
            @{ Name='numeric-cheque'; Json='{"type_document":"Cheque","donnees_extraites":{"identite":{"emetteur_numero_cheque":1234,"emetteur_du_cheque":"Association fictive","beneficiaire_nom":"Tiers fictif"},"transaction":{"date_emission":"2026-11-14","montant_numerique":15.25}}}' },
            @{ Name='numeric-invoice'; Json='{"type_document":"Facture","donnees_extraites":{"identite":{"fournisseur_nom":"Entreprise fictive"},"transaction":{"numero_facture":1234,"date_facture":"2026-11-14","montant_ttc":12.50}}}' }
        )) {
            $fixture = Join-Path $inputPath ($case.Name + '.pdf')
            [IO.File]::WriteAllBytes($fixture, [Text.Encoding]::UTF8.GetBytes('fictitious-' + $case.Name))
            Set-Content -LiteralPath ($fixture + '.analysis.json') -Value $case.Json -Encoding utf8
            $attempt = @(Invoke-FictitiousBatch $inputPath $statePath)
            if (@($attempt | Where-Object { $_.name -eq ($case.Name + '.pdf') -and $_.status -eq 'review' -and $_.reason -eq 'invalid-analysis' }).Count -ne 1) {
                throw "Numeric textual identifier accepted: $($case.Name)"
            }
            Remove-Item -LiteralPath $fixture, ($fixture + '.analysis.json') -Force
        }
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
        # An unknown journal status must fail closed and preserve the recovery point.
        $unknownStatusJournal = $goodJournal -replace '"status"\s*:\s*"valid"', '"status":"pending"'
        if ($unknownStatusJournal -eq $goodJournal) { throw 'Unable to prepare unknown-status fixture' }
        Set-Content -LiteralPath $journalPath -Encoding utf8 -Value $unknownStatusJournal
        $unknownRejected = $false
        try { [void](Invoke-FictitiousBatch $inputPath $statePath) } catch { $unknownRejected = $true }
        if (-not $unknownRejected) { throw 'Unknown journal status was accepted' }
        if ((Get-Content -LiteralPath $journalPath -Raw).Trim() -ne $unknownStatusJournal.Trim()) { throw 'Unknown-status journal was overwritten' }
        Set-Content -LiteralPath $journalPath -Encoding utf8 -Value $goodJournal
        # A journal entry with an unexpected property must not be trusted or overwritten.
        $extraFieldJournal = $goodJournal -replace '"status"\s*:\s*"valid"', '"status":"valid","unexpected":"fixture"'
        if ($extraFieldJournal -eq $goodJournal) { throw 'Unable to prepare extra-field fixture' }
        Set-Content -LiteralPath $journalPath -Encoding utf8 -Value $extraFieldJournal
        $extraRejected = $false
        try { [void](Invoke-FictitiousBatch $inputPath $statePath) } catch { $extraRejected = $true }
        if (-not $extraRejected) { throw 'Unexpected journal property was accepted' }
        if ((Get-Content -LiteralPath $journalPath -Raw).Trim() -ne $extraFieldJournal.Trim()) { throw 'Extra-field journal was overwritten' }
        Set-Content -LiteralPath $journalPath -Encoding utf8 -Value $goodJournal
        $fifth = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($fifth | Where-Object status -eq 'skipped').Count -ne 4) { throw 'Recovery after restoring journal failed' }
        Write-Output 'PASS: fictitious batch, SHA256 duplicate, strict fields, retry, restart and corrupt-journal protection'
    } finally { if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force } }
} else {
    if (-not $InputDirectory -or -not $StateDirectory) { throw 'InputDirectory and StateDirectory are required' }
    Invoke-FictitiousBatch $InputDirectory $StateDirectory | ConvertTo-Json -Depth 4
}
