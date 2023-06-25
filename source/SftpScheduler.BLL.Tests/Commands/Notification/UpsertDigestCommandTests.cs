using NSubstitute;
using NUnit.Framework;
using Quartz;
using SftpScheduler.BLL.Commands.Job;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.Common.IO;
using SftpScheduler.BLL.Jobs;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;
using SftpScheduler.BLL.Commands.Notification;
using SftpScheduler.BLL.Utility;

namespace SftpScheduler.BLL.Tests.Commands.Notification
{
    [TestFixture]
    public class UpsertDigestCommandTests
    {

        [Test]
        public void Execute_DigestJobExists_JobIsDeleted()
        {
            // setup
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            DayOfWeek[] days = { DayOfWeek.Monday };
            int hour = Faker.RandomNumber.Next(0, 23);
            scheduler.CheckExists(Arg.Any<JobKey>()).Returns(true);

            // execute
            IUpsertDigestCommand cmd = new UpsertDigestCommand(schedulerFactory, Substitute.For<CronBuilder>());
            cmd.Execute(days, hour);

            // assert
            scheduler.Received(1).CheckExists(Arg.Any<JobKey>());
            scheduler.Received(1).DeleteJob(Arg.Any<JobKey>()); 
        }

        [Test]
        public void Execute_DigestJobDoesNotExist_NoDeleteCall()
        {
            // setup
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            DayOfWeek[] days = { DayOfWeek.Monday };
            int hour = Faker.RandomNumber.Next(0, 23);
            scheduler.CheckExists(Arg.Any<JobKey>()).Returns(false);

            // execute
            IUpsertDigestCommand cmd = new UpsertDigestCommand(schedulerFactory, Substitute.For<CronBuilder>());
            cmd.Execute(days, hour);

            // assert
            scheduler.Received(1).CheckExists(Arg.Any<JobKey>());
            scheduler.DidNotReceive().DeleteJob(Arg.Any<JobKey>());
        }

        [Test]
        public void Execute_NoDaysOfWeekSupplied_JobIsNotCreated()
        {
            // setup
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            DayOfWeek[] days = Array.Empty<DayOfWeek>();
            int hour = Faker.RandomNumber.Next(0, 23);
            scheduler.CheckExists(Arg.Any<JobKey>()).Returns(false);

            // execute
            IUpsertDigestCommand cmd = new UpsertDigestCommand(schedulerFactory, Substitute.For<CronBuilder>());
            cmd.Execute(days, hour);

            // assert
            scheduler.DidNotReceive().ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>());
        }

        [Test]
        public void Execute_DaysOfWeekSupplied_JobIsCreated()
        {
            // setup
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            DayOfWeek[] daysOfWeek = { DayOfWeek.Monday };
            int hour = Faker.RandomNumber.Next(0, 23);

            CronBuilder cronBuilder = Substitute.For<CronBuilder>();
            cronBuilder.Daily(daysOfWeek, hour).Returns("0 0 17 ? * MON,TUE,WED *");

            IJobDetail? jobDetail = null;

            scheduler.When(x => x.ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>())).Do((cb) =>
            {
                jobDetail = cb.ArgAt<IJobDetail>(0);
            });

            // execute
            IUpsertDigestCommand cmd = new UpsertDigestCommand(schedulerFactory, cronBuilder);
            cmd.Execute(daysOfWeek, hour);

            // assert
            scheduler.Received(1).ScheduleJob(jobDetail!, Arg.Any<ITrigger>());
            Assert.That(jobDetail!.Key.Name, Is.EqualTo(DigestJob.JobKeyName()));
        }

        [Test]
        public void Execute_DaysOfWeekSupplied_JobIsCreatedWithValidTrigger()
        {
            // setup
            ISchedulerFactory schedulerFactory = Substitute.For<ISchedulerFactory>();
            IScheduler scheduler = Substitute.For<IScheduler>();
            schedulerFactory.GetScheduler().Returns(scheduler);

            DayOfWeek[] daysOfWeek = { DayOfWeek.Monday };
            int hour = Faker.RandomNumber.Next(0, 23);

            CronBuilder cronBuilder = Substitute.For<CronBuilder>();
            cronBuilder.Daily(daysOfWeek, hour).Returns("0 0 17 ? * MON,TUE,WED *");

            ITrigger? trigger = null;

            scheduler.When(x => x.ScheduleJob(Arg.Any<IJobDetail>(), Arg.Any<ITrigger>())).Do((cb) =>
            {
                trigger = cb.ArgAt<ITrigger>(1);
            });

            // execute
            IUpsertDigestCommand cmd = new UpsertDigestCommand(schedulerFactory, cronBuilder);
            cmd.Execute(daysOfWeek, hour);

            // assert
            scheduler.Received(1).ScheduleJob(Arg.Any<IJobDetail>(), trigger!);
            Assert.That(trigger!.Key.Name, Is.EqualTo(DigestJob.TriggerKeyName()));
        }


    }
}
