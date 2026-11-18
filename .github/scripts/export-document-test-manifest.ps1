# TEST-only durable staging of validated fictitious documents; no SQL or Gmail access.
param([switch]$SelfTest)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'DocumentBatch.Test.psm1') -Force

function Export-FictitiousBatchManifest([object[]]$Results, [string]$Destination) {
    if ([string]::IsNullOrWhiteSpace($Destination)) { throw 'missing-manifest-destination' }
    # Validate the entire batch before touching the previous staging snapshot.
    $manifest = @(ConvertTo-FictitiousBatchImportManifest $Results)
    $fullPath = [IO.Path]::GetFullPath($Destination)
    $parent = [IO.Path]::GetDirectoryName($fullPath)
    if (-not (Test-Path -LiteralPath $parent -PathType Container)) { throw 'missing-manifest-directory' }
    if (Test-Path -LiteralPath $fullPath -PathType Container) { throw 'manifest-destination-is-directory' }
    $temporary = Join-Path $parent ('.manifest-' + [guid]::NewGuid().ToString('N') + '.tmp')
    $backup = Join-Path $parent ('.manifest-' + [guid]::NewGuid().ToString('N') + '.bak')
    try {
        $json = ConvertTo-Json -InputObject $manifest -Depth 4
        [IO.File]::WriteAllText($temporary, $json, [Text.UTF8Encoding]::new($false))
        # A rename with -Force can delete the previous snapshot before moving the new one.
        # Use a same-directory atomic filesystem replacement when the target exists.
        if ([IO.File]::Exists($fullPath)) {
            [IO.File]::Replace($temporary, $fullPath, $backup)
        } else {
            [IO.File]::Move($temporary, $fullPath)
        }
    } finally {
        if (Test-Path -LiteralPath $temporary) { Remove-Item -LiteralPath $temporary -Force }
        if (Test-Path -LiteralPath $backup) { Remove-Item -LiteralPath $backup -Force }
    }
    return $manifest.Count
}

if ($SelfTest) {
    $root = Join-Path ([IO.Path]::GetTempPath()) ('gt-manifest-' + [guid]::NewGuid().ToString('N'))
    try {
        [void](New-Item -ItemType Directory -Path $root)
        $target = Join-Path $root 'staging.json'
        $valid = [pscustomobject]@{ name='gmail-fixtures/invoice.pdf'; hash=('a' * 64); status='valid'; type_document='Facture'; secret='must-not-appear' }
        $review = [pscustomobject]@{ name='unreadable.pdf'; hash=('b' * 64); status='review'; type_document=$null }
        if ((Export-FictitiousBatchManifest @($valid, $review) $target) -ne 1) { throw 'valid-staging-count' }
        $saved = Get-Content -LiteralPath $target -Raw
        # Replacing an existing snapshot must work without retaining a backup or temp file.
        if ((Export-FictitiousBatchManifest @($valid) $target) -ne 1) { throw 'existing-staging-replace-count' }
        if ((Get-Content -LiteralPath $target -Raw) -cne $saved) { throw 'existing-staging-replace-content' }
        $rows = @(ConvertFrom-Json -InputObject $saved)
        if ($rows.Count -ne 1 -or $rows[0].name -cne $valid.name -or
            @($rows[0].PSObject.Properties.Name).Count -ne 3 -or $saved.Contains('secret') -or
            $saved.Contains('unreadable.pdf')) { throw 'staging-leaked-fields' }
        $invalid = [pscustomobject]@{ name='../escape.pdf'; hash=('c' * 64); status='valid'; type_document='Facture' }
        $rejected = $false
        try { [void](Export-FictitiousBatchManifest @($valid, $invalid) $target) } catch { $rejected = $true }
        if (-not $rejected -or (Get-Content -LiteralPath $target -Raw) -cne $saved) { throw 'invalid-batch-overwrote-staging' }
        if ((Export-FictitiousBatchManifest @($review) $target) -ne 0) { throw 'review-only-staging-count' }
        if (@(ConvertFrom-Json -InputObject (Get-Content -LiteralPath $target -Raw)).Count -ne 0) { throw 'review-only-staging-not-empty' }
        if (@(Get-ChildItem -LiteralPath $root -Filter '.manifest-*.tmp').Count -ne 0) { throw 'temporary-manifest-left-behind' }
        if (@(Get-ChildItem -LiteralPath $root -Filter '.manifest-*.bak').Count -ne 0) { throw 'backup-manifest-left-behind' }
        Write-Output 'Fictitious TEST manifest staging: PASS'
    } finally {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
    }
}

if ($SelfTest) {
    & (Join-Path $PSScriptRoot 'replay-document-test-manifest.ps1') -SelfTest
}
