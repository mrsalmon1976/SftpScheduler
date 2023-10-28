SftpScheduler
==================

SftpScheduler is a web-based tool used to schedule SFTP jobs. It can be installed as a Windows service on a company server, and is accessed via a browser.  Hosts and jobs can be configured with schedules, and if you have access to an SMTP server, notifications can be configured for repeated job failures.

This is useful if your company creates a large number of scheduled SFTP up/downloads.

[![Build status](https://ci.appveyor.com/api/projects/status/snreekxcc215u4if?svg=true)](https://ci.appveyor.com/project/mrsalmon1976/sftpscheduler)

## Features

- Create a singular set of hosts
- Passwords are encrypted
- Flexible scheduling via CRON
- Create multiple upload jobs, monitoring local folders
- Create multiple download jobs, monitoring remote folders - download jobs can be configured to delete downloaded files, or move them into a remote archive path
- Option to copy downloaded files into additional local folders
- Full history of downloads and job success
- Notifications for repeated failures, and configurable daily digest emails containing details of failing jobs from the past 24 hours
- Configurable user list

## Screenshots

![Dashboard screenshot](/img/screenshots/screenshot_dashboard.png?raw=true "Dashboard screenshot")

![Jobs screenshot](/img/screenshots/screenshot_jobs.png?raw=true "Jobs screenshot")

![Job log screenshot](/img/screenshots/screenshot_job_log.png?raw=true "Job log screenshot")




