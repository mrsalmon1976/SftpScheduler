$ErrorActionPreference = "Stop"
Clear-Host

function Remove-Item-IfExists 
{
	param([string]$path)

	if (Test-Path -Path $path) {
		Remove-Item -Path $path -Force -Recurse
	}
}

function RemoveRedundantFiles
{
	param([string]$buildPath)

	Remove-Item-IfExists -Path "$buildPath\appsettings.Development.json" 
	Remove-Item-IfExists -Path "$buildPath\af" 
	Remove-Item-IfExists -Path "$buildPath\ar" 
	Remove-Item-IfExists -Path "$buildPath\az" 
	Remove-Item-IfExists -Path "$buildPath\bg" 
	Remove-Item-IfExists -Path "$buildPath\bn-BD"
	Remove-Item-IfExists -Path "$buildPath\cs" 
	Remove-Item-IfExists -Path "$buildPath\da" 
	Remove-Item-IfExists -Path "$buildPath\de" 
	Remove-Item-IfExists -Path "$buildPath\el" 
	Remove-Item-IfExists -Path "$buildPath\es" 
	Remove-Item-IfExists -Path "$buildPath\es-MX"
	Remove-Item-IfExists -Path "$buildPath\fa"  
	Remove-Item-IfExists -Path "$buildPath\fi"  
	Remove-Item-IfExists -Path "$buildPath\fi-FI" 
	Remove-Item-IfExists -Path "$buildPath\fr-BE" 
	Remove-Item-IfExists -Path "$buildPath\fr" 
	Remove-Item-IfExists -Path "$buildPath\fr-BE"
	Remove-Item-IfExists -Path "$buildPath\he" 
	Remove-Item-IfExists -Path "$buildPath\he-IL" 
	Remove-Item-IfExists -Path "$buildPath\hr"  
	Remove-Item-IfExists -Path "$buildPath\hu"  
	Remove-Item-IfExists -Path "$buildPath\hy"  
	Remove-Item-IfExists -Path "$buildPath\id"  
	Remove-Item-IfExists -Path "$buildPath\is"  
	Remove-Item-IfExists -Path "$buildPath\it"  
	Remove-Item-IfExists -Path "$buildPath\ja"  
	Remove-Item-IfExists -Path "$buildPath\ko"  
	Remove-Item-IfExists -Path "$buildPath\ko-KR" 
	Remove-Item-IfExists -Path "$buildPath\ku" 
	Remove-Item-IfExists -Path "$buildPath\lv" 
	Remove-Item-IfExists -Path "$buildPath\ms-MY"
	Remove-Item-IfExists -Path "$buildPath\mt" 
	Remove-Item-IfExists -Path "$buildPath\nb" 
	Remove-Item-IfExists -Path "$buildPath\nb-NO"
	Remove-Item-IfExists -Path "$buildPath\nl" 
	Remove-Item-IfExists -Path "$buildPath\pl" 
	Remove-Item-IfExists -Path "$buildPath\pt" 
	Remove-Item-IfExists -Path "$buildPath\ro"  
	Remove-Item-IfExists -Path "$buildPath\ru"  
	Remove-Item-IfExists -Path "$buildPath\sl"  
	Remove-Item-IfExists -Path "$buildPath\sk" 
	Remove-Item-IfExists -Path "$buildPath\sr"  
	Remove-Item-IfExists -Path "$buildPath\sr-Latn"
	Remove-Item-IfExists -Path "$buildPath\sv" 
	Remove-Item-IfExists -Path "$buildPath\th-TH"
	Remove-Item-IfExists -Path "$buildPath\th-TN"
	Remove-Item-IfExists -Path "$buildPath\tr" 
	Remove-Item-IfExists -Path "$buildPath\uk" 
	Remove-Item-IfExists -Path "$buildPath\uz-Cyrl-UZ" 
	Remove-Item-IfExists -Path "$buildPath\uz-Latn-UZ"
	Remove-Item-IfExists -Path "$buildPath\vi" 
	Remove-Item-IfExists -Path "$buildPath\zh-Hans" 
	Remove-Item-IfExists -Path "$buildPath\zh-CN" 
	Remove-Item-IfExists -Path "$buildPath\zh-Hant" 

	Remove-Item-IfExists -Path "$buildPath\Data\startup.settings.json" 

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
if (Test-Path -Path $buildPath) {
	Remove-Item -Force -Recurse -Path $buildPath 
}
New-Item -ItemType Directory -Force -Path $buildPath

Write-Host "Updating project files for version $version"
UpdateProjectVersion -filePath "$sourcePath\SftpSchedulerService\SftpSchedulerService.csproj" -version $version

Write-Host "Building version $version"
& dotnet publish $sourcePath\SftpSchedulerService\SftpSchedulerService.csproj /p:EnvironmentName=Production /p:Configuration=Release --output $buildPath

Write-Host "Removing redundant files"
RemoveRedundantFiles -buildPath $buildPath

# package it up 
$zipPath = "$rootPath\SftpScheduler_v$version.zip"
Write-Host "Packaging SftpScheduler version $version into $zipPath"
Remove-Item -Path $zipPath -Force -ErrorAction Ignore
ZipFile -sourcefile "$buildPath\*.*" -zipfile $zipPath 

Write-Host "========== BUILD SUCCESS ==========" -BackgroundColor Green -ForegroundColor White
