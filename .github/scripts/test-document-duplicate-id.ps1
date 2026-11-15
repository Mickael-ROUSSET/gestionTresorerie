# TEST ONLY: reject duplicate Gmail attachment identities before writing any fixture.
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
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
    $rejected = $false
    try {
        [void](& (Join-Path $PSScriptRoot 'test-simulated-gmail-intake.ps1') -ManifestPath $manifest -OutputDirectory $dest)
    } catch {
        if ($_.Exception.Message -eq 'duplicate-attachment-id') { $rejected = $true } else { throw }
    }
    if (-not $rejected) { throw 'duplicate-id-not-rejected' }
    if (Test-Path -LiteralPath $dest) { throw 'duplicate-id-created-inbox' }
    # Two distinct IDs can generate the same on-disk name when underscores are permitted.
    $colliding = @(
        @{message_id='a_b';attachment_id='c';filename='first.pdf';content_base64=$bytes},
        @{message_id='a';attachment_id='b_c';filename='second.pdf';content_base64=$bytes}
    )
    ConvertTo-Json -InputObject $colliding -Depth 8 | Set-Content -LiteralPath $manifest -Encoding utf8
    $collisionRejected = $false
    try {
        [void](& (Join-Path $PSScriptRoot 'test-simulated-gmail-intake.ps1') -ManifestPath $manifest -OutputDirectory $dest)
    } catch {
        if ($_.Exception.Message -eq 'duplicate-output-filename') { $collisionRejected = $true } else { throw }
    }
    if (-not $collisionRejected) { throw 'output-filename-collision-not-rejected' }
    if (Test-Path -LiteralPath $dest) { throw 'output-filename-collision-created-inbox' }
    Write-Output 'PASS: duplicate identity and filename collisions rejected without partial writes'
} finally {
    if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}
