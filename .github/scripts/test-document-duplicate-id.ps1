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
    Write-Output 'PASS: duplicate attachment identity rejected without partial writes'
} finally {
    if (Test-Path -LiteralPath $root) { Remove-Item -LiteralPath $root -Recurse -Force }
}
