namespace NetX.Master;

/// <summary>
///  工作节点job实体对象
/// </summary>
public class WorkerJob
{
    /// <summary>
    /// 节点唯一标识
    /// </summary>
    public string WorkerId { get; set; }

    /// <summary>
    /// job实体<see cref="JobItem"/>
    /// </summary>
    public JobItem JobItem { get; set; }
}
