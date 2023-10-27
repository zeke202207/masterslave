namespace NetX.WorkerPlugin.Contract;

/// <summary>
/// job执行接口定义
/// </summary>
public interface IJobRunner : IDisposable
{
    Task<JobItemResult> RunJobAsync(JobItem job);
}
