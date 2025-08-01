namespace SftpSchedulerService.Workers
{
    public class TempDataCleanupWorker : BackgroundService
    {
        private readonly ILogger<TempDataCleanupWorker> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public const int RetryTime = 60000;	// 1 minute

		public TempDataCleanupWorker(ILogger<TempDataCleanupWorker> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
			_serviceScopeFactory = serviceScopeFactory;
		}

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

			while (!stoppingToken.IsCancellationRequested)
            {
				_logger.LogInformation("TempDataCleanupWorker running cleanup...");
				using (var scope = _serviceScopeFactory.CreateScope())
				{
					ITempDataCleanupWorkerService cleanupWorkerService = scope.ServiceProvider.GetService<ITempDataCleanupWorkerService>()!;
					if (cleanupWorkerService.ExecuteCleanup())
					{
						_logger.LogInformation("TempDataCleanupWorker completing - no more files to clean up.");
						return;
					}
				}

				_logger.LogInformation("TempDataCleanupWorker sleeping for {retryTime} milliseconds", RetryTime);
				await Task.Delay(RetryTime, stoppingToken);
            }
        }
    }
}