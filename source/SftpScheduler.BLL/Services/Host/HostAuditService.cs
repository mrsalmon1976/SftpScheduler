using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Services.Host
{
    public interface IHostAuditService
    {
        IEnumerable<HostAuditLogEntity> CompareHosts(HostEntity currentHostEntity, HostEntity updatedHostEntity, string userName);
    }

    public class HostAuditService : IHostAuditService
    {

        public const string PasswordMask = "**********";

        public IEnumerable<HostAuditLogEntity> CompareHosts(HostEntity currentHostEntity, HostEntity updatedHostEntity, string userName)
        {
            List<HostAuditLogEntity> changes = new List<HostAuditLogEntity>();
            int hostId = currentHostEntity.Id;

            if (currentHostEntity.Name != updatedHostEntity.Name) 
            {
                changes.Add(new HostAuditLogEntity(hostId, "Name", currentHostEntity.Name, updatedHostEntity.Name, userName));
            }
            if (currentHostEntity.Host != updatedHostEntity.Host)
            {
                changes.Add(new HostAuditLogEntity(hostId, "Host", currentHostEntity.Host, updatedHostEntity.Host, userName));
            }
            if (currentHostEntity.Port != updatedHostEntity.Port)
            {
                changes.Add(new HostAuditLogEntity(hostId, "Port", (currentHostEntity.Port ?? 0).ToString(), (updatedHostEntity.Port ?? 0).ToString(), userName));
            }
            if (currentHostEntity.Username != updatedHostEntity.Username)
            {
                changes.Add(new HostAuditLogEntity(hostId, "Username", currentHostEntity.Username, updatedHostEntity.Username, userName));
            }
            if (updatedHostEntity.Password.Length > 0)
            {
                changes.Add(new HostAuditLogEntity(hostId, "Password", PasswordMask, PasswordMask, userName));
            }
            if (currentHostEntity.KeyFingerprint != updatedHostEntity.KeyFingerprint)
            {
                changes.Add(new HostAuditLogEntity(hostId, "KeyFingerprint", currentHostEntity.KeyFingerprint, updatedHostEntity.KeyFingerprint, userName));
            }

            return changes;
        }
    }
}
