using System.Collections.Concurrent;

namespace NetX.Master;

/// <summary>
/// worker节点管理
/// </summary>
public class NodeManagement : INodeManagement
{
    private readonly ILoadBalancing loadBalancing;
    private readonly ILogger logger;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private ConcurrentDictionary<string, WorkerNode> nodes = new ConcurrentDictionary<string, WorkerNode>();

    /// <summary>
    /// 节点管理实例对象
    /// </summary>
    /// <param name="loadBalancing"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public NodeManagement(ILoadBalancing loadBalancing, ILogger<NodeManagement> logger)
    {
        this.loadBalancing = loadBalancing ?? throw new ArgumentNullException(nameof(loadBalancing));
        this.logger = logger;
    }

    /// <summary>
    /// worker 节点注册
    /// </summary>
    /// <param name="node"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public async Task NodeRegister(WorkerNode node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));
        await _semaphore.WaitAsync();
        try
        {
            nodes.AddOrUpdate(node.Id, node, (key, oldValue) => node);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// worker节点取消注册
    /// </summary>
    /// <param name="nodeId"></param>
    public async Task NodeUnRegister(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            throw new ArgumentNullException(nameof(nodeId));
        await _semaphore.WaitAsync();
        try
        {
            nodes.TryRemove(nodeId, out _);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 获取全部可用的worker节点
    /// </summary>
    /// <returns></returns>
    public async Task<WorkerNode> GetAvailableNode(Dictionary<string, string> metaData)
    {
        await _semaphore.WaitAsync();
        try
        {
            return loadBalancing.GetNode(nodes.Values, metaData);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 获取指定worker节点
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    public async Task<WorkerNode> GetNode(string nodeId)
    {
        await _semaphore.WaitAsync();
        try
        {
            return nodes.Values.FirstOrDefault(p => p.Id.Equals(nodeId, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 获取全部节点
    /// </summary>
    /// <returns></returns>
    public async Task<List<WorkerNode>> GetAllNodes()
    {
        await _semaphore.WaitAsync();
        try
        {
            return new List<WorkerNode>(nodes.Values);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// 更新指定worker节点属性
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="nodeFunc"></param>
    public async Task UpdateNode(string nodeId, Func<WorkerNode> nodeFunc)
    {
        var node = nodeFunc?.Invoke();
        await _semaphore.WaitAsync();
        try
        {
            nodes.AddOrUpdate(nodeId, node, (key, oldValue) => node);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
