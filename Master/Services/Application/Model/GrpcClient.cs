using Grpc.Core;

namespace NetX.Master;

public sealed class GrpcClient
{
    private readonly ServerCallContext context;

    public bool? IsAuthed { get; set; }

    /// <summary>Name of method called in this RPC.</summary>
    public string Method => context.Method;

    /// <summary>Name of host called in this RPC.</summary>
    public string Host => context.Host;

    /// <summary>Address of the remote endpoint in URI format.</summary>
    public string Peer => context.Peer;

    /// <summary>Initial metadata sent by client.</summary>
    public Metadata RequestHeaders => context.RequestHeaders;

    /// <summary>
    /// Grpc客户端
    /// </summary>
    /// <param name="context"></param> 
    public GrpcClient(ServerCallContext context)
    {
        this.context = context;
    }
}
