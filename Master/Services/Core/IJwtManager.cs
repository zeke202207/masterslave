namespace NetX.Master;

public interface IJwtManager
{
    /// <summary>
    /// 生成Jwt票据
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    string GenerateJwtToken(JwtModel model);

    /// <summary>
    /// 解析jwt信息
    /// </summary>
    /// <param name="jwtToken"></param>
    /// <returns></returns>
    JwtModel ValidateToken(string jwtToken);
}
