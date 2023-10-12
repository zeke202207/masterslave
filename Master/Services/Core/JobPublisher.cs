using System.Collections.Concurrent;

namespace NetX.Master;

/// <summary>
/// 任务监听观察者发布管理
/// </summary>
public class JobPublisher : IJobPublisher
{
    /// <summary>
    /// 任务监听集合
    /// </summary>
    private readonly ConcurrentBag<IObserver<WorkerJob>> _observers = new ConcurrentBag<IObserver<WorkerJob>>();

    /// <summary>
    /// 读写锁
    /// </summary>
    private ReaderWriterLockSlim _readerWriterLock = new ReaderWriterLockSlim();

    /// <summary>
    /// 注册到观察者集合
    /// </summary>
    /// <param name="observer"></param>
    public void Subscribe(IObserver<WorkerJob> observer)
    {
        _readerWriterLock.EnterWriteLock();
        try
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }
        finally
        {
            _readerWriterLock.ExitWriteLock();
        }
    }

    /// <summary>
    /// 观察者集合取消注册
    /// </summary>
    /// <param name="observer"></param>
    public void Unsubscribe(IObserver<WorkerJob> observer)
    {
        _readerWriterLock.EnterReadLock();
        try
        {
            _observers.TryTake(out observer);
        }
        finally
        {
            _readerWriterLock.ExitReadLock();
        }
    }

    /// <summary>
    /// 发布消息
    /// </summary>
    /// <param name="job"></param>
    public void Publish(WorkerJob job)
    {
        foreach (var observer in _observers)
        {
            JobObserver jobObserver = observer as JobObserver;
            if (null == jobObserver || !jobObserver.WorkerId.Equals(job.WorkerId))
                continue;
            observer.OnNext(job);
        }
    }
}
