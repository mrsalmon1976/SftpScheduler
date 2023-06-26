$ErrorActionPreference = "Stop"
Clear-Host

function RemoveRedundantFiles
{
	param([string]$buildPath)

	Remove-Item -Path "$buildPath\appsettings.Development.json" -Force
	Remove-Item -Path "$buildPath\Updater\appsettings.Development.json" -Force
	Remove-Item -Path "$buildPath\da" -Force -Recurse 
	Remove-Item -Path "$buildPath\de" -Force -Recurse 
	Remove-Item -Path "$buildPath\es" -Force -Recurse 
	Remove-Item -Path "$buildPath\es-MX" -Force -Recurse 
	Remove-Item -Path "$buildPath\fa" -Force -Recurse 
	Remove-Item -Path "$buildPath\fi" -Force -Recurse 
	Remove-Item -Path "$buildPath\fr" -Force -Recurse 
	Remove-Item -Path "$buildPath\he-IL" -Force -Recurse 
	Remove-Item -Path "$buildPath\it" -Force -Recurse 
	Remove-Item -Path "$buildPath\ja" -Force -Recurse 
	Remove-Item -Path "$buildPath\ko" -Force -Recurse 
	Remove-Item -Path "$buildPath\nb" -Force -Recurse 
	Remove-Item -Path "$buildPath\nl" -Force -Recurse 
	Remove-Item -Path "$buildPath\pl" -Force -Recurse 
	Remove-Item -Path "$buildPath\pt" -Force -Recurse 
	Remove-Item -Path "$buildPath\ro" -Force -Recurse 
	Remove-Item -Path "$buildPath\ru" -Force -Recurse 
	Remove-Item -Path "$buildPath\sl" -Force -Recurse 
	Remove-Item -Path "$buildPath\sv" -Force -Recurse 
	Remove-Item -Path "$buildPath\tr" -Force -Recurse 
	Remove-Item -Path "$buildPath\uk" -Force -Recurse 
	Remove-Item -Path "$buildPath\zh-Hans" -Force -Recurse 
	Remove-Item -Path "$buildPath\zh-Hant" -Force -Recurse 
}

function UpdateProjectVersion
{
	param([string]$filePath, [string]$version)

	if (!(Test-Path -Path $filePath)) {
		throw "$filePath does not exist - unable to update project file"
	}

	$doc = New-Object System.Xml.XmlDocument
	$doc.Load($filePath)
	UpdateXmlNodeIfExists -xmlDoc $doc -xpath "//PropertyGroup/Version" -newValue $version
	UpdateXmlNodeIfExists -xmlDoc $doc -xpath "//PropertyGroup/AssemblyVersion" -newValue $version
	UpdateXmlNodeIfExists -xmlDoc $doc -xpath "//PropertyGroup/FileVersion" -newValue $version
	UpdateXmlNodeIfExists -xmlDoc $doc -xpath "//package/metadata/version" -newValue $version
	$doc.Save($filePath)
}

function UpdateXmlNodeIfExists
{
	param($xmlDoc, $xpath, $newValue)
	$node = $xmlDoc.SelectSingleNode($xpath)
	if ($null -ne $node)
	{
		$node.InnerText = $newValue
	}
}

function ZipFile
{
	param(
		[String]$sourceFile,
		[String]$zipFile
	)

	$exeloc = ""
	if (Test-Path -Path "C:\Program Files\7-Zip\7z.exe") {
		$exeloc = "C:\Program Files\7-Zip\7z.exe"
	}
	elseif (Test-Path -Path "C:\Program Files (x86)\7-Zip\7z.exe") {
		$exeloc = "C:\Program Files (x86)\7-Zip\7z.exe"
	}
	else {
		Write-Host "Unable to find 7-zip executable" -BackgroundColor Red -ForegroundColor White
		Exit 1
	}

	set-alias sz $exeloc  
	sz a -xr!'Data\users.json' -tzip -r $zipFile $sourceFile | Out-Null
}

$rootPath = $PSScriptRoot
$sourcePath = $rootPath.Replace("deployment", "") + "source"
$buildPath = "$rootPath\build"
$version = Read-Host -Prompt "What version are we building? [e.g. 2.3.0]"

# ensure the build folder exists and is empty
Write-Host "Removing previous build files"
Remove-Item -Force -Recurse -Path $buildPath 
New-Item -ItemType Directory -Force -Path $buildPath

Write-Host "Updating project files for version $version"
UpdateProjectVersion -filePath "$sourcePath\SftpSchedulerService\SftpSchedulerService.csproj" -version $version
UpdateProjectVersion -filePath "$sourcePath\SftpSchedulerServiceUpdater\SftpSchedulerServiceUpdater.csproj" -version $version

Write-Host "Building version $version"
& dotnet publish $sourcePath\SftpSchedulerService\SftpSchedulerService.csproj /p:EnvironmentName=Production /p:Configuration=Release --output $buildPath
& dotnet publish $sourcePath\SftpSchedulerServiceUpdater\SftpSchedulerServiceUpdater.csproj /p:EnvironmentName=Production /p:Configuration=Release --output "$buildPath\Updater"

Write-Host "Removing redundant files"
RemoveRedundantFiles -buildPath $buildPath

# package it up 
$zipPath = "$rootPath\SftpScheduler_v$version.zip"
Write-Host "Packaging SftpScheduler version $version into $zipPath"
Remove-Item -Path $zipPath -Force -ErrorAction Ignore
ZipFile -sourcefile "$buildPath\*.*" -zipfile $zipPath 

Write-Host "========== BUILD SUCCESS ==========" -BackgroundColor Green -ForegroundColor White
