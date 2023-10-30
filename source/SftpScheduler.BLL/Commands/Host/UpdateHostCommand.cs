using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Security;
using SftpScheduler.BLL.Services.Host;
using SftpScheduler.BLL.Utility;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Commands.Host
{
    public interface IUpdateHostCommand
    {
        Task<HostEntity> ExecuteAsync(IDbContext dbContext, HostEntity hostEntity, string userName);
    }

    public class UpdateHostCommand : IUpdateHostCommand
    {
        private readonly HostRepository _hostRepository;
        private readonly IHostValidator _hostValidator;
        private readonly IEncryptionProvider _encryptionProvider;
        private readonly IHostAuditService _hostAuditService;

        public UpdateHostCommand(HostRepository hostRepository, IHostValidator hostValidator, IEncryptionProvider encryptionProvider, IHostAuditService hostAuditService)
        {
            _hostRepository = hostRepository;
            _hostValidator = hostValidator;
            _encryptionProvider = encryptionProvider;
            _hostAuditService = hostAuditService;
        }

        public virtual async Task<HostEntity> ExecuteAsync(IDbContext dbContext, HostEntity hostEntity, string userName)
        {
            ValidationResult validationResult = _hostValidator.Validate(hostEntity);
            if (!validationResult.IsValid)
            {
                throw new DataValidationException("Host details supplied are invalid", validationResult);
            }

            bool isPasswordUpdated = (hostEntity.Password.Length > 0);
            string passwordUpdateText = "Password";

            if (isPasswordUpdated)
            {
                hostEntity.Password = _encryptionProvider.Encrypt(hostEntity.Password);
                passwordUpdateText = "@Password";
            }

            // run comparison to determine auditing
            HostEntity currentHostEntity = await _hostRepository.GetByIdAsync(dbContext, hostEntity.Id);
            var auditLogs = _hostAuditService.CompareHosts(currentHostEntity, hostEntity, userName);

            string sql = $@"UPDATE Host 
                SET Name = @Name
                , Host = @Host
                , Port = @Port
                , Username = @Username
                , Password = { passwordUpdateText }
                , KeyFingerprint = @KeyFingerprint
                WHERE Id = @Id";
            await dbContext.ExecuteNonQueryAsync(sql, hostEntity);

            // write audit logs
            sql = @"INSERT INTO HostAuditLog (HostId, PropertyName, FromValue, ToValue, UserName, Created) VALUES (@HostId, @PropertyName, @FromValue, @ToValue, @UserName, @Created)";
            foreach (HostAuditLogEntity hostAuditLogEntity in auditLogs)
            {
                await dbContext.ExecuteNonQueryAsync(sql, hostAuditLogEntity);
            }

            // clear the password from the hostEntity
            hostEntity.Password = String.Empty;

            return hostEntity;
        }


    }
}
