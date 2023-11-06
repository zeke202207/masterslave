using Microsoft.Extensions.Configuration;

namespace NetX.Master;

public class AuthMiddleware<TRequest, TReponse> : IApplicationMiddleware<GrpcContext<TRequest, TReponse>>
{
    private readonly ISecurityPolicy _securityPolicy;

    public AuthMiddleware(ISecurityPolicy securityPolicy)
    {
        _securityPolicy = securityPolicy;
    }

    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<TRequest, TReponse>> next, GrpcContext<TRequest, TReponse> context)
    {
        var canAccess = _securityPolicy.IsRequestAllowed(context.Client);
        if(!canAccess) 
            throw new UnauthorizedAccessException("未授权");
        await next?.Invoke(context);
    }
}
