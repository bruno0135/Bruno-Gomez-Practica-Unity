$path = "c:\Users\bruno\Documents\GitHub\Bruno-Gomez-Practica-Unity\Assets\Bruno Gomez Stirparo Practica.unity"
$content = Get-Content $path
$newContent = New-Object System.Collections.Generic.List[string]
$inTargetScript = $false

foreach ($line in $content) {
    $currentLine = $line
    if ($line -match "m_Script: {fileID: 11500000, guid: 122f98fd16fbd37479b51e853df12b46") {
        $inTargetScript = $true
    }
    if ($inTargetScript) {
        if ($line -match "^  delay:") { $currentLine = "  delay: 60" }
        elseif ($line -match "^  volume:") { $currentLine = "  volume: 0.02" }
        elseif ($line -match "^  repeatEveryInterval:") { $currentLine = "  repeatEveryInterval: 1" }
        elseif ($line -match "^--- !u!") { $inTargetScript = $false }
    }
    $newContent.Add($currentLine)
}
[System.IO.File]::WriteAllLines($path, $newContent)
