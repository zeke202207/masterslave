namespace NetX.Master;

/// <summary>
/// worker节点负载选择
/// </summary>
public interface ILoadBalancing
{
    /// <summary>
    /// 获取可用的worker节点
    /// </summary>
    /// <param name="nodes"></param>
    /// <returns></returns>
    WorkerNode GetNode(IEnumerable<WorkerNode> nodes);
}
