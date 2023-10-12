namespace NetX.MasterSDK;

/// <summary>
/// sdk client工厂
/// </summary>
public class MasterServiceSDKFactory
{
    private readonly string _host;

    public MasterServiceSDKFactory(string host)
    {
        _host = host;
    }

    /// <summary>
    /// 创建客户端
    /// </summary>
    /// <returns></returns>
    public MasterServiceClient CreateClient()
    {
        return new MasterServiceClient(_host);
    }
}
