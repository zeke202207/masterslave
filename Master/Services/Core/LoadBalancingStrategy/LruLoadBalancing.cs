namespace NetX.Master;

/// <summary>
/// 最近最少使用策略
/// </summary>
public class LruLoadBalancing : ILoadBalancing
{
    private readonly ILogger _logger;

    public LruLoadBalancing(ILogger<LruLoadBalancing> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 获取可用的worker节点
    /// </summary>
    /// <param name="nodes">工作节点</param>
    /// <param name="metaData">请求元数据</param>
    /// <returns></returns>
    public WorkerNode GetNode(IEnumerable<WorkerNode> nodes, Dictionary<string, string> metaData)
    {
        try
        {
            // Exclude nodes that are currently being used
            var availableNodes = nodes.Where(node => node.Status == WorkNodeStatus.Idle);
            // Select the node that was least recently used
            return availableNodes.OrderBy(node => node.LastUsed).FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError("获取可用工作节点失败", ex);
            return null;
        }
    }
}
