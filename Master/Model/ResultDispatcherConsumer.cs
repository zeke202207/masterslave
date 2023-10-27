using Grpc.Core;
using SDK;
using NetX.MemoryQueue;
using NetX.Master.Services.Core;

namespace NetX.Master;

/// <summary>
/// 结果分发器消费者
/// </summary>
public class ResultDispatcherConsumer
{
    private readonly CancellationTokenSource TokenSource;
    private readonly IResultDispatcher _resultDispatcher;
    private readonly IJobTrackerCache<JobTrackerItem> _jobTrackerCache;

    /// <summary>
    /// 当前时间 - 创建时间 > 超时时间，即为超时
    /// 系统自动清理改消费者
    /// </summary>
    /// <param name="timeout">超时时间：秒</param>
    public ResultDispatcherConsumer(int timeout, IResultDispatcher resultDispatcher, IJobTrackerCache<JobTrackerItem> jobTrackerCache)
    {
        CreatTime = DateTime.Now;
        Timeout = timeout;
        TokenSource = new CancellationTokenSource();
        TokenSource.Token.Register(async () =>
        {
            try
            {
                //2.更新结果集
                if (IsSuccess)
                {
                    await _jobTrackerCache.UpdateAsync(JobId, p =>
                    {
                        p.Status = TrackerStatus.Success;
                        p.EndTime = DateTime.Now;
                        return p;
                    });
                }
                else
                {
                    string message = "处理失败";
                    if (null != Exception)
                        message = Exception.ToString();
                    else if (IsTimeout())
                        message = "超时";
                    else
                        message = "未捕获异常，处理失败";
                    await _jobTrackerCache.UpdateAsync(JobId, p =>
                    {
                        p.Status = TrackerStatus.Failure;
                        p.Message = message;
                        p.EndTime = DateTime.Now;
                        return p;
                    });
                }
            }
            catch (Exception ex)
            {
                //do nothing
            }
            finally
            {
                //1.取消任务监听注册
                _resultDispatcher.ConsumerUnRegister(this);
            }
        });
        TokenSource.CancelAfter(TimeSpan.FromSeconds(Timeout));
        CancellationToken = TokenSource.Token;
        _resultDispatcher = resultDispatcher;
        _jobTrackerCache = jobTrackerCache;
    }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatTime { get; private set; }

    /// <summary>
    /// 超时时间(S)
    /// </summary>
    public int Timeout { get; private set; }

    /// <summary>
    /// job唯一标识
    /// </summary>
    public string JobId { get; set; }

    public CancellationToken CancellationToken { get; private set; }

    public bool IsSuccess { get; private set; } = false;

    public Exception Exception { get; private set; } = null;

    public IServerStreamWriter<ExecuteTaskResponse> StreamWriter { get; set; }

    /// <summary>
    /// 成功
    /// </summary>
    public void SuccessCompleted()
    {
        IsSuccess = true;
        TokenSource.Cancel(false);
    }

    /// <summary>
    /// 失败
    /// </summary>
    /// <param name="ex"></param>
    public void FailedCompleted(Exception ex)
    {
        IsSuccess = false;
        Exception = ex;
        TokenSource.Cancel(false);
    }

    /// <summary>
    /// 判断是否超时
    /// </summary>
    /// <returns></returns>
    public bool IsTimeout()
    {
        return DateTime.Now - this.CreatTime > TimeSpan.FromSeconds(Timeout);
    }
}
