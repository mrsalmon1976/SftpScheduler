using SftpScheduler.Common;
using SftpScheduler.Common.IO;
using SftpSchedulerService.Config;

namespace SftpSchedulerService.Workers
{
    public class UpdateCleanupWorker : BackgroundService
    {
        private readonly ILogger<UpdateCleanupWorker> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public const int RetryTime = 5000;

		public UpdateCleanupWorker(ILogger<UpdateCleanupWorker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
			_serviceScopeFactory = serviceScopeFactory;
		}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

			while (!stoppingToken.IsCancellationRequested)
            {
				_logger.LogInformation("UpdateCleanupWorker running cleanup...");
				using (var scope = _serviceScopeFactory.CreateScope())
				{
					IUpdateCleanupWorkerService cleanupWorkerService = scope.ServiceProvider.GetService<IUpdateCleanupWorkerService>()!;
					if (cleanupWorkerService.ExecuteCleanup())
					{
						_logger.LogInformation("UpdateCleanupWorker completing - no more files to clean up.");
						return;
					}
				}

				_logger.LogInformation("UpdateCleanupWorker sleeping for {retryTime} milliseconds", RetryTime);
				await Task.Delay(RetryTime, stoppingToken);
            }
        }
    }
}