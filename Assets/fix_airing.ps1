$path = "Bruno Gomez Stirparo Practica.unity"
$airingGuid = "d8b78681a0006415a824f4e6273244dd"
$mixerGuid = "9ecfa6bd77461a34cad40befce184a50"
$ambienteId = "3129876785944664619"

$content = Get-Content $path
$newContent = New-Object System.Collections.Generic.List[string]

$inAiring = $false
$hasDistance = $false
$hasMixer = $false

foreach($line in $content) {
    if ($line -match "m_SourcePrefab: {fileID: 100100000, guid: $airingGuid") {
        $inAiring = $true
        $hasDistance = $false
        $hasMixer = $false
    }
    
    if ($inAiring -and $line -match "--- !u!") {
        if (-not $hasDistance) {
            $newContent.Add("    - target: {fileID: 8219170, guid: $airingGuid, type: 3}")
            $newContent.Add("      propertyPath: MaxDistance")
            $newContent.Add("      value: 20")
            $newContent.Add("      objectReference: {fileID: 0}")
        }
        if (-not $hasMixer) {
            $newContent.Add("    - target: {fileID: 8219170, guid: $airingGuid, type: 3}")
            $newContent.Add("      propertyPath: m_OutputAudioMixerGroup")
            $newContent.Add("      value: ")
            $newContent.Add("      objectReference: {fileID: $ambienteId, guid: $mixerGuid, type: 2}")
        }
        $inAiring = $false
    }
    
    $newContent.Add($line)
    
    if ($inAiring) {
        if ($line -match "propertyPath: MaxDistance") { $hasDistance = $true }
        if ($line -match "propertyPath: m_OutputAudioMixerGroup") { $hasMixer = $true }
    }
}

[System.IO.File]::WriteAllLines($path, $newContent)
