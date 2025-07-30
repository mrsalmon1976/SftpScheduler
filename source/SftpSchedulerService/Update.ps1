Clear-Host
$ErrorActionPreference = "Stop"
$global:scriptRoot = $PSScriptRoot

# variables
$global:latestVersionUrl = "https://api.github.com/repos/mrsalmon1976/SftpScheduler/releases/latest"
$global:serviceExecutableName = "SftpSchedulerService.exe"
$global:tempFolder = "$global:scriptRoot\TempUpdate"
$global:tempExtractionFolder = "$global:scriptRoot\TempUpdate\Content"

function ExitWithError {
    param ([System.String]$msg) 
    LogMessage -msg $msg -level "ERROR"
    Exit 1
}
function GetCurrentVersion {
    $pathToExe = "$global:scriptRoot\$global:serviceExecutableName"
    if (!(Test-Path -Path $pathToExe)) {
        ExitWithError -msg "File '$pathToExe' does not exist"
    }

    $versionInfo = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($pathToExe)
    return [System.Version]::Parse($versionInfo.FileVersion).ToString(3);
}

function CreateTempFolder {
    $path = $global:tempFolder
    if (!(Test-Path -Path $path)) {
        New-Item -Path $path -ItemType Directory | Out-Null
    }
}

function DownloadRelease {
    param ([System.String]$url, [System.String]$outputPath) 
    Invoke-WebRequest -Uri $url -OutFile $outputPath    
}

function ExtractRelease {
    param ([System.String]$zipPath) 

    $path = $global:tempExtractionFolder

    # remove the folder if it exists
    if (Test-Path -Path $path) {
        Remove-Item -Path $path -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
    }

    Expand-Archive -Path $zipPath -DestinationPath $path
}

function GetLatestVersion {
    
    $data = Invoke-RestMethod -Uri $global:latestVersionUrl

    return [PSCustomObject]@{
        version = $data.tag_name.Trim().TrimStart('v')
        fileName = $data.assets[0].name
        downloadUrl = $data.assets[0].browser_download_url
    }    
}

function IsLatestVersionInstalled {
    param ([System.String]$installedVersion, [System.String]$latestVersion) 

    $vInstalled = [System.Version]::Parse($installedVersion);
    $vLatest = [System.Version]::Parse($latestVersion);
    return ($vInstalled -eq $vLatest)

}

function LogMessage {

    param ([System.String]$msg, [System.String]$level = "INFO") 

    $consoleText = "$(Get-Date) $msg"
    if ($level -eq "ERROR") {
        Write-Host $consoleText -BackgroundColor Red
    }
    elseif ($level -eq "WARN") {
        Write-Host $consoleText -ForegroundColor Yellow
    }
    else {
        Write-Host $consoleText
    }
    Add-Content -Path "$scriptRoot\updates.log" -Value "$(Get-Date)|$level|$msg"
}

function RemoveTempFolder {
    $path = $global:tempFolder
    if (Test-Path -Path $path) {
        Remove-Item -Path $path -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
    }
}

function Run {
    try {
        LogMessage -msg "----------------------------------------------------------------------"
        LogMessage -msg "Starting update initiated by user '$Env:UserName'"
        LogMessage -msg "----------------------------------------------------------------------"

        # get installed version
        $versionInstalled = GetCurrentVersion
        LogMessage -msg "Version installed: $versionInstalled"

        # get latest version - includes "version,fileName,downloadUrl"
        $latestVersionInfo = GetLatestVersion
        $versionLatest = $latestVersionInfo.version
        LogMessage -msg "Latest version available: $versionLatest"

        # if versions match - print and exit
        if (IsLatestVersionInstalled -installedVersion $versionInstalled -latestVersion $versionLatest) {
            LogMessage -msg "The latest version is already installed" -level "WARN"
            #Exit
        }

        # create temp folder for update
        CreateTempFolder
        LogMessage -msg "Created temporary folder '$global:tempFolder'"

        # download latest version if it has not been downloaded already
        $zipPath = "$global:tempFolder\$($latestVersionInfo.fileName)"
        if (Test-Path -Path $zipPath) {
            LogMessage -msg "Latest release already downloaded to '$zipPath'"
        }
        else {
            DownloadRelease -url $latestVersionInfo.downloadUrl -outputPath $zipPath
            LogMessage -msg "Downloaded latest release to '$zipPath'"
        }

        # todo: unzip latest version
        ExtractRelease -zipPath $zipPath
        LogMessage -msg "Release unzipped to '$global:tempExtractionFolder'"

        # todo: back up current service files into zip
        # todo: if service is installed, stop it
        # todo: delete current service files
        # todo: copy new service files into folder
        # todo: install the service if it is not installed
        # todo: start the service
        # todo: clean up temporary folder - delete everything in it and remove the folder
    }
    catch {
        $ex = $_.Exception

        ExitWithError("An error occurred: $($ex.Message)")
    }

}

Run