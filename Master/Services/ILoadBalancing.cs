namespace NetX.Master;

public interface ILoadBalancing
{
    WorkerNode GetNode(IEnumerable<WorkerNode> nodes);
}
