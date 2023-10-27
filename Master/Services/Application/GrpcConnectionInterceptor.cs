﻿using Grpc.Core;
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
            var response = continuation(request, context);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError("未捕获中间件异常", ex);
            return null;
        }
    }
}