# Publish CommandManBlazor
$publishDir = "./publish"
if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }

# Publish Web App
dotnet publish CommandMan.Web/CommandMan.Web.csproj -c Release -o $publishDir

# Publish Tray App (Merge into same dir)
dotnet publish CommandMan.Tray/CommandMan.Tray.csproj -c Release -o $publishDir

Copy-Item ./SetupContextMenu.ps1 $publishDir/
Write-Host "Publish complete. Files are in the 'publish' directory." -ForegroundColor Green
