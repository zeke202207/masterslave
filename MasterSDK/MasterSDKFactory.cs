namespace NetX.MasterSDK;

/// <summary>
/// sdk client工厂
/// </summary>
public class MasterSDKFactory
{
    private readonly string _host;

    public MasterSDKFactory(string host)
    {
        _host = host;
    }

    /// <summary>
    /// 创建客户端
    /// </summary>
    /// <returns></returns>
    public ServiceClient CreateClient()
    {
        return new ServiceClient(_host);
    }

    /// <summary>
    /// 创建Monitory客户端
    /// </summary>
    /// <returns></returns>
    public MonitorClient MasterMonitorClient()
    {
        return new MonitorClient(_host);
    }
}
