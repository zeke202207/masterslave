using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master
{
    public class WorkerNode
    {
        public string Id { get; set; }
        public WorkNodeStatus Status { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public DateTime LastUsed { get; set; }
        public WorkerNodeInfo Info { get; set; } = new WorkerNodeInfo();
    }
}
