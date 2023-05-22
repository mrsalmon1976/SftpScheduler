 ECHO Installing to %CD%
 sc.exe create SftpScheduler binpath= "%CD%\SftpSchedulerService.exe" start= auto
 sc.exe start SftpScheduler
 PAUSE