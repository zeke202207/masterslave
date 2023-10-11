namespace NetX.Master;

/// <summary>
/// 白名单策略
/// </summary>
public class IpWhitelistSecurityPolicy : ISecurityPolicy
{
    private readonly HashSet<string> _ipWhitelist;
    private readonly ILogger _logger;

    public IpWhitelistSecurityPolicy(IConfiguration configuration, ILogger<IpWhitelistSecurityPolicy> logger)
    {
        _logger = logger;
        _ipWhitelist = new HashSet<string>(configuration.GetSection("Master:IpWhitelist").Get<string[]>());
    }

    public bool IsRequestAllowed(GrpcClient client)
    {
        try
        {
            return _ipWhitelist.Contains(client.Host);
        }
        catch (Exception ex)
        {
            _logger.LogError("授权验证失败", ex);
            return false;
        }
    }
}
