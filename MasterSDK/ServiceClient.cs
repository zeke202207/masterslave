using SDK;

namespace NetX.MasterSDK;

/// <summary>
/// master sdk 客户端
/// </summary>
public class ServiceClient : BaseClient<SDK.MasterServiceSDK.MasterServiceSDKClient>, IDisposable
{
    public ServiceClient(string host,string username, string password)
        : base(host, username, password)
    {

    }

    protected override MasterServiceSDK.MasterServiceSDKClient CreateClient(GrpcChannel channel)
    {
        return new SDK.MasterServiceSDK.MasterServiceSDKClient(channel);
    }

    protected override string Login(string userName, string password)
    {
        try
        {
            var result = _client.Login(new SDK.ServiceLoginRequest() { UserName = userName, Password = password });
            if (!result.IsSuccess)
                throw new Exception($"登录失败：{result.ErrorMessage}");
            return result.Token;
        }
        catch (Exception ex)
        {
            Logger?.Invoke(ex);
            return default(string);
        }
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
            var call = _client.ExecuteTask(request, base.GetMetadata());
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