# Register "Open with CommandMan" context menu for folders
$exePath = Join-Path $PSScriptRoot "CommandMan.Tray.exe"
$command = "`"$exePath`" -left `"%1`""

# 1. Folder context menu
$regPath = "HKCU:\Software\Classes\Directory\shell\CommandMan"
if (-not (Test-Path $regPath)) { New-Item -Path $regPath -Force }
Set-ItemProperty -Path $regPath -Name "(Default)" -Value "Open with CommandMan"
Set-ItemProperty -Path $regPath -Name "Icon" -Value "$exePath"

$cmdPath = Join-Path $regPath "command"
if (-not (Test-Path $cmdPath)) { New-Item -Path $cmdPath -Force }
Set-ItemProperty -Path $cmdPath -Name "(Default)" -Value $command

# 2. Folder background context menu (right-click inside a folder)
$regPathBg = "HKCU:\Software\Classes\Directory\Background\shell\CommandMan"
if (-not (Test-Path $regPathBg)) { New-Item -Path $regPathBg -Force }
Set-ItemProperty -Path $regPathBg -Name "(Default)" -Value "Open with CommandMan"
Set-ItemProperty -Path $regPathBg -Name "Icon" -Value "$exePath"

$cmdPathBg = Join-Path $regPathBg "command"
if (-not (Test-Path $cmdPathBg)) { New-Item -Path $cmdPathBg -Force }
# For background, %V gives the current folder path
$commandBg = "`"$exePath`" -left `"%V`""
Set-ItemProperty -Path $cmdPathBg -Name "(Default)" -Value $commandBg

Write-Host "Context menu registered successfully! Pointing to: $exePath" -ForegroundColor Green
