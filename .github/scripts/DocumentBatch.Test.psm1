# TEST-only reusable batch engine. No Gmail, SQL or production access.
Set-StrictMode -Version Latest
function Invoke-FictitiousBatch([string]$InputPath, [string]$StatePath) {
    if (-not (Test-Path -LiteralPath $InputPath -PathType Container)) { throw 'Input directory missing' }
    # Reject overlapping directories before writing state, including nested paths.
    $inputFull = [IO.Path]::GetFullPath((Resolve-Path -LiteralPath $InputPath).Path).TrimEnd([char[]]@('\','/'))
    $stateFull = [IO.Path]::GetFullPath($StatePath).TrimEnd([char[]]@('\','/'))
    $comparison = [StringComparison]::OrdinalIgnoreCase
    $separator = [string][IO.Path]::DirectorySeparatorChar
    if ($stateFull.Equals($inputFull, $comparison) -or
        $stateFull.StartsWith($inputFull + $separator, $comparison) -or
        $inputFull.StartsWith($stateFull + $separator, $comparison)) {
        throw 'State and input directories must not overlap'
    }
    if (-not (Test-Path -LiteralPath $stateFull)) { [void](New-Item -ItemType Directory -Path $stateFull -Force) }
    $journalPath = Join-Path $stateFull 'journal.json'
    $journal = @{}
    if (Test-Path -LiteralPath $journalPath) {
        $old = Get-Content -LiteralPath $journalPath -Raw | ConvertFrom-Json
        # Fail closed on a corrupt journal: never silently discard history and overwrite it.
        foreach ($entry in @($old)) {
            if ($null -eq $entry -or
                $null -eq $entry.PSObject.Properties['hash'] -or
                $null -eq $entry.PSObject.Properties['status']) { throw 'invalid-journal-entry' }
            $key = [string]$entry.hash
            $value = [string]$entry.status
            if ($key -notmatch '^[A-Fa-f0-9]{64}$' -or $value -ne 'valid' -or $journal.ContainsKey($key)) {
                throw 'invalid-journal-entry'
            }
            $journal[$key] = $value
        }
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
                    # Type-check monetary and date fields before journaling; never accept prose as a number.
                    $transaction = $doc.donnees_extraites.transaction
                    foreach ($field in @('montant_numerique','montant_ttc')) {
                        $property = $transaction.PSObject.Properties[$field]
                        if ($null -ne $property -and $null -ne $property.Value) {
                            $amount = $property.Value
                            if ($amount -isnot [byte] -and $amount -isnot [int] -and
                                $amount -isnot [long] -and $amount -isnot [decimal] -and
                                $amount -isnot [double] -and $amount -isnot [single]) { throw 'invalid-amount-type' }
                            if ([double]$amount -lt 0 -or [double]::IsNaN([double]$amount) -or
                                [double]::IsInfinity([double]$amount)) { throw 'invalid-amount' }
                        }
                    }
                    foreach ($field in @('date_emission','date_facture','date_document')) {
                        $property = $transaction.PSObject.Properties[$field]
                        if ($null -ne $property -and $null -ne $property.Value) {
                            $dateText = $property.Value
                            if ($dateText -isnot [string] -or $dateText -cnotmatch '^\\d{4}-\\d{2}-\\d{2}
                    $allValues = @($doc.donnees_extraites.identite.PSObject.Properties.Value) +
                                 @($doc.donnees_extraites.transaction.PSObject.Properties.Value)
                    if (@($allValues | Where-Object { $null -ne $_ -and ([string]$_).Trim().Length -gt 0 }).Count -eq 0) {
                        throw 'empty-extraction'
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

Export-ModuleMember -Function Invoke-FictitiousBatch
) { throw 'invalid-date-format' }
                            try {
                                [void][datetime]::ParseExact($dateText, 'yyyy-MM-dd', [Globalization.CultureInfo]::InvariantCulture)
                            } catch { throw 'invalid-calendar-date' }
                        }
                    }
                    # A syntactically complete but entirely empty extraction cannot be classified as valid.
                    $allValues = @($doc.donnees_extraites.identite.PSObject.Properties.Value) +
                                 @($doc.donnees_extraites.transaction.PSObject.Properties.Value)
                    if (@($allValues | Where-Object { $null -ne $_ -and ([string]$_).Trim().Length -gt 0 }).Count -eq 0) {
                        throw 'empty-extraction'
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

Export-ModuleMember -Function Invoke-FictitiousBatch
