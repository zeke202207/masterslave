using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NetX.Master
{
    public interface INodeManagement
    {
        void AddNode(WorkerNode node);
        void RemoveNode(string nodeId);
        WorkerNode GetAvailableNode();
        WorkerNode GetNode(string nodeId);
        List<WorkerNode> GetAllNodes();
        void UpdateNode(string nodeId, Func<WorkerNode> nodeFunc);
    }
}
