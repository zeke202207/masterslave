using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NetX.Master
{
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

        public WorkerNode GetNode(IEnumerable<WorkerNode> nodes)
        {
            try
            {
                // Exclude nodes that are currently being used
                var availableNodes = nodes.Where(node => node.Status != WorkNodeStatus.Busy);
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
}
