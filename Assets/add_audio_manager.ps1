$path = "c:\Users\bruno\Documents\GitHub\Bruno-Gomez-Practica-Unity\Assets\Bruno Gomez Stirparo Practica.unity"
$content = Get-Content $path
$newContent = New-Object System.Collections.Generic.List[string]

$musicGuid = "f10e76ffe799e4544961235307b66904"
$ambienceGuid = "c44a146bc46cdf64b8b07dac25efbeb0"
$animSyncGuid = "9e5334dfe1da84b468a7df004add1758"
$playerGoId = "871048157"

# Step 1: Add AnimationSoundSync to Player
$inPlayer = $false
foreach ($line in $content) {
    if ($line -match "--- !u!1 &$playerGoId") { $inPlayer = $true }
    
    $newContent.Add($line)
    
    if ($inPlayer -and $line -match "m_Component:") {
        $newContent.Add("  - component: {fileID: 2100000010}")
        $inPlayer = $false
    }
}

# Step 2: Append New Definitions
$newContent.Add("--- !u!1 &2100000000")
$newContent.Add("GameObject:")
$newContent.Add("  m_Component:")
$newContent.Add("  - component: {fileID: 2100000001}")
$newContent.Add("  - component: {fileID: 2100000002}")
$newContent.Add("  - component: {fileID: 2100000003}")
$newContent.Add("  m_Layer: 0")
$newContent.Add("  m_Name: AudioManager")
$newContent.Add("  m_TagString: Untagged")
$newContent.Add("  m_Icon: {fileID: 0}")
$newContent.Add("  m_NavMeshLayer: 0")
$newContent.Add("  m_StaticEditorFlags: 0")
$newContent.Add("  m_IsActive: 1")
$newContent.Add("--- !u!4 &2100000001")
$newContent.Add("Transform:")
$newContent.Add("  m_GameObject: {fileID: 2100000000}")
$newContent.Add("  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}")
$newContent.Add("  m_LocalPosition: {x: 0, y: 0, z: 0}")
$newContent.Add("  m_LocalScale: {x: 1, y: 1, z: 1}")
$newContent.Add("  m_Children: []")
$newContent.Add("  m_Father: {fileID: 0}")
$newContent.Add("--- !u!114 &2100000002")
$newContent.Add("MonoBehaviour:")
$newContent.Add("  m_GameObject: {fileID: 2100000000}")
$newContent.Add("  m_Enabled: 1")
$newContent.Add("  m_Script: {fileID: 11500000, guid: $musicGuid, type: 3}")
$newContent.Add("  layers: []")
$newContent.Add("--- !u!114 &2100000003")
$newContent.Add("MonoBehaviour:")
$newContent.Add("  m_GameObject: {fileID: 2100000000}")
$newContent.Add("  m_Enabled: 1")
$newContent.Add("  m_Script: {fileID: 11500000, guid: $ambienceGuid, type: 3}")
$newContent.Add("--- !u!114 &2100000010")
$newContent.Add("MonoBehaviour:")
$newContent.Add("  m_GameObject: {fileID: $playerGoId}")
$newContent.Add("  m_Enabled: 1")
$newContent.Add("  m_Script: {fileID: 11500000, guid: $animSyncGuid, type: 3}")

[System.IO.File]::WriteAllLines($path, $newContent)
