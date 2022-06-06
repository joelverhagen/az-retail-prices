[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [int]$Buckets,
    
    [Parameter(Mandatory = $true)]
    [int]$Bucket,
    
    [Parameter(Mandatory = $true)]
    [string]$Destination
)

$url = "https://prices.azure.com/api/retail/prices?`$skip="
$page = 0
$utf8 = New-Object System.Text.UTF8Encoding $False

while ($true) {
    $path = Join-Path $Destination "page$page.json"

    if ((Test-Path $path) -or ($page % $Buckets -ne $Bucket) ) {
        $page += 1
        continue
    }

    $pageUrl = $url + (100 * $page)
    $response = Invoke-WebRequest $pageUrl
    $formatted = $response.Content | jq .
    $json = $formatted | ConvertFrom-Json 

    if ($json.Items) {
        [IO.File]::WriteAllLines($path, $formatted, $utf8)
        $page += 1
    }
    else {
        break
    }

}
