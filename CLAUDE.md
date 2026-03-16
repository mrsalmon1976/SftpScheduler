# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What This Project Is

SftpScheduler is an ASP.NET Core 6.0 Windows service with a web-based admin UI for scheduling SFTP/FTPS/FTP file transfers. It uses Quartz for job scheduling, WinSCP for protocol handling, SQLite for persistence, and Vue.js + AdminLTE for the frontend.

## Commands

### Build
```bash
dotnet build source/SftpScheduler.sln
```

Release build with deployment package:
```powershell
cd deployment
.\build.ps1  # prompts for version number, outputs SftpScheduler_v{version}.zip
```

### Run Tests
```bash
# All tests
dotnet test source/SftpScheduler.sln

# Single project
dotnet test source/SftpScheduler.BLL.Tests/SftpScheduler.BLL.Tests.csproj

# Single test class or method
dotnet test source/SftpScheduler.BLL.Tests/SftpScheduler.BLL.Tests.csproj --filter "FullyQualifiedName~<TestClassName>"
```

### Run the Application
```bash
dotnet run --project source/SftpSchedulerService/SftpSchedulerService.csproj
```
Access at `http://localhost:8642` (default credentials: admin/admin).

## Architecture

### Layer Structure

```
SftpSchedulerService          # ASP.NET Core host: controllers, Razor views, wwwroot
SftpScheduler.BLL             # All business logic: commands, services, jobs, repositories, data
SftpScheduler.Common          # Cross-cutting utilities: IO, HTTP, diagnostics
```

### Key Patterns

**Command pattern** — All write operations go through command classes in `SftpScheduler.BLL/Commands/`. Commands are thin wrappers that validate input and delegate to repositories or services.

**Repository pattern** — `SftpScheduler.BLL/Repositories/` contains Dapper-based repositories backed by SQLite. Entity Framework Core is used only for ASP.NET Identity tables.

**Two databases** — `SftpScheduler.db` (application data via Dapper) and `SftpScheduler_Quartz.db` (Quartz job persistence). Both are SQLite files in the `Data/` directory.

**Job execution flow** — Quartz triggers `TransferJob` → command classes → `FileTransferService` (WinSCP) → optional compression, archiving, and SMTP notifications.

### Test Infrastructure

Each project has a corresponding `*.Tests` project and `*.TestInfrastructure` project. The `SftpScheduler.BLL.Tests.Builders/` project provides a builder pattern for test data. NSubstitute is used for mocking.

### Configuration

Runtime settings live in `Data/startup.settings.json` (port, TLS certificate, max concurrent jobs). Application config is in `appsettings.json`. The `Data/` folder must be preserved across updates — it contains the databases and startup settings.
