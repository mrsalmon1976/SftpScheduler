﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.Test.Common;
using SftpSchedulerService.Models.Host;
using SftpSchedulerService.Utilities;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Host
{
    [TestFixture]
    public class HostFetchOneOrchestratorTests
    {
        [Test]
        public void Execute_FetchesEntityAndReturnsAsViewModel()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IDbContext dbContext = Substitute.For<IDbContext>();
            dbContextFactory.GetDbContext().Returns(dbContext);
            
            int hostId = Faker.RandomNumber.Next(10, 1000);
            string hashId = UrlUtils.Encode(hostId);

            HostRepository hostRepo = Substitute.For<HostRepository>();
            var hostEntity = new SubstituteBuilder<HostEntity>().WithRandomProperties().WithProperty(x => x.Id, hostId).Build();
            hostRepo.GetByIdAsync(dbContext, hostId).Returns(hostEntity);

            IMapper mapper = AutoMapperTestHelper.CreateMapper();

            // execute
            HostFetchOneOrchestrator hostFetchOneOrchestrator = new HostFetchOneOrchestrator(dbContextFactory, mapper, hostRepo);
            var result = hostFetchOneOrchestrator.Execute(hashId).Result as OkObjectResult;
            
            // assert
            Assert.That(result, Is.Not.Null);

            var resultModel = result.Value as HostViewModel;
            Assert.That(resultModel, Is.Not.Null);
            Assert.That(resultModel.HashId, Is.EqualTo(hashId));

        }

    }

}
