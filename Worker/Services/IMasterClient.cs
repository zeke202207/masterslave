namespace NetX.Worker;

/// <summary>
/// master通信客户端
/// </summary>
public interface IMasterClient
{
    /// <summary>
    /// 节点注册
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    Task<bool> RegisterNodeAsync(WorkerItem node);

    /// <summary>
    /// 取消节点注册
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    Task<bool> UnregisterNodeAsync(WorkerItem node);

    /// <summary>
    /// 启动节点
    /// </summary>
    /// <returns></returns>
    Task Start();
}
