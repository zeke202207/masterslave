namespace NetX.Master;

/// <summary>
/// Hangfire服务执行的job接口契约
/// </summary>
public interface IJob
{
    /// <summary>
    /// 定制任务的唯一标识
    /// </summary>
    string Id { get; }

    /// <summary>
    /// 定时任务执行的cron触发器
    /// </summary>
    string Cron { get; }

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <returns></returns>
    public Task<bool> RunJob();
}
