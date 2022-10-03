﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SftpScheduler.BLL.Commands.Host;
using SftpScheduler.BLL.Data;
using SftpScheduler.BLL.Exceptions;
using SftpScheduler.BLL.Models;
using SftpSchedulerService.Models;

namespace SftpSchedulerService.ViewProviders.Hosts
{
    public class PostHostCreateProvider
    {
        private readonly IDbContextFactory _dbContextFactory;
        private readonly IMapper _mapper;
        private readonly CreateHostCommand _createHostCommand;

        public PostHostCreateProvider(IDbContextFactory dbContextFactory, IMapper mapper, CreateHostCommand createHostCommand)
        {
            _dbContextFactory = dbContextFactory;
            _mapper = mapper;
            _createHostCommand = createHostCommand;
        }

        public async Task<IActionResult> Execute(HostViewModel hostViewModel)
        {
            HostEntity hostEntity = _mapper.Map<HostEntity>(hostViewModel);
            using (IDbContext dbContext = _dbContextFactory.GetDbContext())
            {
                try
                {
                    hostEntity = _createHostCommand.Execute(dbContext, hostEntity);
                }
                catch (DataValidationException dve)
                {
                    return new BadRequestObjectResult(dve.ValidationResult);
                }
                hostViewModel.Id = hostEntity.Id;
            }
            var result = _mapper.Map<HostViewModel>(hostEntity);
            return new OkObjectResult(result);
        }
    }
}
