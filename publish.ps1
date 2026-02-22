$csprojPath = "./Mercury.Editor/Mercury.Editor.csproj"
$publishDir = "./publish"
$buildDir = "$($publishDir)/build"
$privateKeyPath = "./private.key"

[xml]$xml = Get-Content $csprojPath
$version = ($xml.Project.PropertyGroup.AssemblyVersion | Out-String).Trim()
if (-not $version) {
    Write-Error "tag <AssemblyVersion> not found on .csproj"
    exit 1
}
$zipName = "Mercury-$version.rar"
Write-Host "Detected version: $version"

# clear publish folder contents
if (Test-Path $buildDir) {
    Remove-Item $buildDir -Recurse -Force
}
if (Test-Path $publishDir) {
    Remove-Item $publishDir -Recurse -Force
}
New-Item -ItemType Directory -Path $publishDir | Out-Null
New-Item -ItemType Directory -Path $buildDir | Out-Null

Write-Host "Publishing Mercury.Editor..."
dotnet publish $csprojPath -o $buildDir -c Release --self-contained
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build Editor"
    exit 1
}

Write-Host "Publishing Updater..."
dotnet publish "./Updater/" -o $buildDir -c Release --self-contained
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build Updater"
    exit 1
}


if (Test-Path $zipName) {
    Remove-Item $zipName -Force
}

Write-Host "Creating $zipName..."
rar a -m5 "$publishDir/$zipName" "$buildDir/*"
Write-Host "Zip file created: $zipName"

Write-Host "Signing $zipName with private key"
$data = [IO.File]::ReadAllBytes("$publishDir/$zipName")
$rsa = [System.Security.Cryptography.RSA]::Create()
$rsa.ImportRSAPrivateKey([IO.File]::ReadAllBytes($privateKeyPath),[ref]0)
$signature = $rsa.SignData($data,[System.Security.Cryptography.HashAlgorithmName]::SHA256,[System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
[IO.File]::WriteAllBytes("$publishDir/$zipName.sig", $signature)
Write-Host "Signature saved at: $publishDir/$zipName.sig"

Write-Host "Creating Installer"
iscc.exe "/DMyAppVersion=$version" "./installer.iss"
Write-Host "Installer created"

Write-Host "Removing build files..."
Remove-Item $buildDir -Recurse -Force
