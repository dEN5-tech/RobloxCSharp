@echo off
cd /d %~dp0

echo =========================================================
echo   [Roblox C# Builder] Compiling C# and Generating Lua
echo =========================================================

dotnet build RobloxCSharp.sln -c Release

echo.
echo =========================================================
echo   Compilation complete!
echo   Lua files updated in 'src/' for Rojo 7.7 live sync.
echo =========================================================
pause
