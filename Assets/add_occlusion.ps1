$path = "c:\Users\bruno\Documents\GitHub\Bruno-Gomez-Practica-Unity\Assets\Bruno Gomez Stirparo Practica.unity"
$content = Get-Content $path
$newContent = New-Object System.Collections.Generic.List[string]

$occlusionGuid = "c6307c27766fbe64b9bdbd23d8382e33"
$targetScripts = @() # List of GameObjects that need occlusion

# Step 1: Find TimedSoundTrigger GameObjects
$inTimedScript = $false
$currentGO = ""
foreach ($line in $content) {
    if ($line -match "m_GameObject: {fileID: (\d+)}") {
        $currentGO = $matches[1]
    }
    if ($line -match "m_Script: {fileID: 11500000, guid: 122f98fd16fbd37479b51e853df12b46") {
        $targetScripts += $currentGO
    }
}

# Step 2: Add component references and append definitions
$newComponentIds = @{}
foreach ($goId in $targetScripts) {
    $newComponentIds[$goId] = Get-Random -Minimum 2000000000 -Maximum 2147483647
}

$inGameObject = ""
foreach ($line in $content) {
    if ($line -match "--- !u!1 &(\d+)") {
        $inGameObject = $matches[1]
    }
    
    $newContent.Add($line)
    
    if ($line -match "m_Component:" -and $targetScripts -contains $inGameObject) {
        $newId = $newComponentIds[$inGameObject]
        $newContent.Add("  - component: {fileID: $newId}")
    }
}

# Step 3: Append component definitions
foreach ($goId in $targetScripts) {
    $compId = $newComponentIds[$goId]
    $newContent.Add("--- !u!114 &$compId")
    $newContent.Add("MonoBehaviour:")
    $newContent.Add("  m_ObjectHideFlags: 0")
    $newContent.Add("  m_CorrespondingSourceObject: {fileID: 0}")
    $newContent.Add("  m_PrefabInstance: {fileID: 0}")
    $newContent.Add("  m_PrefabAsset: {fileID: 0}")
    $newContent.Add("  m_GameObject: {fileID: $goId}")
    $newContent.Add("  m_Enabled: 1")
    $newContent.Add("  m_EditorHideFlags: 0")
    $newContent.Add("  m_Script: {fileID: 11500000, guid: $occlusionGuid, type: 3}")
    $newContent.Add("  m_Name: ")
    $newContent.Add("  m_EditorClassIdentifier: ")
    $newContent.Add("  obstacleLayers: -1")
    $newContent.Add("  minCutoffFreq: 500")
    $newContent.Add("  maxCutoffFreq: 22000")
    $newContent.Add("  smoothSpeed: 10")
    $newContent.Add("  attenuateVolume: 1")
    $newContent.Add("  volumeMultiplierPerWall: 0.5")
    $newContent.Add("  minimumVolume: 0.05")
    $newContent.Add("  maxWallsToConsider: 3")
}

[System.IO.File]::WriteAllLines($path, $newContent)
