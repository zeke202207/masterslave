namespace NetX.Master;

public interface INodeManagement
{
    /// <summary>
    /// worker节点注册
    /// </summary>
    /// <param name="node"></param>
    void NodeRegister(WorkerNode node);

    /// <summary>
    /// worker节点取消注册
    /// </summary>
    /// <param name="nodeId"></param>
    void NodeUnRegister(string nodeId);

    /// <summary>
    /// 获取可用节点
    /// </summary>
    /// <returns></returns>
    WorkerNode GetAvailableNode();

    /// <summary>
    /// 获取指定worker节点
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    WorkerNode GetNode(string nodeId);

    /// <summary>
    /// 获取全部worker节点
    /// </summary>
    /// <returns></returns>
    List<WorkerNode> GetAllNodes();

    /// <summary>
    /// 更新指定worker节点属性
    /// </summary>
    /// <param name="nodeId"></param>
    /// <param name="nodeFunc"></param>
    void UpdateNode(string nodeId, Func<WorkerNode> nodeFunc);
}
