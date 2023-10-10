using System.Collections.Concurrent;

namespace NetX.Master;

/// <summary>
/// worker节点管理
/// </summary>
public class NodeManagement : INodeManagement
{
    private readonly ILoadBalancing loadBalancing;
    private readonly ILogger logger;
    private ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
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
    public void NodeRegister(WorkerNode node)
    {
        if (node == null)
            throw new ArgumentNullException(nameof(node));
        _readerWriterLock.EnterWriteLock();
        try
        {
            nodes.AddOrUpdate(node.Id, node, (key, oldValue) => node);
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// worker节点取消注册
    /// </summary>
    /// <param name="nodeId"></param>
    public void NodeUnRegister(string nodeId)
    {
        _readerWriterLock.EnterWriteLock();
        try
        {
            nodes.TryRemove(nodeId, out _);
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 获取全部可用的worker节点
    /// </summary>
    /// <returns></returns>
    public WorkerNode GetAvailableNode()
    {
        _readerWriterLock.EnterReadLock();
        try
        {
            return loadBalancing.GetNode(nodes.Values);
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
    }

    /// <summary>
    /// 获取指定worker节点
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    public WorkerNode GetNode(string nodeId)
    {
        _readerWriterLock.EnterReadLock();
        try
        {
            return nodes.Values.FirstOrDefault(p => p.Id.Equals(nodeId, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
    }

    /// <summary>
    /// 获取全部节点
    /// </summary>
    /// <returns></returns>
    public List<WorkerNode> GetAllNodes()
    {
        _readerWriterLock.EnterReadLock();
        try
        {
            return new List<WorkerNode>(nodes.Values);
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
    }

    /// <summary>
    /// 更新指定worker节点属性
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="nodeFunc"></param>
    public void UpdateNode(string nodeId, Func<WorkerNode> nodeFunc)
    {
        var node = nodeFunc?.Invoke();
        _readerWriterLock.EnterReadLock();
        try
        {
            nodes.AddOrUpdate(nodeId, node, (key, oldValue) => node);
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
    }
}
