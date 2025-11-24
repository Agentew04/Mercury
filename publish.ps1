$csprojPath = "./Mercury.Editor/Mercury.Editor.csproj"
$publishDir = "./publish"

[xml]$xml = Get-Content $csprojPath
$version = ($xml.Project.PropertyGroup.AssemblyVersion | Out-String).Trim()
if (-not $version) {
    Write-Error "tag <AssemblyVersion> not found on .csproj"
    exit 1
}
Write-Host "Detected version: $version"

# clear publish folder contents
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}
New-Item -ItemType Directory -Path $publishDir | Out-Null

Write-Host "Publishing Mercury.Editor..."
dotnet publish "./Mercury.Editor/Mercury.Editor.csproj" -o $publishDir -c Release --self-contained
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build Editor"
    exit 1
}

Write-Host "Publishing Updater..."
dotnet publish "./Updater/" -o $publishDir -c Release --self-contained
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build Updater"
    exit 1
}

$zipName = "Mercury-$version.zip"
if (Test-Path $zipName) {
    Remove-Item $zipName -Force
}

Write-Host "Creating $zipName..."
Compress-Archive -Path "$publishDir/*" -DestinationPath "$publishDir/$zipName"

Write-Host "Zip file created: $zipName"