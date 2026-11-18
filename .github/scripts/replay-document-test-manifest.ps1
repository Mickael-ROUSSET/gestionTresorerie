# TEST-only replay of validated fictitious staging. Sink is injected; no SQL or Gmail connection.
param([switch]$SelfTest)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
Import-Module (Join-Path $PSScriptRoot 'DocumentBatch.Test.psm1') -Force

function Invoke-FictitiousManifestReplay([string]$ManifestPath, [string]$CheckpointPath, [scriptblock]$Sink) {
    if (-not $Sink) { throw 'missing-test-sink' }
    if (-not [IO.File]::Exists($ManifestPath)) { throw 'missing-test-manifest' }
    $rows = @(ConvertFrom-Json -InputObject ([IO.File]::ReadAllText($ManifestPath)))
    # Revalidate the entire snapshot before invoking any sink callback.
    $validated = @(ConvertTo-FictitiousBatchImportManifest @($rows | ForEach-Object {
        [pscustomobject]@{ name=$_.name; hash=$_.hash; type_document=$_.type_document; status='valid' }
    }))
    if ($validated.Count -ne $rows.Count) { throw 'invalid-test-manifest' }
    $done = @{}
    if ([IO.File]::Exists($CheckpointPath)) {
        $previous = @(ConvertFrom-Json -InputObject ([IO.File]::ReadAllText($CheckpointPath)))
        foreach ($hash in $previous) {
            if ($hash -isnot [string] -or $hash -cnotmatch '^[a-fA-F0-9]{64}$') { throw 'invalid-test-checkpoint' }
            $key = $hash.ToLowerInvariant()
            if ($done.ContainsKey($key)) { throw 'duplicate-test-checkpoint' }
            $done[$key] = $true
        }
    }
    # Preflight the checkpoint destination before calling a sink with side effects.
    $checkpointFullPath = [IO.Path]::GetFullPath($CheckpointPath)
    $manifestFullPath = [IO.Path]::GetFullPath($ManifestPath)
    if ([string]::Equals($checkpointFullPath, $manifestFullPath, [StringComparison]::OrdinalIgnoreCase)) {
        throw 'checkpoint-overwrites-manifest'
    }
    $checkpointDirectory = [IO.Path]::GetDirectoryName($checkpointFullPath)
    if (-not [IO.Directory]::Exists($checkpointDirectory)) { throw 'missing-test-checkpoint-directory' }
    if ([IO.Directory]::Exists($checkpointFullPath)) { throw 'checkpoint-is-directory' }
    $processed = 0
    foreach ($item in $validated) {
        $key = ([string]$item.hash).ToLowerInvariant()
        if ($done.ContainsKey($key)) { continue }
        # The injected sink MUST be idempotent by SHA-256; a crash may occur before checkpointing.
        & $Sink $item
        $next = @($done.Keys) + @($key)
        $path = $checkpointFullPath
        $dir = $checkpointDirectory
        $tmp = Join-Path $dir ('.replay-' + [guid]::NewGuid().ToString('N') + '.tmp')
        $bak = Join-Path $dir ('.replay-' + [guid]::NewGuid().ToString('N') + '.bak')
        try {
            [IO.File]::WriteAllText($tmp, (ConvertTo-Json -InputObject @($next) -Compress), [Text.UTF8Encoding]::new($false))
            if ([IO.File]::Exists($path)) { [IO.File]::Replace($tmp, $path, $bak) }
            else { [IO.File]::Move($tmp, $path) }
        } finally {
            if ([IO.File]::Exists($tmp)) { [IO.File]::Delete($tmp) }
            if ([IO.File]::Exists($bak)) { [IO.File]::Delete($bak) }
        }
        $done[$key] = $true
        $processed++
    }
    return $processed
}

if ($SelfTest) {
    $root = Join-Path ([IO.Path]::GetTempPath()) ('gt-replay-' + [guid]::NewGuid().ToString('N'))
    [void](New-Item -ItemType Directory -Path $root)
    try {
        $manifest = Join-Path $root 'staging.json'
        $checkpoint = Join-Path $root 'checkpoint.json'
        $a = [pscustomobject]@{ name='fake/invoice.pdf'; hash=('a' * 64); type_document='Facture' }
        $b = [pscustomobject]@{ name='fake/receipt.pdf'; hash=('b' * 64); type_document='Recu' }
        [IO.File]::WriteAllText($manifest, (ConvertTo-Json -InputObject @($a,$b)))
        $script:seen = @()
        $script:failOnce = $true
        $sink = {
            param($item)
            if ($item.hash -eq ('b' * 64) -and $script:failOnce) {
                $script:failOnce = $false
                throw 'simulated-sink-failure'
            }
            $script:seen += $item.hash
        }
        $failed = $false
        try { [void](Invoke-FictitiousManifestReplay $manifest $checkpoint $sink) } catch {
            if ($_.Exception.Message -ne 'simulated-sink-failure') { throw }
            $failed = $true
        }
        if (-not $failed -or $script:seen.Count -ne 1) { throw 'replay-failure-not-isolated' }
        if ((Invoke-FictitiousManifestReplay $manifest $checkpoint $sink) -ne 1) { throw 'replay-restart-count' }
        if ((Invoke-FictitiousManifestReplay $manifest $checkpoint $sink) -ne 0 -or $script:seen.Count -ne 2) { throw 'replay-not-idempotent' }
        # A rejected checkpoint destination must never invoke the injected sink.
        $script:preflightCalls = 0
        $countSink = { param($item) $script:preflightCalls++ }
        $missingDir = Join-Path (Join-Path $root 'not-created') 'checkpoint.json'
        foreach ($badCheckpoint in @($manifest, $root, $missingDir)) {
            $rejectedPreflight = $false
            try { [void](Invoke-FictitiousManifestReplay $manifest $badCheckpoint $countSink) } catch { $rejectedPreflight = $true }
            if (-not $rejectedPreflight -or $script:preflightCalls -ne 0) { throw 'replay-preflight-invoked-sink' }
        }
        $before = [IO.File]::ReadAllText($checkpoint)
        [IO.File]::WriteAllText($manifest, '[{"name":"../escape.pdf","hash":"' + ('c' * 64) + '","type_document":"Facture"}]')
        $rejected = $false
        try { [void](Invoke-FictitiousManifestReplay $manifest $checkpoint $sink) } catch { $rejected = $true }
        if (-not $rejected -or [IO.File]::ReadAllText($checkpoint) -cne $before) { throw 'invalid-replay-mutated-checkpoint' }
        if (@(Get-ChildItem -LiteralPath $root -Filter '.replay-*').Count -ne 0) { throw 'replay-temp-files-remain' }
        Write-Output 'Fictitious manifest replay and restart: PASS'
    } finally { Remove-Item -LiteralPath $root -Recurse -Force }
}
