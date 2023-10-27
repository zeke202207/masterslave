namespace NetX.Master;

/// <summary>
/// worker节点负载选择
/// </summary>
public interface ILoadBalancing
{
    /// <summary>
    /// 获取可用的worker节点
    /// </summary>
    /// <param name="nodes">所有可用节点</param>
    /// <param name="metaData">客户端查询元数据，可用于未来扩展指定任务类型来筛选节点服务器</param>
    /// <returns></returns>
    WorkerNode GetNode(IEnumerable<WorkerNode> nodes, Dictionary<string, string> metaData);
}
