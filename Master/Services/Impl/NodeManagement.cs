using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NetX.Master
{
    public class NodeManagement : INodeManagement
    {
        private ILoadBalancing loadBalancing;
        private ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();
        private ConcurrentDictionary<string, WorkerNode> nodes = new ConcurrentDictionary<string, WorkerNode>();

        public NodeManagement(ILoadBalancing loadBalancing)
        {
            this.loadBalancing = loadBalancing ?? throw new ArgumentNullException(nameof(loadBalancing));
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
}
