param(
    [string]$InputDirectory,
    [string]$StateDirectory,
    [switch]$SelfTest
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-FictitiousBatch([string]$InputPath, [string]$StatePath) {
    if (-not (Test-Path -LiteralPath $InputPath -PathType Container)) { throw 'Input directory missing' }
    if (-not (Test-Path -LiteralPath $StatePath)) { [void](New-Item -ItemType Directory -Path $StatePath -Force) }
    $inputFull = (Resolve-Path -LiteralPath $InputPath).Path
    $stateFull = (Resolve-Path -LiteralPath $StatePath).Path
    if ($stateFull.TrimEnd('\','/') -eq $inputFull.TrimEnd('\','/')) { throw 'State must be separate from input' }
    $journalPath = Join-Path $stateFull 'journal.json'
    $journal = @{}
    if (Test-Path -LiteralPath $journalPath) {
        $old = Get-Content -LiteralPath $journalPath -Raw | ConvertFrom-Json
        foreach ($entry in @($old)) { $journal[[string]$entry.hash] = [string]$entry.status }
    }
    $allowed = @{
        Cheque = @('emetteur_numero_cheque','emetteur_du_cheque','beneficiaire_nom','date_emission','montant_numerique')
        Facture = @('fournisseur_nom','numero_facture','date_facture','montant_ttc')
        FormulaireAdhesion = @('nom','prenom','adresse','date_document')
        QuestionnaireMedical = @('nom_complet','date_document')
        Recu = @('nom_complet','reference','date_document','montant_numerique')
    }
    $results = New-Object System.Collections.Generic.List[object]
    $files = @(Get-ChildItem -LiteralPath $inputFull -File | Where-Object { $_.Extension.ToLowerInvariant() -in @('.pdf','.png','.jpg','.jpeg') } | Sort-Object Name)
    foreach ($file in $files) {
        $hash = (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash
        $status = 'review'
        $reason = 'missing-or-invalid-engine-result'
        if ($journal.ContainsKey($hash) -and $journal[$hash] -eq 'valid') {
            $status = 'skipped'
            $reason = 'previously-validated'
        } elseif ($file.Length -eq 0) {
            $reason = 'empty-file'
        } else {
            # Simulated engine contract: sidecar JSON is TEST-only, never an OCR result.
            $sidecar = $file.FullName + '.analysis.json'
            if (Test-Path -LiteralPath $sidecar -PathType Leaf) {
                try {
                    $doc = Get-Content -LiteralPath $sidecar -Raw | ConvertFrom-Json
                    $kind = [string]$doc.type_document
                    if (-not $allowed.ContainsKey($kind)) { throw 'unknown-type' }
                    if ($null -eq $doc.donnees_extraites -or $null -eq $doc.donnees_extraites.identite -or $null -eq $doc.donnees_extraites.transaction) { throw 'missing-sections' }
                    $identityFields = @($doc.donnees_extraites.identite.PSObject.Properties.Name)
                    $transactionFields = @($doc.donnees_extraites.transaction.PSObject.Properties.Name)
                    if ($identityFields.Count -eq 0 -or $transactionFields.Count -eq 0) { throw 'missing-identity-or-transaction' }
                    $identityAllowed = @{
                        Cheque = @('emetteur_numero_cheque','emetteur_du_cheque','beneficiaire_nom')
                        Facture = @('fournisseur_nom')
                        FormulaireAdhesion = @('nom','prenom','adresse')
                        QuestionnaireMedical = @('nom_complet')
                        Recu = @('nom_complet')
                    }
                    # Every declared field must exist, even when its value is null (unreadable).
                    # A partial engine response must not be silently classified as complete.
                    $requiredIdentity = @($identityAllowed[$kind])
                    $requiredTransaction = @($allowed[$kind] | Where-Object { $_ -notin $requiredIdentity })
                    foreach ($field in $requiredIdentity) {
                        if ($field -notin $identityFields) { throw 'missing-identity-field' }
                    }
                    foreach ($field in $requiredTransaction) {
                        if ($field -notin $transactionFields) { throw 'missing-transaction-field' }
                    }
                    foreach ($field in $identityFields) {
                        if ($field -notin $identityAllowed[$kind]) { throw 'unexpected-identity-field' }
                    }
                    foreach ($field in $transactionFields) {
                        if ($field -notin $allowed[$kind] -or $field -in $identityAllowed[$kind]) { throw 'unexpected-transaction-field' }
                    }
                    # Reject unexpected top-level sections: the simulated engine must honor the same contract as the real engine.
                    if (@($doc.PSObject.Properties.Name | Where-Object { $_ -notin @('type_document','donnees_extraites') }).Count -gt 0) { throw 'unexpected-root-field' }
                    if (@($doc.donnees_extraites.PSObject.Properties.Name | Where-Object { $_ -notin @('identite','transaction') }).Count -gt 0) { throw 'unexpected-section' }
                    $status = 'valid'
                    $reason = 'simulated-engine-validated'
                    $journal[$hash] = 'valid'
                } catch {
                    $reason = 'invalid-analysis'
                }
            }
        }
        $results.Add([pscustomobject]@{ name=$file.Name; hash=$hash; status=$status; reason=$reason })
    }
    # Atomic replacement of the journal: an interruption before this point is retryable.
    $journalRows = @($journal.Keys | Sort-Object | ForEach-Object { [pscustomobject]@{hash=$_;status=$journal[$_]} })
    $temp = Join-Path $stateFull ('journal-' + [guid]::NewGuid().ToString('N') + '.tmp')
    try {
        ConvertTo-Json -InputObject $journalRows -Depth 4 | Set-Content -LiteralPath $temp -Encoding utf8
        Move-Item -LiteralPath $temp -Destination $journalPath -Force
    } finally { if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Force } }
    return $results.ToArray()
}

if ($SelfTest) {
    $root = Join-Path ([IO.Path]::GetTempPath()) ('gt-batch-' + [guid]::NewGuid().ToString('N'))
    $inputPath = Join-Path $root 'input'
    $statePath = Join-Path $root 'state'
    try {
        [void](New-Item -ItemType Directory -Path $inputPath -Force)
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
        $first = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($first | Where-Object status -eq 'valid').Count -ne 1 -or @($first | Where-Object status -eq 'skipped').Count -ne 1 -or @($first | Where-Object status -eq 'review').Count -ne 3) { throw 'First pass failed' }
        if (@($first | Where-Object { $_.name -eq 'partial.pdf' -and $_.reason -eq 'invalid-analysis' }).Count -ne 1) { throw 'Partial analysis was accepted' }
        $second = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($second | Where-Object status -eq 'skipped').Count -ne 2 -or @($second | Where-Object status -eq 'review').Count -ne 2) { throw 'Resume failed' }
        if (@($first | Where-Object { $_.name -eq 'wrong-section.pdf' -and $_.reason -eq 'invalid-analysis' }).Count -ne 1) { throw 'Wrong section was accepted' }
        # A previously rejected document must be retryable once the simulated analysis is corrected.
        Set-Content -LiteralPath (Join-Path $inputPath 'wrong-section.pdf.analysis.json') -Encoding utf8 -Value $json
        $third = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($third | Where-Object status -eq 'valid').Count -ne 1 -or @($third | Where-Object status -eq 'skipped').Count -ne 2 -or @($third | Where-Object status -eq 'review').Count -ne 2) { throw 'Corrected analysis was not retried' }
        $fourth = @(Invoke-FictitiousBatch $inputPath $statePath)
        if (@($fourth | Where-Object status -eq 'skipped').Count -ne 3 -or @($fourth | Where-Object status -eq 'review').Count -ne 1) { throw 'Corrected analysis was not persisted' }
        Write-Output 'PASS: fictitious batch, SHA256 duplicate, strict required fields, corrected analysis retry and restart'
    } finally { if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force } }
} else {
    if (-not $InputDirectory -or -not $StateDirectory) { throw 'InputDirectory and StateDirectory are required' }
    Invoke-FictitiousBatch $InputDirectory $StateDirectory | ConvertTo-Json -Depth 4
}
