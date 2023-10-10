using Hangfire;

namespace NetX.Master.BackgroundTask
{
    public class HangFireHostService : IHostedService
    {
        private readonly IEnumerable<IJob> _jobs;
        private readonly ILogger _logger;

        public HangFireHostService(IEnumerable<IJob> jobs, ILogger<HangFireHostService> logger)
        {
            _jobs = jobs;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var job in _jobs)
                {
                    RecurringJob.AddOrUpdate($"{job.Id}", () => job.RunJob(), job.Cron);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("注册job失败", ex);
            }
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            try
            {
                foreach (var job in _jobs)
                {
                    RecurringJob.RemoveIfExists($"{job.Id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("取消job失败", ex);
            }
            await Task.CompletedTask;
        }
    }
}
