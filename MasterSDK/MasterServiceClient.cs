using MasterSDKService;

namespace NetX.MasterSDK;

public class MasterServiceClient : IDisposable
{
    private readonly string _host;
    private GrpcChannel _channel;
    private MasterSDKService.MasterServiceSDK.MasterServiceSDKClient _client;
    public Action<Exception> Logger;

    public MasterServiceClient(string host)
    {
        _host = host;
        InitializeClient();
    }

    private void InitializeClient()
    {
        _channel = GrpcChannel.ForAddress(_host);
        _client = new MasterSDKService.MasterServiceSDK.MasterServiceSDKClient(_channel);
    }

    public async Task<byte[]> ExecuteTaskAsync(ExecuteTaskRequest request)
    {
        try
        {
            await foreach (var s in _client.ExecuteTask(request).ResponseStream.ReadAllAsync())
            {
                return s.Result.ToByteArray();
            }
            return null;
        }
        catch (RpcException ex)
        {
            Logger?.Invoke(new Exception($"gRPC error: {ex.Status.Detail}", ex));
            return default(byte[]);
        }
        catch (Exception ex)
        {
            Logger?.Invoke(new Exception($"Error: {ex.Message}", ex));
            return default(byte[]);
        }
    }

    public void Dispose()
    {
        _channel.ShutdownAsync().Wait(TimeSpan.FromSeconds(60));
        _client = null;
        _channel = null;
    }
}