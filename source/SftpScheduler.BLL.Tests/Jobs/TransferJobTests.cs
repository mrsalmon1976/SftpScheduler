using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Quartz;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Commands.Transfer;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpScheduler.BLL.Tests.Jobs
{
    [TestFixture]
    public class TransferJobTests
    {
        [Test]
        public void GetJobKeyName_ReturnsFormattedJobName()
        {
            int jobId = Faker.RandomNumber.Next(1, 10000);

            string jobName = TransferJob.GetJobKeyName(jobId);

            Assert.That(jobName, Is.EqualTo($"Job.{jobId}"));
        }

        [Test]
        public void GetJobIdFromKeyName_ReturnsJobId()
        {
            int jobId = Faker.RandomNumber.Next(1, 10000);
            string jobName = $"Job.{jobId}";

            int result = TransferJob.GetJobIdFromKeyName(jobName);

            Assert.That(result, Is.EqualTo(jobId));
        }

        [Test]
        public void Execute_CreatesNewJobResult()
        {
            int jobId = Faker.RandomNumber.Next(1, 10000);
            ICreateJobResultCommand createJobResultCommand = Substitute.For<ICreateJobResultCommand>();
            TransferJob transferJob = CreateTransferJob(createJobResultCommand: createJobResultCommand);
            IJobExecutionContext jobExecutionContext = CreateJobExecutionContext(jobId);

            JobResultEntity jobResultEntity = EntityTestHelper.CreateJobResultEntity();
            jobResultEntity.JobId = jobId;
            createJobResultCommand.ExecuteAsync(Arg.Any<IDbContext>(), jobId).Returns(Task.FromResult(jobResultEntity));

            transferJob.Execute(jobExecutionContext).GetAwaiter().GetResult();

            createJobResultCommand.Received(1).ExecuteAsync(Arg.Any<IDbContext>(), jobId);


        }

        [Test]
        public void Execute_ExecutesTransfer()
        {
            int jobId = Faker.RandomNumber.Next(1, 10000);
            ICreateJobResultCommand createJobResultCommand = Substitute.For<ICreateJobResultCommand>();

            ITransferCommandFactory transferCommandFactory = Substitute.For<ITransferCommandFactory>();
            ITransferCommand transferCommand = Substitute.For<ITransferCommand>();
            transferCommandFactory.CreateTransferCommand().Returns(transferCommand);

            TransferJob transferJob = CreateTransferJob(createJobResultCommand: createJobResultCommand, transferCommandFactory: transferCommandFactory);
            IJobExecutionContext jobExecutionContext = CreateJobExecutionContext(jobId);


            JobResultEntity jobResultEntity = EntityTestHelper.CreateJobResultEntity();
            jobResultEntity.JobId = jobId;
            createJobResultCommand.ExecuteAsync(Arg.Any<IDbContext>(), jobId).Returns(Task.FromResult(jobResultEntity));

            transferJob.Execute(jobExecutionContext).GetAwaiter().GetResult();

            transferCommand.Received(1).Execute(jobId);


        }

        [Test]
        public void Execute_OnSuccessfulTransfer_UpdatesJobResultWithSuccess()
        {
            int jobId = Faker.RandomNumber.Next(1, 10000);
            ICreateJobResultCommand createJobResultCommand = Substitute.For<ICreateJobResultCommand>();
            IUpdateJobResultCompleteCommand updateJobResultCompleteCommand = Substitute.For<IUpdateJobResultCompleteCommand>();

            ITransferCommandFactory transferCommandFactory = Substitute.For<ITransferCommandFactory>();
            ITransferCommand transferCommand = Substitute.For<ITransferCommand>();
            transferCommandFactory.CreateTransferCommand().Returns(transferCommand);
            Exception thrownException = new Exception("Transfer failed!");
            transferCommand.When(x => x.Execute(Arg.Any<int>())).Throw(thrownException);

            TransferJob transferJob = CreateTransferJob(createJobResultCommand: createJobResultCommand, transferCommandFactory: transferCommandFactory, updateJobResultCompleteCommand: updateJobResultCompleteCommand);
            IJobExecutionContext jobExecutionContext = CreateJobExecutionContext(jobId);

            JobResultEntity jobResultEntity = EntityTestHelper.CreateJobResultEntity();
            jobResultEntity.Id = Faker.RandomNumber.Next(1, 1000);
            jobResultEntity.JobId = jobId;
            createJobResultCommand.ExecuteAsync(Arg.Any<IDbContext>(), jobId).Returns(Task.FromResult(jobResultEntity));

            transferJob.Execute(jobExecutionContext).GetAwaiter().GetResult();

            updateJobResultCompleteCommand.Received(1).ExecuteAsync(Arg.Any<IDbContext>(), jobResultEntity.Id, 100, JobResult.Failure, thrownException.Message);


        }

        [Test]
        public void Execute_OnFailedTransfer_UpdatesJobResultWithFailure()
        {
            int jobId = Faker.RandomNumber.Next(1, 10000);
            ICreateJobResultCommand createJobResultCommand = Substitute.For<ICreateJobResultCommand>();
            IUpdateJobResultCompleteCommand updateJobResultCompleteCommand = Substitute.For<IUpdateJobResultCompleteCommand>();

            TransferJob transferJob = CreateTransferJob(createJobResultCommand: createJobResultCommand, updateJobResultCompleteCommand: updateJobResultCompleteCommand);
            IJobExecutionContext jobExecutionContext = CreateJobExecutionContext(jobId);


            JobResultEntity jobResultEntity = EntityTestHelper.CreateJobResultEntity();
            jobResultEntity.Id = Faker.RandomNumber.Next(1, 1000);
            jobResultEntity.JobId = jobId;
            createJobResultCommand.ExecuteAsync(Arg.Any<IDbContext>(), jobId).Returns(Task.FromResult(jobResultEntity));

            transferJob.Execute(jobExecutionContext).GetAwaiter().GetResult();

            updateJobResultCompleteCommand.Received(1).ExecuteAsync(Arg.Any<IDbContext>(), jobResultEntity.Id, 100, JobResult.Success, null);


        }


        private TransferJob CreateTransferJob(ILogger<TransferJob> logger = null, IDbContextFactory dbContextFactory = null, ITransferCommandFactory transferCommandFactory = null, ICreateJobResultCommand createJobResultCommand = null, IUpdateJobResultCompleteCommand updateJobResultCompleteCommand = null)
        {
            return new TransferJob(logger ?? Substitute.For<ILogger<TransferJob>>()
                , dbContextFactory ?? Substitute.For<IDbContextFactory>()
                , transferCommandFactory ?? Substitute.For<ITransferCommandFactory>()
                , createJobResultCommand ?? Substitute.For<ICreateJobResultCommand>()
                , updateJobResultCompleteCommand ?? Substitute.For<IUpdateJobResultCompleteCommand>()
                );
        }

        private IJobExecutionContext CreateJobExecutionContext(int jobId)
        {
            IJobExecutionContext jobExecutionContext = Substitute.For<IJobExecutionContext>();
            IJobDetail jobDetail = Substitute.For<IJobDetail>();
            jobExecutionContext.JobDetail.Returns(jobDetail);

            JobKey jobKey = new JobKey($"Job.{jobId}");
            jobDetail.Key.Returns(jobKey);

            return jobExecutionContext;
        }



    }
}
