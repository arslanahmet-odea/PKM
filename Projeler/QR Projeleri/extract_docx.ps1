param([string]$Path)
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::OpenRead($Path)
$entries = $zip.Entries | Where-Object { $_.FullName -match 'word/document\.xml$|ppt/slides/slide\d+\.xml$' } | Sort-Object FullName
foreach ($entry in $entries) {
    $reader = New-Object System.IO.StreamReader($entry.Open())
    $xml = $reader.ReadToEnd()
    $reader.Close()
    # Mark paragraph/slide boundaries
    $xml = $xml -replace '</w:p>', "`n"
    $xml = $xml -replace '</a:p>', "`n"
    $xml = $xml -replace '<w:tab[^>]*/>', "`t"
    $text = [regex]::Replace($xml, '<[^>]+>', '')
    $text = [System.Net.WebUtility]::HtmlDecode($text)
    Write-Output "===== $($entry.FullName) ====="
    Write-Output $text
}
$zip.Dispose()
