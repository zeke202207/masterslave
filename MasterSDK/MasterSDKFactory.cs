namespace NetX.MasterSDK;

/// <summary>
/// sdk client工厂
/// </summary>
public class MasterSDKFactory
{
    private readonly string _host;
    private readonly string _userName;
    private readonly string _password;

    /// <summary>
    ///  GRPC客户端工厂
    ///  基于JWT认证
    /// </summary>
    /// <param name="host">grpc地址</param>
    /// <param name="userName">用户名</param>
    /// <param name="password">密码</param>
    public MasterSDKFactory(string host, string userName, string password)
    {
        _host = host;
        _userName = userName;
        _password = password;
    }

    /// <summary>
    /// 创建客户端
    /// </summary>
    /// <returns></returns>
    public ServiceClient CreateClient()
    {
        return new ServiceClient(_host, _userName, _password);
    }

    /// <summary>
    /// 创建Monitory客户端
    /// </summary>
    /// <returns></returns>
    public MonitorClient MasterMonitorClient()
    {
        return new MonitorClient(_host, _userName, _password);
    }
}
