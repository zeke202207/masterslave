using Hangfire;

namespace NetX.Master;

/// <summary>
/// 无效节点定期清理job
/// </summary>
// 设置超时时间，以防止任务长时间占用资源
[DisableConcurrentExecution(timeoutInSeconds: 5)] 
public class CleanupWorkerNode : IJob
{
    public string Cron => "0/1 * * * * ?";
    public string Id => new Guid("00000000000000000000000000000002").ToString();

    private readonly ILogger _logger;
    private readonly INodeManagement _nodeManagement;

    public CleanupWorkerNode(INodeManagement nodeManagement, ILogger<CleanupResultConsumer> logger)
    {
        _logger = logger;
        _nodeManagement = nodeManagement;
    }

    public Task<bool> RunJob()
    {
        try
        {
            _logger.LogError($"{nameof(CleanupWorkerNode)}");
            foreach (var node in _nodeManagement.GetAllNodes())
            {
                if (node.IsTimeout())
                    node.Status = WorkNodeStatus.Offline;
                else
                {
                    if (node.Status == WorkNodeStatus.Offline)
                        node.Status = WorkNodeStatus.Idle;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"{nameof(CleanupWorkerNode)}任务执行失败", ex);
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }
}
