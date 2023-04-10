using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Routing.AutoValues;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Utility.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Commands.Transfer
{
    [TestFixture]
    public class TransferCommandTests
    {
        #region Execute Tests

        [Test]
        public void Execute_SuccessfulTransfer_ProgressCallBackSetAndCleared()
        {
            ISessionWrapperFactory sessionWrapperFactory = Substitute.For<ISessionWrapperFactory>();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapperFactory.CreateSession(Arg.Any<HostEntity>(), Arg.Any<JobEntity>()).Returns(sessionWrapper);
            IDbContext dbContext = Substitute.For<IDbContext>();

            IFileTransferService fileTransferService = Substitute.For<IFileTransferService>();
            string[] availableFiles = { "C:\\Temp\\1.zip" };
            fileTransferService.UploadFilesAvailable(Arg.Any<string>()).Returns(availableFiles);


            ITransferCommand transferCommand = CreateTransferCommand(sessionWrapperFactory, fileTransferService);
            transferCommand.Execute(dbContext, 1);

            sessionWrapper.Received(2).ProgressCallback = Arg.Any<Action<int>>();
            Assert.That(sessionWrapper.ProgressCallback, Is.Null);
        }

        [Test]
        public void Execute_FailedTransfer_ProgressCallBackSetAndCleared()
        {
            int jobId = Faker.RandomNumber.Next(1000);
            ISessionWrapperFactory sessionWrapperFactory = Substitute.For<ISessionWrapperFactory>();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapperFactory.CreateSession(Arg.Any<HostEntity>(), Arg.Any<JobEntity>()).Returns(sessionWrapper);
            IDbContext dbContext = Substitute.For<IDbContext>();

            JobEntity jobEntity = EntityTestHelper.CreateJobEntity(jobId);
            jobEntity.Type = JobType.Upload;
            JobRepository jobRepo = Substitute.For<JobRepository>();
            jobRepo.GetByIdAsync(dbContext, Arg.Any<int>()).Returns(Task.FromResult(jobEntity));

            IFileTransferService fileTransferService = Substitute.For<IFileTransferService>();
            string[] availableFiles = { "C:\\Temp\\1.zip" };
            fileTransferService.UploadFilesAvailable(Arg.Any<string>()).Returns(availableFiles);

            fileTransferService.When(x => x.UploadFiles(Arg.Any<ISessionWrapper>(), Arg.Any<IEnumerable<string>>(), Arg.Any<string>())).Throw<Exception>();

            // execute
            ITransferCommand transferCommand = CreateTransferCommand(sessionWrapperFactory, fileTransferService, jobRepository: jobRepo);
            try
            {
                transferCommand.Execute(dbContext, jobId);
            }
            catch (Exception)
            {
                // expected exception
            }

            // assert
            fileTransferService.Received(1).UploadFiles(sessionWrapper, Arg.Any<IEnumerable<string>>(), jobEntity.RemotePath);
            sessionWrapper.Received(2).ProgressCallback = Arg.Any<Action<int>>();
            Assert.That(sessionWrapper.ProgressCallback, Is.Null);
        }

        [Test]
        public void Execute_UploadJob_UploadsCorrectly()
        {
            int jobId = Faker.RandomNumber.Next(10, 100);    
            ISessionWrapperFactory sessionWrapperFactory = Substitute.For<ISessionWrapperFactory>();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapperFactory.CreateSession(Arg.Any<HostEntity>(), Arg.Any<JobEntity>()).Returns(sessionWrapper);
            IDbContext dbContext = Substitute.For<IDbContext>();

            JobEntity jobEntity = EntityTestHelper.CreateJobEntity();
            jobEntity.Type = JobType.Upload;
            JobRepository jobRepo = Substitute.For<JobRepository>();
            jobRepo.GetByIdAsync(dbContext, jobId).Returns(Task.FromResult(jobEntity));

            IFileTransferService fileTransferService = Substitute.For<IFileTransferService>();
            string[] availableFiles =  { "C:\\Temp\\1.zip" };
            fileTransferService.UploadFilesAvailable(Arg.Any<string>()).Returns(availableFiles);

            // execute
            ITransferCommand transferCommand = CreateTransferCommand(sessionWrapperFactory, fileTransferService, jobRepository: jobRepo);
            transferCommand.Execute(dbContext, jobId);

            // assert
            fileTransferService.Received(1).UploadFiles(sessionWrapper, Arg.Any<IEnumerable<string>>(), jobEntity.RemotePath);
        }

        [Test]
        public void Execute_ExecuteJob_ClosesSession()
        {
            ISessionWrapperFactory sessionWrapperFactory = Substitute.For<ISessionWrapperFactory>();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapperFactory.CreateSession(Arg.Any<HostEntity>(), Arg.Any<JobEntity>()).Returns(sessionWrapper);
            IDbContext dbContext = Substitute.For<IDbContext>();

            IFileTransferService fileTransferService = Substitute.For<IFileTransferService>();
            fileTransferService.UploadFilesAvailable(Arg.Any<string>()).Returns(new string[] { "C:\\Temp\\1.zip" });

            ITransferCommand transferCommand = CreateTransferCommand(sessionWrapperFactory, fileTransferService);
            transferCommand.Execute(dbContext, 1);

            sessionWrapper.Received(1).Open();
            sessionWrapper.Received(1).Close();
        }

        #endregion

        #region Private Methods

        private ITransferCommand CreateTransferCommand(ISessionWrapperFactory sessionWrapperFactory
            , IFileTransferService fileTransferService
            , JobRepository? jobRepository = null
            , HostRepository? hostRepository = null)
        {
            return new TransferCommand(Substitute.For<ILogger<TransferCommand>>()
                , sessionWrapperFactory
                , fileTransferService 
                , jobRepository ?? Substitute.For<JobRepository>()
                , hostRepository ?? Substitute.For<HostRepository>()
                );
        }

        #endregion


    }
}
