namespace NetX.Master;

/// <summary>
/// 工作节点实体
/// </summary>
public class WorkerNode
{
    /// <summary>
    /// 超时时间
    /// </summary>
    private int timeout { get; } = 10;

    /// <summary>
    /// 工作节点唯一标识
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 工作节点名称
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 工作节点元数据
    /// </summary>
    public Dictionary<string, string> MetaData { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// 工作节点状态
    /// </summary>
    public WorkNodeStatus Status { get; set; }

    /// <summary>
    /// 工作节点上次心跳时间
    /// </summary>
    public DateTime LastHeartbeat { get; set; }

    /// <summary>
    /// 工作节点上次使用时间
    /// </summary>
    public DateTime LastUsed { get; set; }

    /// <summary>
    /// 工作节点计算机信息
    /// </summary>
    public WorkerNodeInfo SystemInfo { get; set; } = new WorkerNodeInfo();

    /// <summary>
    /// 是否超时
    /// </summary>
    /// <returns></returns>
    public bool IsTimeout()
    {
        return DateTime.Now - this.LastHeartbeat > TimeSpan.FromSeconds(timeout);
    }
}
