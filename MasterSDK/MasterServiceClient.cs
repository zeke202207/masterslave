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
        _channel = GrpcChannel.ForAddress(_host, new GrpcChannelOptions() 
        {
              MaxSendMessageSize = int.MaxValue, 
            MaxReceiveMessageSize = int.MaxValue,
        });
        _client = new MasterSDKService.MasterServiceSDK.MasterServiceSDKClient(_channel);
    }

    public async Task<byte[]> ExecuteTaskAsync(ExecuteTaskRequest request)
    {
        try
        {
            List<byte> result = new List<byte>();
            var call = _client.ExecuteTask(request);
            var responseStream = call.ResponseStream;
            while (await responseStream.MoveNext())
            {
                var response = responseStream.Current;
                if (response.Result == null || response.Result.Length == 0)
                    break;
                result.AddRange(response.Result.ToByteArray());
            }
            return result.ToArray();
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