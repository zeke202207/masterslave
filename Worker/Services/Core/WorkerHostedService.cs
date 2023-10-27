using Microsoft.Extensions.Options;

namespace NetX.Worker;

/// <summary>
/// 节点主机服务
/// </summary>
public class WorkerHostedService : IHostedService
{
    private readonly IMasterClient _masterClient;
    private readonly WorkerNode _config;
    private readonly ILogger _logger;

    /// <summary>
    /// 节点主机服务实例
    /// </summary>
    /// <param name="masterClient"></param>
    /// <param name="config"></param>
    /// <param name="logger"></param>
    public WorkerHostedService(IMasterClient masterClient, IOptions<WorkerNode> config, ILogger<WorkerHostedService> logger)
    {
        _masterClient = masterClient;
        _config = config.Value;
        _logger = logger;
    }

    /// <summary>
    /// 服务开启
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _masterClient.RegisterNodeAsync(new WorkerItem() 
            { 
                Id = _config.Id, 
                Name = _config.Name,
                MetaData = _config.MetaData,
                IsBusy = false,
                LastActiveTime = DateTime.UtcNow 
            });
            await _masterClient.Start();
        }
        catch (Exception ex)
        {
            _logger.LogError("注册节点失败", ex);
        }
    }

    /// <summary>
    /// 服务停止
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _masterClient.UnregisterNodeAsync(new WorkerItem() { Id = _config.Id });
    }
}
