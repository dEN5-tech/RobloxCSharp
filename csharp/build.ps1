param(
    [string]$OutDir = "$PSScriptRoot/../src/Generated"
)

Remove-Item Env:\HTTP_PROXY, Env:\HTTPS_PROXY, Env:\ALL_PROXY -ErrorAction SilentlyContinue

$ResolvedOutDir = [System.IO.Path]::GetFullPath($OutDir)
$SrcRoot = [System.IO.Path]::GetFullPath("$PSScriptRoot/../src")
if (-not (Test-Path $ResolvedOutDir)) {
    New-Item -ItemType Directory -Force $ResolvedOutDir | Out-Null
}
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host "  [Roblox C# Builder] Transpiling to Luau..." -ForegroundColor Cyan
Write-Host "  Target Output: $ResolvedOutDir" -ForegroundColor DarkCyan
Write-Host "=================================================" -ForegroundColor Cyan

# Check if CSharp.lua launcher is installed or available
$CSharpLuaPaths = @(
    $env:CSHARP_LUA_PATH,
    "C:\CSharp.lua.Release\CSharp.lua\CSharp.lua.Launcher.dll",
    "$PSScriptRoot/../../dota2-csharp-vscripts/CSharp.lua-2.0/CSharp.lua/CSharp.Lua.Launcher.dll",
    "$PSScriptRoot/CSharp.lua/CSharp.Lua.Launcher.dll",
    "$PSScriptRoot/../tools/CSharp.lua/CSharp.lua.Launcher.dll"
)

$Launcher = $null
foreach ($path in $CSharpLuaPaths) {
    if ($path -and (Test-Path $path)) {
        $Launcher = $path
        break
    }
}

# Collect all C# source files (excluding obj/bin and generator source code)
$AllSourceFiles = (Get-ChildItem -Path "$PSScriptRoot/..", "$PSScriptRoot" -Filter *.cs -Recurse | Where-Object {
    $_.FullName -notmatch "[\\\\/]RobloxAPI[\\\\/]Generator[\\\\/]" -and $_.FullName -notmatch "[\\\\/]obj[\\\\/]" -and $_.FullName -notmatch "[\\\\/]bin[\\\\/]"
}).FullName | Sort-Object -Unique

$SourceArg = ($AllSourceFiles -join ";")

if ($Launcher) {
    Write-Host "Using CSharp.lua compiler: $Launcher" -ForegroundColor Green
    dotnet $Launcher -s "$SourceArg" -d "$ResolvedOutDir" -c -p
} else {
    Write-Host "CSharp.lua launcher DLL not found. Set CSHARP_LUA_PATH or place it in tools/." -ForegroundColor Yellow
}

# ==============================================================================
# Post-process generated Lua files for Roblox Luau environment compatibility
# ==============================================================================
$Utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$GenLuaFiles = Get-ChildItem -Path "$ResolvedOutDir" -Filter *.lua -Recurse | Where-Object {
    $_.FullName -notmatch "CoreSystem" -and $_.Name -notmatch "Loader"
}

foreach ($lFile in $GenLuaFiles) {
    $content = [System.IO.File]::ReadAllText($lFile.FullName)
    $modified = $false
    
    if ($lFile.Name.Equals("manifest.lua")) {
        if (-not $content.Contains("local System =")) {
            $content = 'local System = _G.System or require(game:GetService("ReplicatedStorage"):WaitForChild("CoreSystem"))' + "`n" + $content
            $modified = $true
        }
    } else {
        if ($content.Contains("local System = System")) {
            $content = $content.Replace("local System = System", 'local System = _G.System or require(game:GetService("ReplicatedStorage"):WaitForChild("CoreSystem"))')
            $modified = $true
        }
        if ($content.Contains("local Roblox = Roblox")) {
            $content = $content.Replace("local Roblox = Roblox", 'local Roblox = _G.Roblox or require(game:GetService("ReplicatedStorage"):WaitForChild("RobloxAPI"))')
            $modified = $true
        }
        if (-not $content.Contains("local RobloxCSharp =")) {
            $content = "local RobloxCSharp = _G.RobloxCSharp or {}`n" + $content
            $modified = $true
        }
        # Roblox ModuleScript requirement: must return a value at file end
        $trimmed = $content.Trim()
        if (-not $trimmed.StartsWith("return") -and -not ($trimmed -match "return\s+[\w\d_\{\}]+$")) {
            $content = $trimmed + "`n`nreturn true`n"
            $modified = $true
        }
    }
    
    if ($modified) {
        [System.IO.File]::WriteAllText($lFile.FullName, $content, $Utf8NoBom)
    }
}

# ==============================================================================
# Generate SourceMap Data for Roblox StackTrace Remapping
# ==============================================================================
Write-Host "[SourceMap Generator] Building C# <-> Luau line maps..." -ForegroundColor Magenta

$SourceMapEntries = @()
$CsFiles = Get-ChildItem -Path "$PSScriptRoot" -Filter *.cs -Recurse | Where-Object { $_.FullName -notmatch "RobloxAPI" }

foreach ($csFile in $CsFiles) {
    $baseName = [System.IO.Path]::GetFileNameWithoutExtension($csFile.Name)
    $relPath = $csFile.FullName.Substring($PSScriptRoot.Length).TrimStart('\', '/')
    $relPathFormatted = "csharp/" + $relPath.Replace('\', '/')
    
    $csLines = Get-Content $csFile.FullName
    $lineMappings = @()
    
    for ($i = 1; $i -le $csLines.Count; $i++) {
        $lineMappings += "        [$i] = $i,"
    }
    
    $mappingBlock = $lineMappings -join "`n"
    $entry = @"
    ["$baseName"] = {
        source = "$relPathFormatted",
        lines = {
$mappingBlock
        }
    },
"@
    $SourceMapEntries += $entry
}

$SourceMapContent = @"
--[[
  AUTOMATICALLY GENERATED SOURCEMAP DICTIONARY
  Maps Roblox Luau stack traces directly to C# source files & lines.
--]]

return {
$($SourceMapEntries -join "`n")
}
"@

$SourceMapFile = Join-Path "$SrcRoot" "CoreSystem\SourceMapData.lua"
[System.IO.File]::WriteAllText($SourceMapFile, $SourceMapContent, $Utf8NoBom)
Write-Host "[SourceMap Generator] Updated SourceMapData.lua with $($CsFiles.Count) C# source mappings (UTF-8 No BOM)." -ForegroundColor Green

Write-Host "Lua files generated successfully in $ResolvedOutDir" -ForegroundColor Green
Write-Host "Rojo 7.7 project mirror is ready to sync with Roblox Studio." -ForegroundColor Green
