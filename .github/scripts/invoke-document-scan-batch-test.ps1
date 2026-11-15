# TEST-only standalone scan-folder batch entry point. No Gmail, SQL, OCR, or PROD writes.
param([string]$InputPath, [string]$StatePath, [switch]$SelfTest)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'DocumentBatch.Test.psm1') -Force

function Invoke-ScanBatch([string]$InputDirectory, [string]$StateDirectory) {
    if ([string]::IsNullOrWhiteSpace($InputDirectory) -or [string]::IsNullOrWhiteSpace($StateDirectory)) {
        throw 'InputPath and StatePath required'
    }
    $items = @(Invoke-FictitiousBatch $InputDirectory $StateDirectory)
    return [pscustomobject]@{
        total = $items.Count
        valid = @($items | Where-Object status -eq 'valid').Count
        review = @($items | Where-Object status -eq 'review').Count
        skipped = @($items | Where-Object status -eq 'skipped').Count
        documents = $items
    }
}

if ($SelfTest) {
    $root = Join-Path ([IO.Path]::GetTempPath()) ('gt-scan-cli-' + [guid]::NewGuid().ToString('N'))
    try {
        $inputDir = Join-Path $root 'scans'
        $stateDir = Join-Path $root 'state'
        [void](New-Item -ItemType Directory -Path $inputDir -Force)
        $invoice = Join-Path $inputDir 'invoice.pdf'
        $unknown = Join-Path $inputDir 'unknown.png'
        [IO.File]::WriteAllBytes($invoice, [Text.Encoding]::UTF8.GetBytes('synthetic-invoice'))
        [IO.File]::WriteAllBytes($unknown, [Text.Encoding]::UTF8.GetBytes('synthetic-unknown'))
        $analysis = @{
            type_document = 'Facture'
            donnees_extraites = @{
                identite = @{ fournisseur_nom = 'Fournisseur fictif' }
                transaction = @{ numero_facture = 'F-001'; date_facture = '2026-11-15'; montant_ttc = 12.50 }
            }
        }
        ConvertTo-Json -InputObject $analysis -Depth 12 | Set-Content -LiteralPath ($invoice + '.analysis.json') -Encoding utf8
        $first = Invoke-ScanBatch $inputDir $stateDir
        if ($first.total -ne 2 -or $first.valid -ne 1 -or $first.review -ne 1 -or $first.skipped -ne 0) { throw 'first-pass-counts' }
        $second = Invoke-ScanBatch $inputDir $stateDir
        if ($second.total -ne 2 -or $second.valid -ne 0 -or $second.review -ne 1 -or $second.skipped -ne 1) { throw 'resume-counts' }
        ConvertTo-Json -InputObject $analysis -Depth 12 | Set-Content -LiteralPath ($unknown + '.analysis.json') -Encoding utf8
        $third = Invoke-ScanBatch $inputDir $stateDir
        if ($third.total -ne 2 -or $third.valid -ne 1 -or $third.review -ne 0 -or $third.skipped -ne 1) { throw 'review-recovery-counts' }
        $fourth = Invoke-ScanBatch $inputDir $stateDir
        if ($fourth.total -ne 2 -or $fourth.skipped -ne 2) { throw 'final-resume-counts' }
        Write-Output 'PASS: standalone synthetic scan batch and resumable review'
    } finally {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
    }
} else {
    Invoke-ScanBatch $InputPath $StatePath | ConvertTo-Json -Depth 12
}
