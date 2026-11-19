# TEST ONLY. Validate the last-run snapshot before a future Windows/SQL TEST adapter consumes it.
# This does not connect to SQL or Gmail and never reads extracted personal data.
param([string]$SnapshotPath, [string]$WorkspacePath, [switch]$SelfTest)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Read-TestDocumentSnapshot([string]$Snapshot, [string]$Workspace) {
    $state = [IO.Path]::GetFullPath((Join-Path $Workspace 'state'))
    $inbox = [IO.Path]::GetFullPath((Join-Path $Workspace 'inbox'))
    $source = [IO.Path]::GetFullPath($Snapshot)
    if (-not ([IO.Path]::GetDirectoryName($source)).Equals($state, [StringComparison]::OrdinalIgnoreCase) -or
        [IO.Path]::GetFileName($source).Equals('journal.json', [StringComparison]::OrdinalIgnoreCase)) {
        throw 'snapshot-outside-test-state'
    }
    if (-not (Test-Path -LiteralPath $source -PathType Leaf)) { throw 'snapshot-missing' }
    $data = Get-Content -LiteralPath $source -Raw | ConvertFrom-Json
    if ($null -eq $data.staged -or $null -eq $data.staged_count) { throw 'snapshot-invalid-contract' }
    $entries = @($data.staged)
    # JSON counts must be integral numbers, not strings or rounded fractional values.
    if ($data.staged_count -isnot [ValueType] -or $data.staged_count -is [bool]) { throw 'snapshot-invalid-count' }
    if ([decimal]$data.staged_count -lt 0 -or [decimal]$data.staged_count -ne [decimal]$entries.Count) { throw 'snapshot-count-mismatch' }
    $allowed = @('Cheque','Facture','FormulaireAdhesion','QuestionnaireMedical','Recu')
    $seen = @{}
    $seenNames = [Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    $projection = @(
        foreach ($entry in $entries) {
            if ($null -eq $entry -or @($entry.PSObject.Properties.Name | Where-Object { $_ -cnotin @('name','hash','type_document') }).Count -gt 0 -or
                @($entry.PSObject.Properties.Name).Count -ne 3 -or
                $null -eq $entry.name -or $null -eq $entry.hash -or $null -eq $entry.type_document) {
                throw 'snapshot-unexpected-fields'
            }
            if ($entry.name -isnot [string] -or $entry.hash -isnot [string] -or $entry.type_document -isnot [string]) { throw 'snapshot-invalid-field-type' }
            $name = [string]$entry.name
            $hash = [string]$entry.hash
            $kind = [string]$entry.type_document
            if ([string]::IsNullOrWhiteSpace($name) -or $name -ne [IO.Path]::GetFileName($name) -or
                $name -in @('.','..') -or $name.Contains([char]92) -or $name.Contains([char]47)) { throw 'snapshot-invalid-name' }
            if ($hash -cnotmatch '^[A-Fa-f0-9]{64}$' -or $kind -cnotin $allowed) { throw 'snapshot-invalid-entry' }
            if ($seen.ContainsKey($hash)) { throw 'snapshot-duplicate-hash' }
            if (-not $seenNames.Add($name)) { throw 'snapshot-duplicate-name' }
            $seen[$hash] = $true
            $file = Join-Path $inbox $name
            if (-not (Test-Path -LiteralPath $file -PathType Leaf)) { throw 'snapshot-file-missing' }
            if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash -cne $hash.ToUpperInvariant()) { throw 'snapshot-hash-mismatch' }
            [pscustomobject]@{ name=$name; hash=$hash; type_document=$kind }
        }
    )
    return $projection
}

