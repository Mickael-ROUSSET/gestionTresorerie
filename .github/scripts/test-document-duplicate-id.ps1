# TEST ONLY: reject duplicate Gmail attachment identities before writing any fixture.
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# Run the negative fixture in an isolated PowerShell process: terminating script
# errors can otherwise bypass a caller's catch across script boundaries.
function Assert-RejectedFixture([string]$Manifest, [string]$Destination, [string]$ExpectedError) {
    $stdout = [IO.Path]::GetTempFileName()
    $stderr = [IO.Path]::GetTempFileName()
    try {
        $script = Join-Path $PSScriptRoot 'test-simulated-gmail-intake.ps1'
        $process = Start-Process -FilePath (Get-Process -Id $PID).Path -ArgumentList @('-NoProfile', '-NonInteractive', '-File', ('"' + $script + '"'), '-ManifestPath', ('"' + $Manifest + '"'), '-OutputDirectory', ('"' + $Destination + '"')) -Wait -PassThru -RedirectStandardOutput $stdout -RedirectStandardError $stderr
        $diagnostic = (Get-Content -LiteralPath $stdout -Raw) + (Get-Content -LiteralPath $stderr -Raw)
        if ($process.ExitCode -eq 0 -or -not $diagnostic.Contains($ExpectedError)) {
            throw ('expected-rejection-missing:' + $ExpectedError + '; exit=' + $process.ExitCode + '; output=' + $diagnostic)
        }
    } finally {
        Remove-Item -LiteralPath $stdout, $stderr -Force -ErrorAction SilentlyContinue
    }
}
$root = Join-Path ([IO.Path]::GetTempPath()) ('gt-duplicate-id-' + [guid]::NewGuid().ToString('N'))
try {
    [void](New-Item -ItemType Directory -Path $root -Force)
    $manifest = Join-Path $root 'manifest.json'
    $dest = Join-Path $root 'inbox'
    $bytes = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes('synthetic-content'))
    $entries = @(
        @{message_id='synthetic01';attachment_id='attachment01';filename='first.pdf';content_base64=$bytes},
        @{message_id='synthetic01';attachment_id='attachment01';filename='second.pdf';content_base64=$bytes}
    )
    ConvertTo-Json -InputObject $entries -Depth 8 | Set-Content -LiteralPath $manifest -Encoding utf8
    Assert-RejectedFixture $manifest $dest 'duplicate-attachment-id'
    if (Test-Path -LiteralPath $dest) { throw 'duplicate-id-created-inbox' }
    # Two distinct IDs can generate the same on-disk name when underscores are permitted.
    $colliding = @(
        @{message_id='a_b';attachment_id='c';filename='first.pdf';content_base64=$bytes},
        @{message_id='a';attachment_id='b_c';filename='second.pdf';content_base64=$bytes}
    )
    ConvertTo-Json -InputObject $colliding -Depth 8 | Set-Content -LiteralPath $manifest -Encoding utf8
    Assert-RejectedFixture $manifest $dest 'duplicate-output-filename'
    if (Test-Path -LiteralPath $dest) { throw 'output-filename-collision-created-inbox' }
    # A malformed later attachment must reject the entire batch before any write.
    $malformed = @(
        @{message_id='new';attachment_id='good';filename='good.pdf';content_base64=$bytes},
        @{message_id='new';attachment_id='bad';filename='bad.pdf';content_base64='not-base64!'}
    )
    ConvertTo-Json -InputObject $malformed -Depth 8 | Set-Content -LiteralPath $manifest -Encoding utf8
    Assert-RejectedFixture $manifest $dest 'invalid-base64'
    if (Test-Path -LiteralPath $dest) { throw 'invalid-base64-created-inbox' }
    # An orphan analysis sidecar must not be silently associated with a new file.
    [void](New-Item -ItemType Directory -Path $dest -Force)
    $orphanSidecar = Join-Path $dest 'orphan_att.pdf.analysis.json'
    Set-Content -LiteralPath $orphanSidecar -Value '{"type_document":"Facture"}' -Encoding utf8
    $freshName = Join-Path $dest 'fresh_att.pdf'
    $orphanEntries = @(
        @{message_id='fresh';attachment_id='att';filename='fresh.pdf';content_base64=$bytes},
        @{message_id='orphan';attachment_id='att';filename='orphan.pdf';content_base64=$bytes}
    )
    ConvertTo-Json -InputObject $orphanEntries -Depth 8 | Set-Content -LiteralPath $manifest -Encoding utf8
    Assert-RejectedFixture $manifest $dest 'orphan-analysis-sidecar'
    if (Test-Path -LiteralPath $freshName) { throw 'orphan-sidecar-partial-write' }
    if (Test-Path -LiteralPath (Join-Path $dest 'orphan_att.pdf')) { throw 'orphan-sidecar-created-file' }
    if (-not (Test-Path -LiteralPath $orphanSidecar)) { throw 'orphan-sidecar-destroyed' }
    Write-Output 'PASS: duplicate IDs, filename collisions, malformed base64 and orphan sidecars rejected without partial writes'
} finally {
    if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}
