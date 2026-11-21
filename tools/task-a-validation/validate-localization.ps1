param([string]$TmlDirectory = 'F:/SteamLibrary/steamapps/common/tModLoader')
$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
Add-Type -Path (Join-Path $TmlDirectory 'Libraries/hjson/3.0.0/lib/netstandard1.3/Hjson.dll')
function Get-LeafKeys($node, [string]$prefix = '') {
    if ($node -is [Hjson.JsonObject]) {
        foreach ($key in $node.Keys) { Get-LeafKeys $node[$key] ($prefix + $key + '.') }
    } elseif ($node.ToString() -ne '""') { $prefix.TrimEnd('.') }
}
$allKeys = @()
foreach ($language in @('en-US', 'ru-RU')) {
    $path = Join-Path $root "Localization/${language}_Mods.RPGMod.hjson"
    $value = [Hjson.HjsonValue]::Parse([IO.File]::ReadAllText($path))
    if ($value['Nodes'].Count -ne 75) { throw "$language must have 75 localized nodes" }
    if ($value['Stats'].Count -ne 7) { throw "$language must have 7 localized stats" }
    foreach ($key in @('Name', 'Effect', 'Bonus')) {
        if (!$value['Awakening']['rogue'].ContainsKey($key)) { throw "Missing rogue awakening $key" }
    }
    $allKeys += ,@(Get-LeafKeys $value | Sort-Object)
    Write-Output "${language}: valid HJSON, 75 nodes, 7 stats, $($allKeys[-1].Count) translated strings"
}
if (Compare-Object $allKeys[0] $allKeys[1]) { throw 'Localization key sets differ' }
Write-Output 'RU/EN nonempty localization key sets match (optional empty tooltips excluded).'