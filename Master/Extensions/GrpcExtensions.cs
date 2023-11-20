using Grpc.Core;

namespace NetX.Master;

public static class GrpcExtensions
{
    /// <summary>
    /// Grpc Context扩展方法
    /// 构建自定义管道上下文
    /// </summary>
    /// <typeparam name="TRequest"></typeparam>
    /// <typeparam name="TResponse"></typeparam>
    /// <param name="context"></param>
    /// <param name="request"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    public static GrpcContext<TRequest, TResponse> CreateGrpcContext<TRequest, TResponse>(this ServerCallContext context, TRequest request, TResponse response)
    {
        var client = new GrpcClient(context);
        var grpcRequest = GrpcRequest<TRequest>.Create(request);
        var grpcResponse = new GrpcResponse<TResponse>(response);
        var grpcContext = new GrpcContext<TRequest, TResponse>(client, grpcRequest, grpcResponse, context.GetHttpContext().Features)
        {
            CancellationToken = context.CancellationToken
        };
        return grpcContext;
    }
}
