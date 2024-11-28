SftpScheduler
==================

SftpScheduler is a web-based tool used to schedule SFTP jobs. It can be installed as a Windows service on a company server, and is accessed via a browser.  Hosts and jobs can be configured with schedules, and if you have access to an SMTP server, notifications can be configured for repeated job failures.

This is useful if your company creates a large number of scheduled SFTP up/downloads.

[![Build status](https://ci.appveyor.com/api/projects/status/snreekxcc215u4if?svg=true)](https://ci.appveyor.com/project/mrsalmon1976/sftpscheduler)

# Features

- Create a singular set of hosts
- Supports SFTP, FTPS and FTP
- Passwords are encrypted
- Flexible scheduling via CRON
- Create multiple upload jobs, monitoring local folders
- Create multiple download jobs, monitoring remote folders - download jobs can be configured to delete downloaded files, or move them into a remote archive path
- Option to copy downloaded files into additional local folders
- Zipping of files before upload
- Full history of downloads and job success
- Notifications for repeated failures, and configurable daily digest emails containing details of failing jobs from the past 24 hours (requires SMTP server)
- Configurable user list
- Audit logs of host and job changes
- HTTP or HTTPS support - see Configuration section below for details on how to configure for HTTPS

# Installation

## First Time and/or Manual Installation

- Download the latest release.
- Extract the contents of the zip folder, and move them to the installation directory.
- Run "install.bat" as administrator - this will install SftpScheduler as a service on the machine.
- You may want to edit the installed service properties through the Windows services console and adjust the service user.  By default, running as SYSTEM may not have the permissions required to access folders on your machine or network, so you may want to set the Logon user as a local administrator or a domain user.

## Updates

- SftpScheduler will automatically update when new versions are released.
- If this update fails, check that your anti-virus is not quarantining the update files, and add the folder as an exclusion if necessary.
- To manually update, follow the steps in "First Time and/or Manual Installation" section.  Make sure you do not delete the "Data" folder - this contains all pevious settings, hosts and jobs.

## Configuration

After installation, there are some configurable settings that can be adjusted in the file `Data\startup.settings.json`.  Note that these settings can also be adjusted within the application, but the service will need to be restarted before they take effect.

| **Setting Name**        | **Setting Value** |
| -                       | -                 |
| *CertificatePath*       | The path to a certificate for SSL support.  This must be the absolute location of the .pfx file, e.g. `C:\\MyFolder\\MyCertificate.pfx`.  Note that this is a JSON document, so path separators must be `\\`.  Permissions on the file must allow the service user access to load the .pfx file, otherwise the application will default back to HTTP.  If this is set, HTTP will not be available. |
| *CertificatePassword*  | Accompanies the `CertificatePath` - this is the password for the .pfx file.
| *MaxConcurrentJobs*    | The maximum number of jobs that can be run concurrently - defaults to 2.  Be careful not to set this too high! More is not always better. |
| *Port*                 | The port that the application uses - defaults to 8642.  Note that the application will only ever occupy a single port (either over HTTP, or HTTPS if a certificate is configured) |

# Screenshots

![Dashboard screenshot](/img/screenshots/screenshot_dashboard.png?raw=true "Dashboard screenshot")

![Jobs screenshot](/img/screenshots/screenshot_jobs.png?raw=true "Jobs screenshot")

![Job log screenshot](/img/screenshots/screenshot_job_log.png?raw=true "Job log screenshot")




