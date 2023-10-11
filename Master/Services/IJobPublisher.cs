
namespace NetX.Master;

/// <summary>
/// 任务发布者
/// </summary>
public interface IJobPublisher
{
    /// <summary>
    /// 注册任务监听
    /// </summary>
    /// <param name="observer"></param>
    void Subscribe(IObserver<WorkerJob> observer);

    /// <summary>
    /// 取消任务监听注册
    /// </summary>
    /// <param name="observer"></param>
    void Unsubscribe(IObserver<WorkerJob> observer);

    /// <summary>
    /// 发布任务
    /// </summary>
    /// <param name="job"></param>
    void Publish(WorkerJob job);
}
