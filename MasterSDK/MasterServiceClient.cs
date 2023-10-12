using MasterSDKService;

namespace NetX.MasterSDK;

/// <summary>
/// master sdk 客户端
/// </summary>
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

    /// <summary>
    /// 执行任务
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
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
        catch (Exception ex)
        {
            Logger?.Invoke(new Exception($"Error: {ex.Message}", ex));
            return default(byte[]);
        }
    }

    /// <summary>
    /// 资源释放
    /// </summary>
    public void Dispose()
    {
        _channel.ShutdownAsync().Wait(TimeSpan.FromSeconds(60));
        _client = null;
        _channel = null;
    }
}