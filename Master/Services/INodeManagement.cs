namespace NetX.Master;

/// <summary>
/// 节点管理器
/// </summary>
public interface INodeManagement
{
    /// <summary>
    /// worker节点注册
    /// </summary>
    /// <param name="node"></param>
    Task NodeRegister(WorkerNode node);

    /// <summary>
    /// worker节点取消注册
    /// </summary>
    /// <param name="nodeId"></param>
    Task NodeUnRegister(string nodeId);

    /// <summary>
    /// 获取可用节点
    /// </summary>
    /// <returns></returns>
    Task<WorkerNode> GetAvailableNode(Dictionary<string, string> metaData);

    /// <summary>
    /// 获取指定worker节点
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    Task<WorkerNode> GetNode(string nodeId);

    /// <summary>
    /// 获取全部worker节点
    /// </summary>
    /// <returns></returns>
    Task<List<WorkerNode>> GetAllNodes();

    /// <summary>
    /// 更新指定worker节点属性
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="nodeFunc"></param>
    Task UpdateNode(string nodeId, Func<WorkerNode> nodeFunc);
}
