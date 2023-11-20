namespace NetX.Master;

/// <summary>
/// 异常捕获中间件
/// 顶层注册，统一异常处理
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TReponse"></typeparam>
public class ExceptionMiddleware<TRequest, TReponse> : IApplicationMiddleware<GrpcContext<TRequest, TReponse>>
{
    private readonly ILogger _logger;

    public ExceptionMiddleware(ILogger<ExceptionMiddleware<TRequest, TReponse>> logger, ISecurityPolicy securityPolicy)
    {
        _logger = logger;
    }


    public async Task InvokeAsync(ApplicationDelegate<GrpcContext<TRequest, TReponse>> next, GrpcContext<TRequest, TReponse> context)
    {
        try
        {
            await next?.Invoke(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"中间间处理异常");
        }
    }
}
