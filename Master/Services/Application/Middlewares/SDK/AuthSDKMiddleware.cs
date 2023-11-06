using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

public class AuthSDKMiddleware<TRequest, TReponse> : IApplicationMiddleware<GrpcContext<TRequest, TReponse>>
{
    private readonly IJwtManager _jwtManager;

    public AuthSDKMiddleware(IJwtManager jwtManager)
    {
        _jwtManager = jwtManager;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<TRequest, TReponse>> next, GrpcContext<TRequest, TReponse> context)
    {
        var jwtToken = context.Client.RequestHeaders.FirstOrDefault(p => p.Key.Equals("authorization"))?.Value;
        if(string.IsNullOrWhiteSpace(jwtToken) || !jwtToken.StartsWith("Bearer "))
            throw new UnauthorizedAccessException("未找到Jwt信息");
        //解析jwt
        var validateResult = _jwtManager.ValidateToken(jwtToken[7..]);
        if(null == validateResult)
            throw new UnauthorizedAccessException("未授权");
        await next?.Invoke(context);
    }
}
