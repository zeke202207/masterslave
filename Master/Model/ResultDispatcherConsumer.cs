using Grpc.Core;
using MasterSDKService;
using NetX.MemoryQueue;

namespace NetX.Master;

/// <summary>
/// 结果分发器消费者
/// </summary>
public class ResultDispatcherConsumer
{
    private readonly CancellationTokenSource TokenSource;

    /// <summary>
    /// 当前时间 - 创建时间 > 超时时间，即为超时
    /// 系统自动清理改消费者
    /// </summary>
    /// <param name="timeout">超时时间：秒</param>
    public ResultDispatcherConsumer(int timeout)
    {
        CreatTime = DateTime.Now;
        Timeout = timeout;
        TokenSource = new CancellationTokenSource();
        TokenSource.CancelAfter(TimeSpan.FromSeconds(Timeout));
        CancellationToken = TokenSource.Token;
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

    public bool IsComplete { get; private set; } = false;

    public IServerStreamWriter<ExecuteTaskResponse> StreamWriter { get; set; }

    public void Complete()
    {
        IsComplete = true;
        if (TokenSource.IsCancellationRequested)
            TokenSource.Cancel();
    }

    public bool IsTimeout()
    {
        return DateTime.Now - this.CreatTime > TimeSpan.FromSeconds(Timeout);
    }
}
