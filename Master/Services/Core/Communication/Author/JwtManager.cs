using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NetX.Master;

public class JwtManager : IJwtManager
{
    private readonly ILogger _logger;
    private readonly IConfiguration _configuration;

    public JwtManager(IConfiguration configuration, ILogger<JwtManager> logger)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="jwtModel"></param>
    /// <returns></returns>
    public string GenerateJwtToken(JwtModel jwtModel)
    {
        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, jwtModel.UserId),
                new Claim(JwtRegisteredClaimNames.UniqueName, jwtModel.Password)
            };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Get<string>()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(_configuration.GetSection("Jwt:Issuer").Get<string>(),
            _configuration.GetSection("Jwt:Audience").Get<string>(),
            claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// 验证token有效性
    /// </summary>
    /// <param name="jwtToken"></param>
    /// <returns></returns>
    public JwtModel ValidateToken(string jwtToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Get<string>());

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration.GetSection("Jwt:Issuer").Get<string>(),
            ValidAudience = _configuration.GetSection("Jwt:Audience").Get<string>(),
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // 如果你想要更精确的过期时间验证，你可以设置ClockSkew为TimeSpan.Zero
        };

        SecurityToken validatedToken;
        try
        {
            tokenHandler.ValidateToken(jwtToken, validationParameters, out validatedToken);
        }
        catch
        {
            return null;
        }

        var jwtSecurityToken = validatedToken as JwtSecurityToken;

        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            return null; 
        return new JwtModel
            {
                UserId = jwtSecurityToken.Subject,
                Password = jwtSecurityToken.Claims.First(x => x.Type == "unique_name").Value
            };
    }
}
