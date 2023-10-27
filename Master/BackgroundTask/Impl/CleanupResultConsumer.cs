using Hangfire;

namespace NetX.Master;

/// <summary>
/// 结果监听消费者定期清理job
/// </summary>
// 设置超时时间，以防止任务长时间占用资源
[DisableConcurrentExecution(timeoutInSeconds: 10)]
public class CleanupResultConsumer : IJob
{
    public string Cron => "0/5 * * * * ?";
    public string Id => new Guid("00000000000000000000000000000001").ToString();

    private readonly ILogger _logger;
    private readonly IResultDispatcher _dispatcher;

    public CleanupResultConsumer(IResultDispatcher resultDispatcher, ILogger<CleanupResultConsumer> logger)
    {
        _logger = logger;
        _dispatcher = resultDispatcher;
    }

    public Task<bool> RunJob()
    {
        try
        {
            foreach (var consumer in _dispatcher.GetConsumers())
            {
                if (consumer.IsTimeout() && !consumer.CancellationToken.IsCancellationRequested)
                    consumer.FailedCompleted(new TimeoutException($"{nameof(CleanupResultConsumer)}任务超时"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(CleanupResultConsumer)}任务执行失败", ex);
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }
}
