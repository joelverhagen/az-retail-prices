[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [int]$Buckets
)

$dir = Join-Path $PSScriptRoot "snapshot/$((Get-Date).ToUniversalTime().ToString("yyyy-MM-dd"))"
New-Item $dir -ItemType Directory -Force | Out-Null

0..($Buckets - 1) | ForEach-Object {
    Start-Job -ScriptBlock {
        & (Join-Path $args[0] "snapshot-bucket.ps1") -Buckets $args[1] -Bucket $args[2] -Destination $args[3]
    } -ArgumentList $PSScriptRoot, $Buckets, $_, $dir
}

Write-Host ""

try {
    while (Get-Job -State "Running") {
        Get-Job | Receive-Job
        Start-Sleep 1
    }
    Get-Job | Receive-Job
}
finally {
    Remove-Job *
}
