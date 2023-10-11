namespace NetX.Master;

/// <summary>
/// job执行器
/// </summary>
public interface IJobExecutor
{
    /// <summary>
    /// 执行job任务
    /// </summary>
    /// <param name="workerNodeId">工作节点唯一标识</param>
    /// <param name="job">任务详情</param>
    /// <returns></returns>
    Task ExecuteJobAsync(string workerNodeId, JobItem job);
}
