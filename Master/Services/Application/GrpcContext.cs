using Microsoft.AspNetCore.Http.Features;

namespace NetX.Master;

public sealed class GrpcContext<TRequest,TResponse> : ApplicationContext
{
    /// <summary>
    /// 获取redis客户端
    /// </summary>
    public GrpcClient Client { get; }

    /// <summary>
    /// 获取redis请求
    /// </summary>
    public GrpcRequest<TRequest> Reqeust { get; }

    /// <summary>
    /// 获取redis响应
    /// </summary>
    public GrpcResponse<TResponse> Response { get; }

    /// <summary>
    /// 取消令牌
    /// </summary>
    public CancellationToken CancellationToken{get;set;}

    /// <summary>
    /// Grpc上下文
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request"></param>
    /// <param name="response"></param>
    /// <param name="features"></param> 
    public GrpcContext(GrpcClient client, GrpcRequest<TRequest> request, GrpcResponse<TResponse> response, IFeatureCollection features)
        : base(features)
    {
        this.Client = client;
        this.Reqeust = request;
        this.Response = response;
    }

    public override string ToString()
    {
        return $"{this.Client} {this.Reqeust}";
    }
}
