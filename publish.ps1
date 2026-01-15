# Publish CommandManBlazor
$publishDir = "./publish"
if (Test-Path $publishDir) { Remove-Item -Recurse -Force $publishDir }

# Publish Web App to temp
$webPublishDir = "./publish_web_temp"
if (Test-Path $webPublishDir) { Remove-Item -Recurse -Force $webPublishDir }
dotnet publish CommandMan.Web/CommandMan.Web.csproj -c Release -o $webPublishDir

# Publish Tray App to root
dotnet publish CommandMan.Tray/CommandMan.Tray.csproj -c Release -o $publishDir

# Copy Web App files to root (merge)
Copy-Item -Path "$webPublishDir/*" -Destination $publishDir -Recurse -Force
Remove-Item -Recurse -Force $webPublishDir

Copy-Item ./SetupContextMenu.ps1 $publishDir/
Write-Host "Publish complete. Files are in the 'publish' directory." -ForegroundColor Green
