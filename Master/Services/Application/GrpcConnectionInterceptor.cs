using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.AspNetCore.Http;

namespace NetX.Master;

public class GrpcConnectionInterceptor : Interceptor
{
    private readonly ILogger _logger;

    public GrpcConnectionInterceptor(ILogger<GrpcConnectionInterceptor> logger)
    {
        _logger = logger;
    }

    public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        try
        {            
            _logger.LogInformation($"------->Before Call -> {context.Method}");
            var response = continuation(request, context); 
            _logger.LogInformation($"------->After Call -> {context.Method}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("服务器处理异常", ex);
            return null;
        }
    }
}