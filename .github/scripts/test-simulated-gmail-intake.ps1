# TEST ONLY: synthetic Gmail attachment manifest; never calls Gmail or writes production.
param([string]$ManifestPath, [string]$OutputDirectory, [switch]$SelfTest)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Import-SimulatedAttachments([string]$Manifest, [string]$Destination) {
    if (-not (Test-Path -LiteralPath $Manifest -PathType Leaf)) { throw 'manifest-missing' }
    $items = Get-Content -LiteralPath $Manifest -Raw | ConvertFrom-Json
    if ($null -eq $items -or $items -isnot [array]) { throw 'manifest-must-be-array' }
    $seen = @{}
    $planned = [Collections.Generic.List[object]]::new()
    foreach ($item in $items) {
        if ($null -eq $item -or $item -isnot [pscustomobject]) { throw 'invalid-attachment' }
        if (@($item.PSObject.Properties.Name | Where-Object { $_ -notin @('message_id','attachment_id','filename','content_base64','analysis') }).Count -gt 0) { throw 'unexpected-attachment-field' }
        foreach ($name in @('message_id','attachment_id','filename','content_base64')) {
            if ($null -eq $item.PSObject.Properties[$name] -or $item.PSObject.Properties[$name].Value -isnot [string] -or
                [string]::IsNullOrWhiteSpace($item.PSObject.Properties[$name].Value)) { throw 'invalid-attachment-field' }
        }
        if ($item.message_id -cnotmatch '^[A-Za-z0-9_-]{1,80}$' -or $item.attachment_id -cnotmatch '^[A-Za-z0-9_-]{1,80}$') { throw 'unsafe-identifier' }
        if ($item.filename -cnotmatch '^[^/\\:]+\.(pdf|png|jpg|jpeg)$' -or $item.filename -match '^\.\.') { throw 'unsafe-filename' }
        $key = $item.message_id + '_' + $item.attachment_id
        if ($seen.ContainsKey($key)) { throw 'duplicate-attachment-id' }
        $seen[$key] = $true
        try { $bytes = [Convert]::FromBase64String($item.content_base64) } catch { throw 'invalid-base64' }
        if ($bytes.Length -eq 0 -or $bytes.Length -gt 10485760) { throw 'invalid-attachment-size' }
        $analysisText = $null
        if ($null -ne $item.PSObject.Properties['analysis'] -and $null -ne $item.analysis) {
            if ($item.analysis -isnot [pscustomobject]) { throw 'invalid-analysis-fixture' }
            $analysisText = ConvertTo-Json -InputObject $item.analysis -Depth 20 -Compress
        }
        $extension = [IO.Path]::GetExtension($item.filename).ToLowerInvariant()
        $planned.Add([pscustomobject]@{ name=($key + $extension); bytes=$bytes; analysis=$analysisText })
    }
    if (-not (Test-Path -LiteralPath $Destination)) { [void](New-Item -ItemType Directory -Path $Destination -Force) }
    foreach ($entry in $planned) {
        $path = Join-Path $Destination $entry.name
        if (Test-Path -LiteralPath $path) {
            if ((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash -ne [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($entry.bytes))) { throw 'existing-attachment-conflict' }
        } else { [IO.File]::WriteAllBytes($path, $entry.bytes) }
        if ($null -ne $entry.analysis) {
            $sidecar = $path + '.analysis.json'
            if (-not (Test-Path -LiteralPath $sidecar)) { Set-Content -LiteralPath $sidecar -Value $entry.analysis -Encoding utf8 }
        }
    }
    return @($planned | ForEach-Object { $_.name })
}

if ($SelfTest) {
    $root = Join-Path ([IO.Path]::GetTempPath()) ('gt-mail-fixture-' + [guid]::NewGuid().ToString('N'))
    try {
        [void](New-Item -ItemType Directory -Path $root -Force)
        $manifest = Join-Path $root 'manifest.json'
        $dest = Join-Path $root 'attachments'
        $state = Join-Path $root 'state'
        $bytes = [Text.Encoding]::UTF8.GetBytes('synthetic-invoice')
        $analysis = @{ type_document='Facture'; donnees_extraites=@{ identite=@{ fournisseur_nom='Fictif' }; transaction=@{ numero_facture='F-001'; date_facture='2026-11-15'; montant_ttc=12.5 } } }
        $fixtures = @(
            @{message_id='mail01';attachment_id='att01';filename='invoice.pdf';content_base64=[Convert]::ToBase64String($bytes);analysis=$analysis},
            @{message_id='mail02';attachment_id='att02';filename='invoice.pdf';content_base64=[Convert]::ToBase64String($bytes);analysis=$analysis}
        )
        ConvertTo-Json -InputObject $fixtures -Depth 12 | Set-Content -LiteralPath $manifest -Encoding utf8
        $imported = @(Import-SimulatedAttachments $manifest $dest)
        if ($imported.Count -ne 2) { throw 'import-count' }
        Import-Module (Join-Path $PSScriptRoot 'DocumentBatch.Test.psm1') -Force
        $first = @(Invoke-FictitiousBatch $dest $state)
        if (@($first | Where-Object status -eq 'valid').Count -ne 1 -or @($first | Where-Object status -eq 'skipped').Count -ne 1) { throw 'batch-dedup-failed' }
        $second = @(Invoke-FictitiousBatch $dest $state)
        if (@($second | Where-Object status -eq 'skipped').Count -ne 2) { throw 'batch-resume-failed' }
        $bad = @(@{message_id='../escape';attachment_id='att03';filename='x.pdf';content_base64=[Convert]::ToBase64String($bytes)})
        ConvertTo-Json -InputObject $bad -Depth 8 | Set-Content -LiteralPath $manifest -Encoding utf8
        $rejected = $false
        try { [void](Import-SimulatedAttachments $manifest $dest) } catch { $rejected = $true }
        if (-not $rejected -or (Test-Path -LiteralPath (Join-Path $root 'escape'))) { throw 'unsafe-id-accepted' }
        Write-Output 'PASS: simulated Gmail manifest, deduplication, restart, unsafe identifier rejection'
    } finally { if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force } }
} else {
    if (-not $ManifestPath -or -not $OutputDirectory) { throw 'ManifestPath and OutputDirectory required' }
    Import-SimulatedAttachments $ManifestPath $OutputDirectory
}
