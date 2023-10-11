using Grpc.Core;

namespace NetX.Master;

public sealed class GrpcResponse<T>
{
    public Metadata MetaData { get; set; }

    public T Response { get; set; }

    public IServerStreamWriter<T> ResponseStream { get; set; }

    public GrpcResponse(T response)
    {
        MetaData = new Metadata();
        Response = response;
    }
}
