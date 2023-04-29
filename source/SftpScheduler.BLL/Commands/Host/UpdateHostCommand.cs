﻿using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Security;
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
        Task<HostEntity> ExecuteAsync(IDbContext dbContext, HostEntity hostEntity);
    }

    public class UpdateHostCommand : IUpdateHostCommand
    {
        private readonly HostValidator _hostValidator;
        private readonly IPasswordProvider _passwordProvider;

        public UpdateHostCommand(HostValidator hostValidator, IPasswordProvider passwordProvider)
        {
            _hostValidator = hostValidator;
            _passwordProvider = passwordProvider;
        }

        public virtual async Task<HostEntity> ExecuteAsync(IDbContext dbContext, HostEntity hostEntity)
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
                hostEntity.Password = _passwordProvider.Encrypt(hostEntity.Password);
                passwordUpdateText = "@Password";
            }

            string sql = $@"UPDATE Host 
                SET Name = @Name
                , Host = @Host
                , Port = @Port
                , Username = @Username
                , Password = { passwordUpdateText }
                , KeyFingerprint = @KeyFingerprint
                WHERE Id = @Id";
            await dbContext.ExecuteNonQueryAsync(sql, hostEntity);

            hostEntity.Password = String.Empty;

            return hostEntity;
        }
    }
}