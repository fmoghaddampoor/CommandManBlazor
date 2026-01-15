@echo off
echo Closing existing application...
taskkill /F /IM CommandMan.Tray.exe >nul 2>&1
taskkill /F /IM CommandMan.Web.exe >nul 2>&1
timeout /t 2 /nobreak >nul

echo Publishing...
powershell -ExecutionPolicy Bypass -File "publish.ps1"
if %errorlevel% neq 0 (
    echo Publish failed!
    pause
    exit /b %errorlevel%
)
echo Running...
start "" "publish\CommandMan.Tray.exe"
