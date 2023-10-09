using System.Collections.Concurrent;

namespace NetX.Master;

public class NodeManagement : INodeManagement
{
    private ILoadBalancing loadBalancing;
    private ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
    private ConcurrentDictionary<string, WorkerNode> nodes = new ConcurrentDictionary<string, WorkerNode>();
    private readonly Timer timer;
    private readonly int _interval = 10 * 1000;

    public NodeManagement(ILoadBalancing loadBalancing)
    {
        this.loadBalancing = loadBalancing ?? throw new ArgumentNullException(nameof(loadBalancing));
        timer = new Timer(p =>
        {
            try
            {
                foreach (var node in nodes)
                {
                    if ((DateTime.Now - node.Value.LastHeartbeat).TotalMilliseconds > _interval)
                        node.Value.Status = WorkNodeStatus.Offline;
                    else
                    {
                        if (node.Value.Status == WorkNodeStatus.Offline)
                            node.Value.Status = WorkNodeStatus.Idle;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }, null, 0, 1 * 1000);
    }

    public void AddNode(WorkerNode node)
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

    public void RemoveNode(string nodeId)
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
