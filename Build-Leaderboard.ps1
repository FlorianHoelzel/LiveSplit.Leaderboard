$ErrorActionPreference = 'Stop'

$Here = Split-Path -Parent $MyInvocation.MyCommand.Path
$Parent = Split-Path -Parent $Here
$LiveSplit = Join-Path $Parent 'LiveSplit'
$Project = Join-Path $Here 'src\LiveSplit.Leaderboard\LiveSplit.Leaderboard.csproj'
$Output = Join-Path $Here 'READY-TO-INSTALL'

function Fail($Message) {
    Write-Host "`n$Message" -ForegroundColor Red
    Write-Host "`nPress Enter to close..."
    Read-Host | Out-Null
    exit 1
}

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Fail 'Git is not installed. Install Git for Windows, then run this file again.'
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Fail '.NET SDK is not installed. Install the .NET 8 SDK, then run this file again.'
}

if (-not (Test-Path $LiveSplit)) {
    Write-Host 'Downloading the LiveSplit source code...'
    git clone --recursive https://github.com/LiveSplit/LiveSplit.git $LiveSplit
} else {
    Write-Host 'Using the existing LiveSplit source folder.'
}

Write-Host 'Building the Leaderboard component...'
dotnet build $Project -c Release -p:LsRoot="$LiveSplit"
if ($LASTEXITCODE -ne 0) {
    Fail 'The build failed. Read the actual error above. The Developer Pack is only one possible cause.'
}

$Dll = Get-ChildItem -Path (Join-Path $Here 'src\LiveSplit.Leaderboard\bin\Release') -Filter 'LiveSplit.Leaderboard.dll' -Recurse | Select-Object -First 1
if (-not $Dll) {
    Fail 'Build completed, but the DLL could not be found.'
}

New-Item -ItemType Directory -Force -Path $Output | Out-Null
Copy-Item $Dll.FullName (Join-Path $Output $Dll.Name) -Force

Write-Host "`nFinished." -ForegroundColor Green
Write-Host "Your DLL is here:`n$Output\LiveSplit.Leaderboard.dll"
Start-Process explorer.exe $Output
Write-Host "`nCopy it into LiveSplit\Components and restart LiveSplit."
Write-Host "`nPress Enter to close..."
Read-Host | Out-Null
