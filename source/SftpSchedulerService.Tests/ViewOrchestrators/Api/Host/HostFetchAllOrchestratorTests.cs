﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpScheduler.BLL.Repositories;
using SftpScheduler.BLL.Validators;
using SftpSchedulerService.Models;
using SftpSchedulerService.ViewOrchestrators.Api.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SftpSchedulerService.Tests.ViewOrchestrators.Api.Host
{
    [TestFixture]
    public class HostFetchAllOrchestratorTests
    {
        [Test]
        public void Execute_OnSave_ReturnsOk()
        {
            IDbContextFactory dbContextFactory = Substitute.For<IDbContextFactory>();
            IMapper mapper = Substitute.For<IMapper>();
            HostRepository hostRepo = Substitute.For<HostRepository>();

            HostViewModel[] hostViewModels = { ViewModelTestHelper.CreateHostViewModel() };
            HostEntity[] hostEntities = { EntityTestHelper.CreateHostEntity() };

            hostRepo.GetAllAsync(Arg.Any<IDbContext>()).Returns(hostEntities);
            mapper.Map<HostEntity[], HostViewModel[]>(Arg.Any<HostEntity[]>()).Returns(hostViewModels);


            HostFetchAllOrchestrator hostFetchAllProvider = new HostFetchAllOrchestrator(dbContextFactory, mapper, hostRepo);
            var result = hostFetchAllProvider.Execute().Result as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.Not.Null);

            IEnumerable<HostViewModel> hostViewModelResult = (IEnumerable<HostViewModel>)result.Value;

            Assert.IsNotNull(hostViewModelResult);
            Assert.That(hostViewModelResult.Count, Is.EqualTo(1));
        }

    }
}