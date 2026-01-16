# StandardizeProject.ps1
# Usage: .\StandardizeProject.ps1 -ProjectDir "C:\Path\To\Project"

param (
    [Parameter(Mandatory = $true)]
    [string]$ProjectDir
)

$projectName = Split-Path $ProjectDir -Leaf
$csprojPath = Get-ChildItem -Path $ProjectDir -Filter "*.csproj" | Select-Object -First 1

if (-not $csprojPath) {
    Write-Host "No .csproj found in $ProjectDir" -ForegroundColor Red
    exit 1
}

Write-Host "Standardizing $projectName..." -ForegroundColor Cyan

# 1. Add stylecop.json if missing
$rootStyleCopPath = Join-Path (Get-Item .).FullName "CommandMan.Core\stylecop.json"
$targetStyleCopPath = Join-Path $ProjectDir "stylecop.json"

if (Test-Path $rootStyleCopPath) {
    if (-not (Test-Path $targetStyleCopPath)) {
        Copy-Item $rootStyleCopPath $targetStyleCopPath
        Write-Host "  Added stylecop.json"
    }
}

# 2. Modify .csproj
[xml]$xml = Get-Content $csprojPath.FullName

# Ensure StyleCop package is present
$ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
$ns.AddNamespace("ns", "http://schemas.microsoft.com/developer/msbuild/2003")

$itemGroup = $xml.Project.ItemGroup | Where-Object { $_.PackageReference -ne $null } | Select-Object -First 1
if (-not $itemGroup) {
    $itemGroup = $xml.CreateElement("ItemGroup")
    $xml.Project.AppendChild($itemGroup)
}

$styleCopRef = $xml.Project.ItemGroup.PackageReference | Where-Object { $_.Include -eq "StyleCop.Analyzers" }
if (-not $styleCopRef) {
    $pkg = $xml.CreateElement("PackageReference")
    $pkg.SetAttribute("Include", "StyleCop.Analyzers")
    $pkg.SetAttribute("Version", "1.1.118")
    
    $pa = $xml.CreateElement("PrivateAssets")
    $pa.InnerText = "all"
    $pkg.AppendChild($pa)
    
    $ia = $xml.CreateElement("IncludeAssets")
    $ia.InnerText = "runtime; build; native; contentfiles; analyzers; buildtransitive"
    $pkg.AppendChild($ia)
    
    $itemGroup.AppendChild($pkg)
    Write-Host "  Added StyleCop.Analyzers package reference"
}

# Ensure PropertyGroup settings
$propGroup = $xml.Project.PropertyGroup | Select-Object -First 1
if (-not $propGroup.GenerateDocumentationFile) {
    $node = $xml.CreateElement("GenerateDocumentationFile")
    $node.InnerText = "true"
    $propGroup.AppendChild($node)
    Write-Host "  Enabled GenerateDocumentationFile"
}

if (-not $propGroup.NoWarn -or $propGroup.NoWarn -notmatch "SA1636") {
    if ($propGroup.NoWarn) {
        $propGroup.NoWarn = '$(NoWarn);SA1636'
    }
    else {
        $node = $xml.CreateElement("NoWarn")
        $node.InnerText = '$(NoWarn);SA1636'
        $propGroup.AppendChild($node)
    }
    Write-Host "  Added SA1636 to NoWarn"
}

$xml.Save($csprojPath.FullName) | Out-Null
Write-Host "$projectName standardized successfully!" -ForegroundColor Green
