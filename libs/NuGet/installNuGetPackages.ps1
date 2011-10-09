#variables for development
#Set-Location "the folder of a solution you want to install NuGet packages for"
#$nuGetPath = "relative path from solution folder to NuGet.exe"
#$packageInstallationFolder = "relative path to desired package install location"

param(
	[string]$nuGetPath,
	[string]$packageInstallationFolder
)
Get-Item "**\packages.config" `
	| ForEach-Object {
		$packagesConfig = $_.FullName
		Write-Host "installing NuGet packages from: $packagesConfig" `
			-BackgroundColor Black `
			-ForegroundColor Yellow
		Invoke-Expression "$nuGetPath install '$packagesConfig' -o '$packageInstallationFolder'" `
			| Out-Host
	}