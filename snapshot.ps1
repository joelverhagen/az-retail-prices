[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [int]$Buckets
)

$stopwatch = [Diagnostics.StopWatch]::StartNew()

$dir = Join-Path $PSScriptRoot "snapshot"
if (Test-Path $dir) {
    $initialCount = @(Get-ChildItem $dir).Count
    Remove-Item $dir -Recurse -Force
} else {
    $initialCount = "?"
}
New-Item $dir -ItemType Directory | Out-Null

0..($Buckets - 1) | ForEach-Object {
    Start-Job -ScriptBlock {
        & (Join-Path $args[0] "snapshot-bucket.ps1") -Buckets $args[1] -Bucket $args[2] -Destination $args[3]
    } -ArgumentList $PSScriptRoot, $Buckets, $_, $dir
}

Write-Host ""

try {
    while (Get-Job -State "Running") {
        Write-Host "[$($stopwatch.Elapsed)] Files in snapshot dir: $(@(Get-ChildItem $dir).Count) / $initialCount"
        Get-Job | Receive-Job
        Start-Sleep 5
    }
    Get-Job | Receive-Job
}
finally {
    Remove-Job *
}
