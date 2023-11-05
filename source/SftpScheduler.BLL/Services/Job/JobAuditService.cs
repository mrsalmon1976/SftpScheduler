using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Services.Job
{
    public interface IJobAuditService
    {
        Task<IEnumerable<JobAuditLogEntity>> CompareJobs(IDbContext dbContext, JobEntity currentJobEntity, JobEntity updatedJobEntity, string userName);
    }

    public class JobAuditService : IJobAuditService
    {
        private readonly HostRepository _hostRepo;

        public JobAuditService(HostRepository hostRepo) 
        {
            _hostRepo = hostRepo;
        }

        public async Task<IEnumerable<JobAuditLogEntity>> CompareJobs(IDbContext dbContext, JobEntity currentJobEntity, JobEntity updatedJobEntity, string userName)
        {
            List<JobAuditLogEntity> changes = new List<JobAuditLogEntity>();
            int hostId = currentJobEntity.Id;

            if (currentJobEntity.Name != updatedJobEntity.Name) 
            {
                changes.Add(new JobAuditLogEntity(hostId, "Name", currentJobEntity.Name, updatedJobEntity.Name, userName));
            }
            if (currentJobEntity.HostId != updatedJobEntity.HostId)
            {
                HostEntity currentHostEntity = await _hostRepo.GetByIdAsync(dbContext, currentJobEntity.HostId);
                HostEntity updatedHostEntity = await _hostRepo.GetByIdAsync(dbContext, updatedJobEntity.HostId);
                changes.Add(new JobAuditLogEntity(hostId, "Host", $"{currentHostEntity.Name}", $"{updatedHostEntity.Name}", userName));
            }
            if (currentJobEntity.Type != updatedJobEntity.Type)
            {
                changes.Add(new JobAuditLogEntity(hostId, "JobType", currentJobEntity.Type.ToString(), updatedJobEntity.Type.ToString(), userName));
            }
            if (currentJobEntity.Schedule != updatedJobEntity.Schedule)
            {
                changes.Add(new JobAuditLogEntity(hostId, "Schedule", currentJobEntity.Schedule, updatedJobEntity.Schedule, userName));
            }
            if (currentJobEntity.LocalPath != updatedJobEntity.LocalPath)
            {
                changes.Add(new JobAuditLogEntity(hostId, "LocalPath", currentJobEntity.LocalPath, updatedJobEntity.LocalPath, userName));
            }
            if (currentJobEntity.RemotePath != updatedJobEntity.RemotePath)
            {
                changes.Add(new JobAuditLogEntity(hostId, "RemotePath", currentJobEntity.RemotePath, updatedJobEntity.RemotePath, userName));
            }
            if (currentJobEntity.DeleteAfterDownload != updatedJobEntity.DeleteAfterDownload)
            {
                changes.Add(new JobAuditLogEntity(hostId, "DeleteAfterDownload", HumanizeBoolean(currentJobEntity.DeleteAfterDownload), HumanizeBoolean(updatedJobEntity.DeleteAfterDownload), userName));
            }
            if (currentJobEntity.RemoteArchivePath != updatedJobEntity.RemoteArchivePath)
            {
                changes.Add(new JobAuditLogEntity(hostId, "RemoteArchivePath", currentJobEntity.RemoteArchivePath, updatedJobEntity.RemoteArchivePath, userName));
            }
            if (currentJobEntity.LocalCopyPaths != updatedJobEntity.LocalCopyPaths)
            {
                changes.Add(new JobAuditLogEntity(hostId, "LocalCopyPaths", currentJobEntity.LocalCopyPaths, updatedJobEntity.LocalCopyPaths, userName));
            }
            if (currentJobEntity.IsEnabled != updatedJobEntity.IsEnabled)
            {
                changes.Add(new JobAuditLogEntity(hostId, "Enabled", HumanizeBoolean(currentJobEntity.IsEnabled), HumanizeBoolean(updatedJobEntity.IsEnabled), userName));
            }

            return changes;
        }

        private string HumanizeBoolean(bool booleanVal)
        {
            return (booleanVal ? "True" : "False");
        }
    }
}
