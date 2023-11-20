namespace NetX.Master;

/// <summary>
/// 任务执行器
/// 将任务发配给指定工作节点
/// </summary>
public class JobExecutor : IJobExecutor
{
    private readonly IJobPublisher _publisher;
    private readonly INodeManagement _nodeManager;
    private readonly ILogger _logger;

    public JobExecutor(IJobPublisher jobPublisher, INodeManagement nodeManagement, ILogger<JobExecutor> logger)
    {
        _publisher = jobPublisher;
        _logger = logger;
        _nodeManager = nodeManagement;
    }

    /// <summary>
    /// 将任务发送给工作节点执行
    /// </summary>
    /// <param name="workerNodeId">工作节点唯一标识</param>
    /// <param name="job">任务详细信息</param>
    /// <returns></returns>
    public async Task ExecuteJobAsync(string workerNodeId, JobItem job)
    {
        try
        {
            var node = await _nodeManager.GetNode(workerNodeId);
            if (null == node)
                throw new NodeNotFoundException();
            node.Status = WorkNodeStatus.Busy;
            await _nodeManager.UpdateNode(workerNodeId, () => node);
            _publisher.Publish(new WorkerJob() { WorkerId = node.Id, JobItem = job });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "执行任务失败");
        }
    }
}
