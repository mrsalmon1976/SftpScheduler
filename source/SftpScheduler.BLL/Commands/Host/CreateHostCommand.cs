using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Security;
using SftpScheduler.BLL.Validators;

namespace SftpScheduler.BLL.Commands.Host
{
    public interface ICreateHostCommand
    {
        Task<HostEntity> ExecuteAsync(IDbContext dbContext, HostEntity hostEntity);
    }

    public class CreateHostCommand : ICreateHostCommand
    {
        private readonly IHostValidator _hostValidator;
        private readonly IEncryptionProvider _encryptionProvider;

        public CreateHostCommand(IHostValidator hostValidator, IEncryptionProvider encryptionProvider)
        {
            _hostValidator = hostValidator;
            _encryptionProvider = encryptionProvider;
        }

        public virtual async Task<HostEntity> ExecuteAsync(IDbContext dbContext, HostEntity hostEntity)
        {
            ValidationResult validationResult = _hostValidator.Validate(hostEntity);
            if (!validationResult.IsValid)
            {
                throw new DataValidationException("Host details supplied are invalid", validationResult);
            }

            hostEntity.Password = _encryptionProvider.Encrypt(hostEntity.Password);

            string sql = @"INSERT INTO Host (Name, Host, Port, Username, Password, KeyFingerprint, Created, Protocol, FtpsMode) VALUES (@Name, @Host, @Port, @Username, @Password, @KeyFingerprint, @Created, @Protocol, @FtpsMode)";
            await dbContext.ExecuteNonQueryAsync(sql, hostEntity);

            sql = @"select last_insert_rowid()";
            hostEntity.Id = await dbContext.ExecuteScalarAsync<int>(sql);
            hostEntity.Password = String.Empty;

            return hostEntity;
        }
    }
}
