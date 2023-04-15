$ErrorActionPreference = "Stop"
Clear-Host

$root = $PSScriptRoot

& dotnet publish $root\..\source\SftpSchedulerService\SftpSchedulerService.csproj /p:EnvironmentName=Production /p:Configuration=Release --output $root\build
