param(
    [switch]$Fetch,
    [string]$DumpPath = "$PSScriptRoot/api-dump.json"
)

Remove-Item Env:\HTTP_PROXY, Env:\HTTPS_PROXY, Env:\ALL_PROXY -ErrorAction SilentlyContinue

$FetchArg = if ($Fetch) { "--fetch" } else { "" }

Write-Host "[RobloxAPI] Running RobloxAPI.Generator..." -ForegroundColor Cyan
dotnet run --project "$PSScriptRoot/Generator/RobloxAPI.Generator.csproj" -- $FetchArg "$DumpPath" "$PSScriptRoot/RobloxAPI.cs"

if ($LASTEXITCODE -eq 0) {
    Write-Host "[RobloxAPI] RobloxAPI bindings successfully generated!" -ForegroundColor Green
} else {
    Write-Host "[RobloxAPI] Generation failed with exit code $LASTEXITCODE" -ForegroundColor Red
}

