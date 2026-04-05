$response = Invoke-RestMethod -Uri "https://userapi.sepay.vn/v2/transactions?page=1&per_page=5&transaction_date_sort=desc" -Headers @{ "Authorization" = "Bearer RXJGIJ9YBGM8QOADHR0V53M46PBO0SUWASQNNEKF97XOIH3QXTZG8CXL7BFYKKT4" }
$tx = $response.data[0]
Write-Host "=== First Transaction Fields ==="
$tx | Get-Member -MemberType NoteProperty | ForEach-Object {
    $name = $_.Name
    $value = $tx.$name
    Write-Host "$name = $value"
}
