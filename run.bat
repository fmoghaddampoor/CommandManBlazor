@echo off
if not exist "publish\CommandMan.Tray.exe" (
    echo Application not found in 'publish' directory. Please publish first.
    pause
    exit /b 1
)
start "" "publish\CommandMan.Tray.exe"
