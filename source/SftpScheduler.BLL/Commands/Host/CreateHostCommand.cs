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
    public interface ICreateHostCommand
    {
        Task<HostEntity> ExecuteAsync(IDbContext dbContext, HostEntity hostEntity);
    }

    public class CreateHostCommand : ICreateHostCommand
    {
        private readonly HostValidator _hostValidator;
        private readonly IPasswordProvider _passwordProvider;

        public CreateHostCommand(HostValidator hostValidator, IPasswordProvider passwordProvider)
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

            hostEntity.Password = _passwordProvider.Encrypt(hostEntity.Password);

            string sql = @"INSERT INTO Host (Name, Host, Port, Username, Password, Created) VALUES (@Name, @Host, @Port, @Username, @Password, @Created)";
            await dbContext.ExecuteNonQueryAsync(sql, hostEntity);

            sql = @"select last_insert_rowid()";
            hostEntity.Id = await dbContext.ExecuteScalarAsync<int>(sql);
            hostEntity.Password = String.Empty;

            return hostEntity;
        }
    }
}
