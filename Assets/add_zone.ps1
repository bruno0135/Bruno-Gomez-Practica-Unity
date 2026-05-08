$path = "c:\Users\bruno\Documents\GitHub\Bruno-Gomez-Practica-Unity\Assets\Bruno Gomez Stirparo Practica.unity"
$content = Get-Content $path
$newContent = New-Object System.Collections.Generic.List[string]

$zoneGuid = "634ca795502eb7248a6ca18be2c5f778"

foreach ($line in $content) {
    $newContent.Add($line)
}

# Add Interior Zone GameObject
$newContent.Add("--- !u!1 &2100000020")
$newContent.Add("GameObject:")
$newContent.Add("  m_Component:")
$newContent.Add("  - component: {fileID: 2100000021}")
$newContent.Add("  - component: {fileID: 2100000022}")
$newContent.Add("  - component: {fileID: 2100000023}")
$newContent.Add("  m_Layer: 0")
$newContent.Add("  m_Name: Interior_AudioZone")
$newContent.Add("--- !u!4 &2100000021")
$newContent.Add("Transform:")
$newContent.Add("  m_GameObject: {fileID: 2100000020}")
$newContent.Add("  m_LocalPosition: {x: 0, y: 0, z: 0}")
$newContent.Add("--- !u!65 &2100000022")
$newContent.Add("BoxCollider:")
$newContent.Add("  m_GameObject: {fileID: 2100000020}")
$newContent.Add("  m_IsTrigger: 1")
$newContent.Add("  m_Center: {x: 0, y: 1, z: 0}")
$newContent.Add("  m_Size: {x: 10, y: 5, z: 10}")
$newContent.Add("--- !u!114 &2100000023")
$newContent.Add("MonoBehaviour:")
$newContent.Add("  m_GameObject: {fileID: 2100000020}")
$newContent.Add("  m_Enabled: 1")
$newContent.Add("  m_Script: {fileID: 11500000, guid: $zoneGuid, type: 3}")

[System.IO.File]::WriteAllLines($path, $newContent)
