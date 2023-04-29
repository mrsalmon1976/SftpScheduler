using NSubstitute;
using NUnit.Framework;
using Quartz;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Utility.IO;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using WinSCP;

namespace SftpScheduler.BLL.Tests.Commands.Job
{
    [TestFixture]
    public class ExecuteJobCommandTests
    {

        [Test]
        public void Execute_AdHocJobAlreadyExists_ThrowsInvalidOperationException()
        {
            // setup 
            int jobId = Faker.RandomNumber.Next();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            IJobDetail jobDetail = Substitute.For<IJobDetail>();
            scheduler.GetJobDetail(Arg.Any<JobKey>()).Returns(jobDetail);

            // execute
            IExecuteJobCommand executeJobCommand = new ExecuteJobCommand(schedulerFactory);
            try
            {
                executeJobCommand.ExecuteAsync(jobId).GetAwaiter().GetResult();
                Assert.Fail("Expected exception not thrown");
            }
            catch (InvalidOperationException)
            {
                // 
            }

            // assert
            schedulerFactory.Received(1).GetScheduler();
            scheduler.Received(1).GetJobDetail(Arg.Any<JobKey>());
        }
    

        [Test]
        public void Execute_AdHocJobDoesNotExist_SchedulesJobWithOnceOffTrigger()
        {
            // setup 
            int jobId = Faker.RandomNumber.Next();
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();
            IJobDetail? jobDetail = null;
            scheduler.GetJobDetail(Arg.Any<JobKey>()).Returns(jobDetail);
            schedulerFactory.GetScheduler().Returns(scheduler);

            IJobDetail? scheduledJobDetail = null;
            ITrigger? scheduledTrigger = null;
            scheduler.When(x => x.ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>())).Do((ci) =>
            {
                scheduledJobDetail = ci.ArgAt<IJobDetail>(0);
                scheduledTrigger = ci.ArgAt<ITrigger>(1);
            });

            // execute
            IExecuteJobCommand executeJobCommand = new ExecuteJobCommand(schedulerFactory);
            executeJobCommand.ExecuteAsync(jobId).GetAwaiter().GetResult();

            // assert
            Assert.That(scheduledJobDetail, Is.Not.Null);
            Assert.That(scheduledJobDetail.Durable, Is.False);
            Assert.That(scheduledJobDetail.Key.Name, Is.EqualTo($"AdHoc.Job.{jobId}"));

            Assert.That(scheduledTrigger, Is.Not.Null);
            Assert.That(scheduledTrigger.Key.Name, Is.EqualTo($"AdHoc.Trigger.{jobId}"));
            Assert.That(scheduledTrigger.FinalFireTimeUtc, Is.LessThanOrEqualTo(new DateTimeOffset(DateTime.UtcNow)));

        }



    }
}
