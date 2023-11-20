using Grpc.Core;

namespace NetX.Master;

/// <summary>
/// 表示Grpc请求
/// </summary>
public sealed class GrpcRequest<T>
{
    /// <summary>
    /// 获取数据包大小
    /// </summary>
    public int Size { get; private set; }

    public T Request { get; private set; }

    public IAsyncStreamReader<T> RequestStream { get; set; }

    /// <summary>
    /// Grpc命令
    /// </summary> 
    private GrpcRequest()
    {
    }

    public static GrpcRequest<T> Create(T request)
    {
        var grpcRequest = new GrpcRequest<T>();
        grpcRequest.Request = request;
        return grpcRequest;
    }
}
