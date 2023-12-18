using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Routing.AutoValues;
using NUnit.Framework;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Tests.Builders.Models;
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
            int jobId = Faker.RandomNumber.Next(1000);
            ISessionWrapperFactory sessionWrapperFactory = Substitute.For<ISessionWrapperFactory>();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapperFactory.CreateSession(Arg.Any<HostEntity>()).Returns(sessionWrapper);
            IDbContext dbContext = Substitute.For<IDbContext>();

            IFileTransferService fileTransferService = Substitute.For<IFileTransferService>();
            string[] availableFiles = { "C:\\Temp\\1.zip" };
            fileTransferService.UploadFilesAvailable(Arg.Any<string>()).Returns(availableFiles);

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithId(jobId).WithType(JobType.Upload).Build();
            JobRepository jobRepo = Substitute.For<JobRepository>();
            jobRepo.GetByIdAsync(dbContext, jobId).Returns(Task.FromResult(jobEntity));


            ITransferCommand transferCommand = CreateTransferCommand(sessionWrapperFactory, fileTransferService, jobRepo);
            transferCommand.Execute(dbContext, jobId);

            sessionWrapper.Received(2).ProgressCallback = Arg.Any<Action<int>>();
            Assert.That(sessionWrapper.ProgressCallback, Is.Null);
        }

        [Test]
        public void Execute_FailedTransfer_ProgressCallBackSetAndCleared()
        {
            int jobId = Faker.RandomNumber.Next(1000);
            ISessionWrapperFactory sessionWrapperFactory = Substitute.For<ISessionWrapperFactory>();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapperFactory.CreateSession(Arg.Any<HostEntity>()).Returns(sessionWrapper);
            IDbContext dbContext = Substitute.For<IDbContext>();

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithId(jobId).WithType(JobType.Upload).Build();
            JobRepository jobRepo = Substitute.For<JobRepository>();
            jobRepo.GetByIdAsync(dbContext, Arg.Any<int>()).Returns(Task.FromResult(jobEntity));

            IFileTransferService fileTransferService = Substitute.For<IFileTransferService>();
            string[] availableFiles = { "C:\\Temp\\1.zip" };
            fileTransferService.UploadFilesAvailable(Arg.Any<string>()).Returns(availableFiles);

            fileTransferService.When(x => x.UploadFiles(Arg.Any<ISessionWrapper>(), Arg.Any<IDbContext>(), Arg.Any<UploadOptions>())).Throw<Exception>();

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
			fileTransferService.Received(1).UploadFiles(sessionWrapper, dbContext, Arg.Any<UploadOptions>());
            sessionWrapper.Received(2).ProgressCallback = Arg.Any<Action<int>>();
            Assert.That(sessionWrapper.ProgressCallback, Is.Null);
        }

        [Test]
        public void Execute_DownloadJob_DownloadsCorrectly()
        {
            int jobId = Faker.RandomNumber.Next(10, 100);
            ISessionWrapperFactory sessionWrapperFactory = Substitute.For<ISessionWrapperFactory>();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapperFactory.CreateSession(Arg.Any<HostEntity>()).Returns(sessionWrapper);
            IDbContext dbContext = Substitute.For<IDbContext>();

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithType(JobType.Download).Build();
            JobRepository jobRepo = Substitute.For<JobRepository>();
            jobRepo.GetByIdAsync(dbContext, jobId).Returns(Task.FromResult(jobEntity));

            IFileTransferService fileTransferService = Substitute.For<IFileTransferService>();

            // execute
            ITransferCommand transferCommand = CreateTransferCommand(sessionWrapperFactory, fileTransferService, jobRepository: jobRepo);
            transferCommand.Execute(dbContext, jobId);

            // assert
            fileTransferService.Received(1).DownloadFiles(sessionWrapper, dbContext, Arg.Any<DownloadOptions>());
        }

        [Test]
        public void Execute_UploadJob_UploadsCorrectly()
        {
            int jobId = Faker.RandomNumber.Next(10, 100);    
            ISessionWrapperFactory sessionWrapperFactory = Substitute.For<ISessionWrapperFactory>();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapperFactory.CreateSession(Arg.Any<HostEntity>()).Returns(sessionWrapper);
            IDbContext dbContext = Substitute.For<IDbContext>();

            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithType(JobType.Upload).Build();
            JobRepository jobRepo = Substitute.For<JobRepository>();
            jobRepo.GetByIdAsync(dbContext, jobId).Returns(Task.FromResult(jobEntity));

            IFileTransferService fileTransferService = Substitute.For<IFileTransferService>();
            string[] availableFiles =  { "C:\\Temp\\1.zip" };
            fileTransferService.UploadFilesAvailable(Arg.Any<string>()).Returns(availableFiles);

            UploadOptions? optionsReceived = null;
            fileTransferService.When(x => x.UploadFiles(sessionWrapper, dbContext, Arg.Any<UploadOptions>())).Do(ci =>
            {
				optionsReceived = ci.ArgAt<UploadOptions>(2);
            });

            // execute
            ITransferCommand transferCommand = CreateTransferCommand(sessionWrapperFactory, fileTransferService, jobRepository: jobRepo);
            transferCommand.Execute(dbContext, jobId);

            // assert
            fileTransferService.Received(1).UploadFiles(sessionWrapper, dbContext, Arg.Any<UploadOptions>());
            Assert.That(optionsReceived, Is.Not.Null);
			Assert.That(optionsReceived.JobId, Is.EqualTo(jobId));
			Assert.That(optionsReceived.RemotePath, Is.EqualTo(jobEntity.RemotePath));
            Assert.That(optionsReceived.RestartOnFailure, Is.EqualTo(jobEntity.RestartOnFailure));
        }

        [Test]
        public void Execute_ExecuteJob_ClosesSession()
        {
            ISessionWrapperFactory sessionWrapperFactory = Substitute.For<ISessionWrapperFactory>();
            ISessionWrapper sessionWrapper = Substitute.For<ISessionWrapper>();
            sessionWrapperFactory.CreateSession(Arg.Any<HostEntity>()).Returns(sessionWrapper);
            IDbContext dbContext = Substitute.For<IDbContext>();

            IFileTransferService fileTransferService = Substitute.For<IFileTransferService>();
            fileTransferService.UploadFilesAvailable(Arg.Any<string>()).Returns(new string[] { "C:\\Temp\\1.zip" });

            int jobId = Faker.RandomNumber.Next();
            JobEntity jobEntity = new JobEntityBuilder().WithRandomProperties().WithId(jobId).WithType(JobType.Upload).Build();
            JobRepository jobRepo = Substitute.For<JobRepository>();
            jobRepo.GetByIdAsync(dbContext, jobEntity.Id).Returns(Task.FromResult(jobEntity));


            ITransferCommand transferCommand = CreateTransferCommand(sessionWrapperFactory, fileTransferService, jobRepo);
            transferCommand.Execute(dbContext, jobEntity.Id);

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
