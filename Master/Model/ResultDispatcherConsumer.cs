using Grpc.Core;
using MasterSDKService;

namespace NetX.Master;

/// <summary>
/// 结果分发器消费者
/// </summary>
public class ResultDispatcherConsumer
{
    /// <summary>
    /// 当前时间 - 创建时间 > 超时时间，即为超时
    /// 系统自动清理改消费者
    /// </summary>
    /// <param name="timeout">超时时间：秒</param>
    public ResultDispatcherConsumer(int timeout)
    {
        CreatTime = DateTime.Now;
        Timeout = timeout;
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

    public CancellationTokenSource TokenSource { get; set; }

    public IServerStreamWriter<ExecuteTaskResponse> StreamWriter { get; set; }

    public bool IsTimeout()
    {
        return DateTime.Now - this.CreatTime > TimeSpan.FromSeconds(Timeout);
    }
}