if ($SelfTest) {
    $root = Join-Path ([IO.Path]::GetTempPath()) ('gt-adapter-' + [guid]::NewGuid().ToString('N'))
    try {
        $state = Join-Path $root 'state'
        $inbox = Join-Path $root 'inbox'
        [void](New-Item -ItemType Directory -Path $state,$inbox -Force)
        $file = Join-Path $inbox 'fiction.pdf'
        [IO.File]::WriteAllText($file, 'fiction-only')
        $hash = (Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash
        $snapshot = Join-Path $state 'last-run.json'
        $payload = @{ staged_count=1; staged=@(@{name='fiction.pdf';hash=$hash;type_document='Facture'}) }
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        if (@(Read-TestDocumentSnapshot $snapshot $root).Count -ne 1) { throw 'adapter-valid-fixture-failed' }
        $payload.staged[0].hash = '0' * 64
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        $rejected = $false
        try { [void](Read-TestDocumentSnapshot $snapshot $root) } catch { if ($_.Exception.Message -eq 'snapshot-hash-mismatch') { $rejected = $true } else { throw } }
        if (-not $rejected) { throw 'adapter-tamper-not-rejected' }
        $payload.staged[0].hash = $hash
        $payload.staged[0].name = '../fiction.pdf'
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        $rejected = $false
        try { [void](Read-TestDocumentSnapshot $snapshot $root) } catch { if ($_.Exception.Message -eq 'snapshot-invalid-name') { $rejected = $true } else { throw } }
        if (-not $rejected) { throw 'adapter-traversal-not-rejected' }
        # Reject a forged count and duplicate bytes before exposing an import projection.
        $payload.staged[0].name = 'fiction.pdf'
        $payload.staged_count = 2
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        $rejected = $false
        try { [void](Read-TestDocumentSnapshot $snapshot $root) } catch { if ($_.Exception.Message -eq 'snapshot-count-mismatch') { $rejected = $true } else { throw } }
        if (-not $rejected) { throw 'adapter-count-not-rejected' }
        # A fractional or string count must not be rounded/coerced into a valid count.
        # ConvertFrom-Json can represent a JSON numeric count as Double; integral values remain valid. 
        $payload.staged_count = 1.0
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        if (@(Read-TestDocumentSnapshot $snapshot $root).Count -ne 1) { throw 'adapter-double-integral-count-rejected' }
        $payload.staged_count = 1.5
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        $rejected = $false
        try { [void](Read-TestDocumentSnapshot $snapshot $root) } catch { if ($_.Exception.Message -eq 'snapshot-count-mismatch') { $rejected = $true } else { throw } }
        if (-not $rejected) { throw 'adapter-fractional-count-not-rejected' }
        $payload.staged_count = '1'
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        $rejected = $false
        try { [void](Read-TestDocumentSnapshot $snapshot $root) } catch { if ($_.Exception.Message -eq 'snapshot-invalid-count') { $rejected = $true } else { throw } }
        if (-not $rejected) { throw 'adapter-string-count-not-rejected' }
        $payload.staged_count = 2
        $payload.staged = @(
            @{name='fiction.pdf';hash=$hash;type_document='Facture'},
            @{name='fiction.pdf';hash=$hash;type_document='Recu'}
        )
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        $rejected = $false
        try { [void](Read-TestDocumentSnapshot $snapshot $root) } catch { if ($_.Exception.Message -eq 'snapshot-duplicate-hash') { $rejected = $true } else { throw } }
        if (-not $rejected) { throw 'adapter-duplicate-not-rejected' }
        # Windows paths are case-insensitive: two entries cannot target the same inbox file.
        $payload.staged_count = 2
        $payload.staged = @(
            @{name='fiction.pdf';hash=$hash;type_document='Facture'},
            @{name='FICTION.PDF';hash=('0' * 64);type_document='Recu'}
        )
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        $rejected = $false
        try { [void](Read-TestDocumentSnapshot $snapshot $root) } catch { if ($_.Exception.Message -eq 'snapshot-duplicate-name') { $rejected = $true } else { throw } }
        if (-not $rejected) { throw 'adapter-duplicate-name-not-rejected' }
        # Required metadata cannot be silently coerced from absent/null JSON fields.
        $payload.staged_count = 1
        $payload.staged = @(@{name='fiction.pdf';type_document='Facture'})
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        $rejected = $false
        try { [void](Read-TestDocumentSnapshot $snapshot $root) } catch { if ($_.Exception.Message -eq 'snapshot-unexpected-fields') { $rejected = $true } else { throw } }
        if (-not $rejected) { throw 'adapter-missing-hash-not-rejected' }
        $payload.staged = @(@{name='fiction.pdf';hash=$hash;type_document=$null})
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        $rejected = $false
        try { [void](Read-TestDocumentSnapshot $snapshot $root) } catch { if ($_.Exception.Message -eq 'snapshot-unexpected-fields') { $rejected = $true } else { throw } }
        if (-not $rejected) { throw 'adapter-null-type-not-rejected' }
        # Reject non-string metadata rather than silently coercing JSON numbers or booleans.
        $payload.staged = @(@{name=123;hash=$hash;type_document='Facture'})
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        $rejected = $false
        try { [void](Read-TestDocumentSnapshot $snapshot $root) } catch { if ($_.Exception.Message -eq 'snapshot-invalid-field-type') { $rejected = $true } else { throw } }
        if (-not $rejected) { throw 'adapter-numeric-name-not-rejected' }
        $payload.staged = @(@{name='fiction.pdf';hash=$hash;type_document=$true})
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        $rejected = $false
        try { [void](Read-TestDocumentSnapshot $snapshot $root) } catch { if ($_.Exception.Message -eq 'snapshot-invalid-field-type') { $rejected = $true } else { throw } }
        if (-not $rejected) { throw 'adapter-boolean-type-not-rejected' }
        # A zero-document replay must produce a valid empty projection.
        $payload.staged_count = 0
        $payload.staged = @()
        ConvertTo-Json -InputObject $payload -Depth 5 | Set-Content -LiteralPath $snapshot -Encoding utf8
        if (@(Read-TestDocumentSnapshot $snapshot $root).Count -ne 0) { throw 'adapter-empty-replay-failed' }
        Write-Output 'PASS: TEST snapshot adapter contract, content hash and traversal'
    } finally {
        if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
    }
} else {
    if (-not $SnapshotPath -or -not $WorkspacePath) { throw 'SnapshotPath and WorkspacePath required' }
    ConvertTo-Json -InputObject @(Read-TestDocumentSnapshot $SnapshotPath $WorkspacePath) -Depth 5
}
