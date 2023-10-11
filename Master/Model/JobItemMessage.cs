using NetX.MemoryQueue;

namespace NetX.Master;

/// <summary>
/// job消息队列实体
/// </summary>
public class JobItemMessage : MessageArgument
{
    /// <summary>
    /// job实体对象<see cref="JobItem"/>
    /// </summary>
    public JobItem Job { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="jobItem"></param>
    public JobItemMessage(JobItem jobItem)
    {
        Job = jobItem;
    }
}
