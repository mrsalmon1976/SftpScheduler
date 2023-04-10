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
        public void Execute_CreatesNewJobLog()
        {
            int jobId = Faker.RandomNumber.Next(1, 10000);
            ICreateJobLogCommand createJobLogCmd = Substitute.For<ICreateJobLogCommand>();
            TransferJob transferJob = CreateTransferJob(createJobLogCommand: createJobLogCmd);
            IJobExecutionContext jobExecutionContext = CreateJobExecutionContext(jobId);

            JobLogEntity jobLog = EntityTestHelper.CreateJobLogEntity();
            jobLog.JobId = jobId;
            createJobLogCmd.ExecuteAsync(Arg.Any<IDbContext>(), jobId).Returns(Task.FromResult(jobLog));

            transferJob.Execute(jobExecutionContext).GetAwaiter().GetResult();

            createJobLogCmd.Received(1).ExecuteAsync(Arg.Any<IDbContext>(), jobId);


        }

        [Test]
        public void Execute_ExecutesTransfer()
        {
            int jobId = Faker.RandomNumber.Next(1, 10000);
            ICreateJobLogCommand createJobLogCmd = Substitute.For<ICreateJobLogCommand>();

            ITransferCommand transferCommand = Substitute.For<ITransferCommand>();

            TransferJob transferJob = CreateTransferJob(createJobLogCommand: createJobLogCmd, transferCommand: transferCommand);
            IJobExecutionContext jobExecutionContext = CreateJobExecutionContext(jobId);


            JobLogEntity jobLog = EntityTestHelper.CreateJobLogEntity();
            jobLog.JobId = jobId;
            createJobLogCmd.ExecuteAsync(Arg.Any<IDbContext>(), jobId).Returns(Task.FromResult(jobLog));

            transferJob.Execute(jobExecutionContext).GetAwaiter().GetResult();

            transferCommand.Received(1).Execute(Arg.Any<IDbContext>(), jobId);


        }

        [Test]
        public void Execute_OnFailedTransfer_UpdatesJobLogWithFailure()
        {
            int jobId = Faker.RandomNumber.Next(1, 10000);
            ICreateJobLogCommand createJobLogCmd = Substitute.For<ICreateJobLogCommand>();
            IUpdateJobLogCompleteCommand updateJobLogCompleteCmd = Substitute.For<IUpdateJobLogCompleteCommand>();

            ITransferCommand transferCommand = Substitute.For<ITransferCommand>();
            Exception thrownException = new Exception("Transfer failed!");
            transferCommand.When(x => x.Execute(Arg.Any<IDbContext>(), Arg.Any<int>())).Throw(thrownException);


            TransferJob transferJob = CreateTransferJob(createJobLogCommand: createJobLogCmd, updateJobLogCompleteCommand: updateJobLogCompleteCmd, transferCommand: transferCommand);
            IJobExecutionContext jobExecutionContext = CreateJobExecutionContext(jobId);


            JobLogEntity jobLogEntity = EntityTestHelper.CreateJobLogEntity();
            jobLogEntity.Id = Faker.RandomNumber.Next(1, 1000);
            jobLogEntity.JobId = jobId;
            createJobLogCmd.ExecuteAsync(Arg.Any<IDbContext>(), jobId).Returns(Task.FromResult(jobLogEntity));

            transferJob.Execute(jobExecutionContext).GetAwaiter().GetResult();

            updateJobLogCompleteCmd.Received(1).ExecuteAsync(Arg.Any<IDbContext>(), jobLogEntity.Id, 100, JobStatus.Failure, thrownException.Message);


        }

        [Test]
        public void Execute_OnSuccessfulTransfer_UpdatesJobLogWithSuccess()
        {
            int jobId = Faker.RandomNumber.Next(1, 10000);
            ICreateJobLogCommand createJobLogCommand = Substitute.For<ICreateJobLogCommand>();
            IUpdateJobLogCompleteCommand updateJobLogCompleteCommand = Substitute.For<IUpdateJobLogCompleteCommand>();

            TransferJob transferJob = CreateTransferJob(createJobLogCommand: createJobLogCommand, updateJobLogCompleteCommand: updateJobLogCompleteCommand);
            IJobExecutionContext jobExecutionContext = CreateJobExecutionContext(jobId);


            JobLogEntity jobLogEntity = EntityTestHelper.CreateJobLogEntity();
            jobLogEntity.Id = Faker.RandomNumber.Next(1, 1000);
            jobLogEntity.JobId = jobId;
            createJobLogCommand.ExecuteAsync(Arg.Any<IDbContext>(), jobId).Returns(Task.FromResult(jobLogEntity));

            transferJob.Execute(jobExecutionContext).GetAwaiter().GetResult();

            updateJobLogCompleteCommand.Received(1).ExecuteAsync(Arg.Any<IDbContext>(), jobLogEntity.Id, 100, JobStatus.Success, null);


        }


        private TransferJob CreateTransferJob(ILogger<TransferJob>? logger = null, IDbContextFactory? dbContextFactory = null, ITransferCommand? transferCommand = null, ICreateJobLogCommand? createJobLogCommand = null, IUpdateJobLogCompleteCommand? updateJobLogCompleteCommand = null)
        {
            return new TransferJob(logger ?? Substitute.For<ILogger<TransferJob>>()
                , dbContextFactory ?? Substitute.For<IDbContextFactory>()
                , transferCommand ?? Substitute.For<ITransferCommand>()
                , createJobLogCommand ?? Substitute.For<ICreateJobLogCommand>()
                , updateJobLogCompleteCommand ?? Substitute.For<IUpdateJobLogCompleteCommand>()
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
