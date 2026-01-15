@echo off
echo Publishing...
powershell -ExecutionPolicy Bypass -File "publish.ps1"
if %errorlevel% neq 0 (
    echo Publish failed!
    pause
    exit /b %errorlevel%
)
echo Running...
start "" "publish\CommandMan.Tray.exe"
