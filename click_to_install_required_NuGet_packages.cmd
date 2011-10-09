powershell $ep = Get-ExecutionPolicy; Set-ExecutionPolicy Unrestricted; .\libs\NuGet\installNuGetPackages.ps1 .\libs\NuGet\NuGet.exe .\packages; Set-ExecutionPolicy $ep
PAUSE