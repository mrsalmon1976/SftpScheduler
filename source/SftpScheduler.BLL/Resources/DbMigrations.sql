﻿CREATE TABLE IF NOT EXISTS Host (
	Id INTEGER PRIMARY KEY,
	Name TEXT NOT NULL,
	Host TEXT NOT NULL,
	Port INTEGER NOT NULL,
	Username TEXT NOT NULL,
	Password TEXT NOT NULL,
	KeyFingerprint TEXT NULL,
	Created TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS HostAuditLog (
	Id INTEGER PRIMARY KEY,
	HostId INTEGER NOT NULL,
	PropertyName TEXT NOT NULL,
	FromValue TEXT NOT NULL,
	ToValue TEXT NOT NULL,
	UserName TEXT NOT NULL,
	Created TEXT NOT NULL,
	FOREIGN KEY(HostId) REFERENCES Host(Id)
);
CREATE INDEX IF NOT EXISTS IX_HostAuditLog_HostId ON HostAuditLog (HostId);

CREATE TABLE IF NOT EXISTS Job (
	Id INTEGER PRIMARY KEY,
	Name TEXT NOT NULL,
	HostId TEXT NOT NULL,
	Type INTEGER NOT NULL,
	Schedule TEXT NOT NULL,
	ScheduleInWords TEXT NOT NULL,
	LocalPath TEXT NOT NULL,
	RemotePath TEXT NOT NULL,
	DeleteAfterDownload INTEGER NULL,
	RemoteArchivePath TEXT NULL,
	LocalCopyPaths TEXT NULL,
	IsEnabled INTEGER NOT NULL,
	Created TEXT NOT NULL,
	FOREIGN KEY(HostId) REFERENCES Host(Id)
);
CREATE TABLE IF NOT EXISTS JobLog (
	Id INTEGER PRIMARY KEY,
	JobId INTEGER NOT NULL,
	StartDate TEXT NOT NULL,
	EndDate TEXT NULL,
	Progress INTEGER NOT NULL,
	Status TEXT NOT NULL,
	ErrorMessage TEXT NULL,
	FOREIGN KEY(JobId) REFERENCES Job(Id)
);
CREATE INDEX IF NOT EXISTS IX_JobLog_StartDate ON JobLog (StartDate);
CREATE INDEX IF NOT EXISTS IX_JobLog_JobId ON JobLog (JobId);

CREATE TABLE IF NOT EXISTS AspNetRoles (
	Id TEXT PRIMARY KEY,
	ConcurrencyStamp TEXT NULL,
	Name TEXT NULL,
	NormalizedName TEXT NULL
);
CREATE TABLE IF NOT EXISTS AspNetRoleClaims (
	Id INTEGER PRIMARY KEY AUTOINCREMENT,
	ClaimType TEXT NULL,
	ClaimValue TEXT NULL,
	RoleId TEXT NOT NULL,
	FOREIGN KEY(RoleId) REFERENCES AspNetRoles(Id)
);
CREATE TABLE IF NOT EXISTS AspNetUsers (
	Id TEXT PRIMARY KEY,
	AccessFailedCount INTEGER NOT NULL,
	ConcurrencyStamp TEXT NULL,
	Email TEXT NULL,
	EmailConfirmed INTEGER NOT NULL,
	LockoutEnabled INTEGER NOT NULL,
	LockoutEnd TEXT NULL,
	NormalizedEmail TEXT NULL,
	NormalizedUserName TEXT NULL,
	PasswordHash TEXT NULL,
	PhoneNumber TEXT NULL,
	PhoneNumberConfirmed INTEGER NOT NULL,
	SecurityStamp TEXT NULL,
	TwoFactorEnabled INTEGER NOT NULL,
	UserName TEXT NULL
);
CREATE TABLE IF NOT EXISTS AspNetUserClaims (
	Id INTEGER PRIMARY KEY AUTOINCREMENT,
	ClaimType TEXT NULL,
	ClaimValue TEXT NULL,
	UserId TEXT NOT NULL,
	FOREIGN KEY(UserId) REFERENCES AspNetUsers(Id)
);
CREATE TABLE IF NOT EXISTS AspNetUserLogins (
	LoginProvider TEXT PRIMARY KEY,
	ProviderKey TEXT NOT NULL,
	ProviderDisplayName TEXT NULL,
	UserId TEXT NOT NULL,
	FOREIGN KEY(UserId) REFERENCES AspNetUsers(Id)
);
CREATE TABLE IF NOT EXISTS AspNetUserRoles (
	UserId TEXT NOT NULL,
	RoleId TEXT NOT NULL,
	PRIMARY KEY(UserId, RoleId)
	FOREIGN KEY(UserId) REFERENCES AspNetUsers(Id),
	FOREIGN KEY(RoleId) REFERENCES AspNetRoles(Id)
);
CREATE TABLE IF NOT EXISTS AspNetUserTokens (
	UserId TEXT NOT NULL,
	LoginProvider TEXT NOT NULL,
	Name TEXT NOT NULL,
	Value TEXT NULL,
	PRIMARY KEY(UserId, LoginProvider, Name)
	FOREIGN KEY(UserId) REFERENCES AspNetUsers(Id)
);
CREATE TABLE IF NOT EXISTS GlobalUserSetting (
	Id TEXT NOT NULL,
	SettingValue TEXT NOT NULL,
	PRIMARY KEY(Id)
);
CREATE TABLE IF NOT EXISTS JobFileLog (
	Id INTEGER PRIMARY KEY,
	JobId INTEGER NOT NULL,
	FileName TEXT NOT NULL,
	FileLength INTEGER NOT NULL,
	StartDate TEXT NOT NULL,
	EndDate TEXT NULL,
	FOREIGN KEY(JobId) REFERENCES Job(Id)
);
CREATE INDEX IF NOT EXISTS IX_JobFileLog_JobId ON JobFileLog (JobId);
