@echo off
setlocal
cd /d "%~dp0"

echo =========================================================
echo   [Roblox API Generator] Generating C# Bindings
echo =========================================================

powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0generate.ps1" %*

if %ERRORLEVEL% equ 0 (
    echo.
    echo =========================================================
    echo   Generation complete! RobloxAPI.cs updated.
    echo =========================================================
) else (
    echo.
    echo Generation failed with error code %ERRORLEVEL%.
)
endlocal
