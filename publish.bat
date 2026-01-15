@echo off
powershell -ExecutionPolicy Bypass -File "publish.ps1"
if %errorlevel% neq 0 (
    echo Publish failed!
    pause
)
