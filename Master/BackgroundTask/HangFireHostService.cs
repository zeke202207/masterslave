using Hangfire;

namespace NetX.Master;

/// <summary>
/// 定时任务服务主机
/// </summary>
public class HangFireHostService : IHostedService
{
    /// <summary>
    /// 所有任务集合
    /// </summary>
    private readonly IEnumerable<IJob> _jobs;
    private readonly ILogger _logger;

    /// <summary>
    /// 定时任务服务主机实例对象
    /// </summary>
    /// <param name="jobs"></param>
    /// <param name="logger"></param>
    public HangFireHostService(IEnumerable<IJob> jobs, ILogger<HangFireHostService> logger)
    {
        _jobs = jobs;
        _logger = logger;
    }

    /// <summary>
    /// 主机服务开启
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
            _logger.LogError(ex, "注册job失败");
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// 主机服务停止
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
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
            _logger.LogError(ex,"取消job失败");
        }
        await Task.CompletedTask;
    }
}
