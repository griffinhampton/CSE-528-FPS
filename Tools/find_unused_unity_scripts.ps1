param(
    [string]$ProjectRoot = (Resolve-Path ".").Path,
    [string]$AssetsDir = "Assets",
    [string]$OutputPath = "UnusedScriptsReport.md"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Get-UnityGuidFromMeta([string]$metaPath) {
    $content = Get-Content -LiteralPath $metaPath -Raw
    if ($content -match "(?m)^guid:\s*([0-9a-f]{32})\s*$") {
        return $Matches[1]
    }
    return $null
}

function Escape-Regex([string]$text) {
    return [Regex]::Escape($text)
}

$projectRootFull = (Resolve-Path -LiteralPath $ProjectRoot).Path
$assetsFull = Join-Path $projectRootFull $AssetsDir
if (-not (Test-Path -LiteralPath $assetsFull)) {
    throw "Assets directory not found: $assetsFull"
}

# 1) Collect all scripts + GUIDs
$scriptMetaFiles = Get-ChildItem -LiteralPath $assetsFull -Recurse -File -Filter "*.cs.meta"
$guidToScript = @{}
$scriptPathToGuid = @{}
foreach ($meta in $scriptMetaFiles) {
    $guid = Get-UnityGuidFromMeta $meta.FullName
    if ([string]::IsNullOrWhiteSpace($guid)) { continue }

    $csPath = $meta.FullName.Substring(0, $meta.FullName.Length - ".meta".Length)
    $relativeCs = (Resolve-Path -LiteralPath $csPath).Path.Substring($projectRootFull.Length).TrimStart([char[]]@('\','/')) -replace "\\","/"

    $guidToScript[$guid] = $relativeCs
    $scriptPathToGuid[$relativeCs] = $guid
}

# 2) Scan Assets for serialized script references (m_Script)
# Unity uses YAML with lines like: m_Script: {fileID: 11500000, guid: <guid>, type: 3}
$scriptGuidRefs = New-Object 'System.Collections.Generic.HashSet[string]'

$assetFiles = Get-ChildItem -LiteralPath $assetsFull -Recurse -File |
    Where-Object {
        # Exclude code and meta; include common Unity YAML containers.
        $_.Extension -notin @('.cs', '.meta', '.dll')
    }

$re = [Regex]::new("m_Script:\s*\{[^\}]*guid:\s*([0-9a-f]{32})", [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
foreach ($f in $assetFiles) {
    # Many Unity assets are text; if a file is binary, -Raw read may throw or include NULs.
    try {
        $raw = Get-Content -LiteralPath $f.FullName -Raw -ErrorAction Stop
    } catch {
        continue
    }

    if ($raw.IndexOf("m_Script", [System.StringComparison]::OrdinalIgnoreCase) -lt 0) {
        continue
    }

    $matches = $re.Matches($raw)
    if ($matches.Count -eq 0) { continue }

    foreach ($m in $matches) {
        $g = $m.Groups[1].Value
        if (-not [string]::IsNullOrWhiteSpace($g)) {
            [void]$scriptGuidRefs.Add($g)
        }
    }
}

# 3) Determine unused by serialization
$unusedBySerialization = @()
$usedBySerialization = @()
foreach ($kvp in $guidToScript.GetEnumerator()) {
    if ($scriptGuidRefs.Contains($kvp.Key)) {
        $usedBySerialization += $kvp.Value
    } else {
        $unusedBySerialization += $kvp.Value
    }
}

$unusedBySerialization = @($unusedBySerialization | Sort-Object)
$usedBySerialization = @($usedBySerialization | Sort-Object)

# 4) Heuristic: if unused-by-serialization but mentioned in code, it might be created/used dynamically.
# Use file base name as a proxy for class name (common convention).
$allCsFiles = Get-ChildItem -LiteralPath $assetsFull -Recurse -File -Filter "*.cs" |
    ForEach-Object {
        (Resolve-Path -LiteralPath $_.FullName).Path.Substring($projectRootFull.Length).TrimStart([char[]]@('\','/')) -replace "\\","/"
    }

$csFileContents = @{}
foreach ($cs in $allCsFiles) {
    $full = Join-Path $projectRootFull ($cs -replace "/","\\")
    try {
        $csFileContents[$cs] = Get-Content -LiteralPath $full -Raw
    } catch {
        $csFileContents[$cs] = ""
    }
}

function Is-MentionedInOtherCode([string]$typeName, [string]$excludeCsPath) {
    if ([string]::IsNullOrWhiteSpace($typeName)) { return $false }
    $pattern = "(?<![A-Za-z0-9_])" + (Escape-Regex $typeName) + "(?![A-Za-z0-9_])"
    foreach ($kvp in $csFileContents.GetEnumerator()) {
        if (-not [string]::IsNullOrWhiteSpace($excludeCsPath) -and $kvp.Key -eq $excludeCsPath) {
            continue
        }
        if ($kvp.Value -match $pattern) {
            return $true
        }
    }
    return $false
}

$maybeUsedDynamically = @()
$probablySafeToRemove = @()
$editorScripts = @()

foreach ($cs in $unusedBySerialization) {
    if ($cs -match "(?i)/Editor/" -or $cs -match "(?i)Editor\\") {
        $editorScripts += $cs
        continue
    }

    $baseName = [IO.Path]::GetFileNameWithoutExtension($cs)
    if (Is-MentionedInOtherCode $baseName $cs) {
        $maybeUsedDynamically += $cs
    } else {
        $probablySafeToRemove += $cs
    }
}

$maybeUsedDynamically = @($maybeUsedDynamically | Sort-Object)
$probablySafeToRemove = @($probablySafeToRemove | Sort-Object)
$editorScripts = @($editorScripts | Sort-Object)

# 5) Emit report
$outFull = Join-Path $projectRootFull $OutputPath
$lines = New-Object System.Collections.Generic.List[string]

$lines.Add("# Unused Scripts Report")
$lines.Add("")
$lines.Add("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')")
$lines.Add("Project: $projectRootFull")
$lines.Add("")
$lines.Add("## How to read this")
$lines.Add("- **Used (Serialized)**: Script GUID referenced by a scene/prefab/asset `m_Script` field.")
$lines.Add("- **Unused (Not Serialized)**: Script GUID not referenced by any `m_Script` in `Assets/`. These may still be used dynamically (e.g., `AddComponent<T>()`, reflection, string-based lookups).")
$lines.Add("- **Maybe Used in Code**: Not serialized, but its file base name is mentioned somewhere in C# code (heuristic).")
$lines.Add("- **Editor Scripts**: Not serialized; often only invoked by the Unity Editor and may be safe/unsafe to delete depending on tooling.")
$lines.Add("")

$lines.Add("## Summary")
$lines.Add("- Total scripts: $($guidToScript.Count)")
$lines.Add("- Used (Serialized): $($usedBySerialization.Count)")
$lines.Add("- Unused (Not Serialized): $($unusedBySerialization.Count)")
$lines.Add("  - Maybe Used in Code (heuristic): $($maybeUsedDynamically.Count)")
$lines.Add("  - Editor Scripts: $($editorScripts.Count)")
$lines.Add("  - Probably Safe To Remove (no refs found): $($probablySafeToRemove.Count)")
$lines.Add("")

function Add-Section([string]$title, [string[]]$items) {
    $lines.Add("## $title")
    if (-not $items -or $items.Count -eq 0) {
        $lines.Add("(none)")
        $lines.Add("")
        return
    }

    foreach ($i in $items) {
        $lines.Add("- $i")
    }
    $lines.Add("")
}

Add-Section "Probably Safe To Remove" $probablySafeToRemove
Add-Section "Maybe Used Dynamically (Heuristic)" $maybeUsedDynamically
Add-Section "Editor Scripts (Not Serialized)" $editorScripts

Set-Content -LiteralPath $outFull -Value ($lines -join "`n") -Encoding UTF8

Write-Host "Wrote report: $outFull"
Write-Host "Probably safe to remove: $($probablySafeToRemove.Count)"
Write-Host "Maybe used dynamically: $($maybeUsedDynamically.Count)"
Write-Host "Editor scripts: $($editorScripts.Count)"
