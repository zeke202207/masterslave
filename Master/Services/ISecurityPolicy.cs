namespace NetX.Master;

/// <summary>
/// 安全测率
/// </summary>
public interface ISecurityPolicy
{
    /// <summary>
    /// 判断请求是否允许
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    bool IsRequestAllowed(AppContext context);
}
