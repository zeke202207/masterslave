namespace NetX.Master;

/// <summary>
/// job状态
/// </summary>
public enum JobStatus
{
    /// <summary>
    /// 待执行
    /// </summary>
    Pending,

    /// <summary>
    /// 执行中
    /// </summary>
    Running,

    /// <summary>
    /// 执行结束
    /// </summary>
    Completed,

    /// <summary>
    /// 执行失败
    /// </summary>
    Failed
}

/// <summary>
/// 节点状态
/// </summary>
public enum WorkNodeStatus
{
    /// <summary>
    /// 空闲
    /// </summary>
    Idle,

    /// <summary>
    /// 工作中
    /// </summary>
    Busy,

    /// <summary>
    /// 离线
    /// </summary>
    Offline
}
