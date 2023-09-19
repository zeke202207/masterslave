using Grpc.Core;
using Grpc.Net.Client;
using MasterSDKService;
using System.Threading.Channels;

namespace NetX.MasterSDK
{
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

        public async Task<ExecuteTaskResponse> ExecuteTaskAsync(ExecuteTaskRequest request)
        {
            try
            {
                return await _client.ExecuteTaskAsync(request);
            }
            catch (RpcException ex)
            {
                Logger?.Invoke(new Exception($"gRPC error: {ex.Status.Detail}",ex));
                return default(ExecuteTaskResponse);
            }
            catch (Exception ex)
            {
                Logger?.Invoke(new Exception($"Error: {ex.Message}", ex));
                return default(ExecuteTaskResponse);
            }
        }

        public async Task<CancelTaskResponse> CancelTaskAsync(CancelTaskRequest request)
        {
            try
            {
                return await _client.CancelTaskAsync(request);
            }
            catch (RpcException ex)
            {
                Logger?.Invoke(new Exception($"gRPC error: {ex.Status.Detail}",ex));
                return default(CancelTaskResponse);
            }
            catch (Exception ex)
            {
                Logger?.Invoke(new Exception($"Error: {ex.Message}",ex));
                return default(CancelTaskResponse);
            }
        }

        public async Task<GetJobStatusResponse> GetJobStatusAsync(GetJobStatusRequest request)
        {
            try
            {
                return await _client.GetJobStatusAsync(request);
            }
            catch (RpcException ex)
            {
                Logger?.Invoke(new Exception($"gRPC error: {ex.Status.Detail}",ex));
                return default(GetJobStatusResponse);
            }
            catch (Exception ex)
            {
                Logger?.Invoke(new Exception($"Error: {ex.Message}",ex));
                return default(GetJobStatusResponse);
            }
        }

        public async Task<GetJobResultResponse> GetJobResultAsync(GetJobResultRequest request)
        {
            try
            {
                return await _client.GetJobResultAsync(request);
            }
            catch (RpcException ex)
            {
                Logger?.Invoke(new Exception($"gRPC error: {ex.Status.Detail}",ex));
                return default(GetJobResultResponse);
            }
            catch (Exception ex)
            {
                Logger?.Invoke(new Exception($"Error: {ex.Message}",ex));
                return default(GetJobResultResponse);
            }
        }

        public void Dispose()
        {
            _channel.ShutdownAsync().Wait(TimeSpan.FromSeconds(60));
            _client = null;
            _channel = null;
        }
    }
}