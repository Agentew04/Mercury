$csprojPath = Resolve-Path "../src/Mercury.Editor/Mercury.Editor.csproj"
$updaterProjectPath = Resolve-Path "../src/Updater/Updater.csproj"
$publishDir = "../publish"
$buildDir = "$($publishDir)/build"
$privateKeyPath = Resolve-Path "./private.key"

if(-not (Test-Path $publishDir)){
	New-Item -ItemType Directory -Path $publishDir | Out-Null
}
$publishDir = Resolve-Path $publishDir
if(-not (Test-Path $privateKeyPath)) {
		Write-Host "Nao encontrei arquivo da chave privada! Abortando"
		exit 1
}
if(-not (Test-Path $csprojPath)){
		Write-Host "Nao encontrei projeto! Abortando"
		exit 1
}

[xml]$xml = Get-Content $csprojPath
$version = ($xml.Project.PropertyGroup.AssemblyVersion | Out-String).Trim()
if (-not $version) {
    Write-Error "tag <AssemblyVersion> not found on .csproj"
    exit 1
}
$zipNameWin = "Mercury_$($version)_Windows.rar"
$zipNameLinux = "Mercury_$($version)_Linux.tar.gz"
Write-Host "Detected version: $version"

# clear publish folder contents
if(-not (Test-Path $publishDir)){
    New-Item -ItemType Directory -Path $publishDir | Out-Null
}
if (Test-Path $buildDir) {
    Remove-Item $buildDir -Recurse -Force
}
New-Item -ItemType Directory -Path $buildDir | Out-Null




Write-Host "Publishing Mercury.Editor for Windows..."
dotnet publish $csprojPath -o $buildDir -c Release --self-contained -r win-x64
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build Editor"
    exit 1
}

Write-Host "Publishing Updater for Windows..."
dotnet publish $updaterProjectPath -o $buildDir -c Release --self-contained -r win-x64
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build Updater"
    exit 1
}

if (Test-Path $zipNameWin) {
    Remove-Item $zipNameWin -Force
}

Write-Host "Creating $zipNameWin..."
Push-Location $buildDir
rar a -r -m5 "../$zipNameWin" "*"
Pop-Location
Write-Host "Zip file created: $zipNameWin"

Write-Host "Signing $zipNameWin with private key"
$data = [IO.File]::ReadAllBytes("$publishDir/$zipNameWin")
$rsa = [System.Security.Cryptography.RSA]::Create()
$rsa.ImportRSAPrivateKey([IO.File]::ReadAllBytes($privateKeyPath),[ref]0)
$signature = $rsa.SignData($data,[System.Security.Cryptography.HashAlgorithmName]::SHA256,[System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
[IO.File]::WriteAllBytes("$publishDir/$zipNameWin.sig", $signature)
Write-Host "Signature saved at: $publishDir/$zipNameWin.sig"

Write-Host "Creating Installer for Windows"
iscc.exe "/DMyAppVersion=$version" "./installer.iss"
Write-Host "Installer created"

Write-Host "Removing build files..."
Remove-Item $buildDir -Recurse -Force

Write-Host "Publishing Mercury.Editor for Linux..."
New-Item -ItemType Directory -Path $buildDir | Out-Null
dotnet publish $csprojPath -o $buildDir -c Release --self-contained -r linux-x64
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build Editor"
    exit 1
}
Write-Host "Publishing Updater for Linux..."
dotnet publish $updaterProjectPath -o $buildDir -c Release --self-contained -r linux-x64
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to build Updater"
    exit 1
}
if (Test-Path $zipNameLinux) {
    Remove-Item $zipNameLinux -Force
}
Write-Host "Creating $zipNameLinux..."
Push-Location $buildDir
tar -cvzf "../$zipNameLinux" "*"
Pop-Location
Write-Host "Zip file created: $zipNameLinux"

Write-Host "Signing $zipNameLinux with private key"
$data = [IO.File]::ReadAllBytes("$publishDir/$zipNameLinux")
$rsa = [System.Security.Cryptography.RSA]::Create()
$rsa.ImportRSAPrivateKey([IO.File]::ReadAllBytes($privateKeyPath),[ref]0)
$signature = $rsa.SignData($data,[System.Security.Cryptography.HashAlgorithmName]::SHA256,[System.Security.Cryptography.RSASignaturePadding]::Pkcs1)
[IO.File]::WriteAllBytes("$publishDir/$zipNameLinux.sig", $signature)
Write-Host "Signature saved at: $publishDir/$zipNameLinux.sig"

Write-Host "Removing build files..."
Remove-Item $buildDir -Recurse -Force
