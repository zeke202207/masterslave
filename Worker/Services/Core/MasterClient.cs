using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using MasterWorkerService;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetX.Common;
using NetX.WorkerPlugin.Contract;
using System.Buffers;
using System.Collections.Concurrent;

namespace NetX.Worker;

public class MasterClient : IMasterClient, IDisposable
{
    private readonly ILogger<MasterClient> _logger;
    private readonly ConcurrentQueue<JobItem> _queue;
    private CancellationTokenSource _cancellationTokenSource;
    private GrpcChannel _channel;
    private MasterWorkerService.MasterNodeService.MasterNodeServiceClient _client;
    private WorkerConfig _config;
    private WorkerItem _node;
    private readonly RetryPolicy _retryPolicy;
    private IJobRunner _jobRunner;
    private readonly BlockingCollection<JobItemResult> _blockingCollection = new BlockingCollection<JobItemResult>();

    public MasterClient(IOptions<WorkerConfig> config, IJobRunner jobRunner, ILogger<MasterClient> logger)
    {
        _logger = logger;
        _queue = new ConcurrentQueue<JobItem>();
        _cancellationTokenSource = new CancellationTokenSource();
        _config = config.Value;
        _jobRunner = jobRunner;
        InitializeClient();
        _retryPolicy = new RetryPolicy(maxRetryCount: 5, initialRetryInterval: TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// 启动客户端链接
    /// </summary>
    /// <returns></returns>
    public async Task Start()
    {
        await Task.Factory.StartNew(async () => await StartHeartbeatAsync(), TaskCreationOptions.LongRunning);
        await Task.Factory.StartNew(async () => await StartReportStatusAsync(), TaskCreationOptions.LongRunning);
    }

    /// <summary>
    /// 节点注册
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public async Task<bool> RegisterNodeAsync(WorkerItem node)
    {
        bool result = false;
        _node = node;
        var request = new RegisterNodeRequest
        {
            Node = new Node() { Id = node.Id, IsBusy = node.IsBusy, LastUsed = node.LastActiveTime.DateTimeToUnixTimestamp() }
        };
        try
        {
            await _retryPolicy.ExecuteAsync(
                async () =>
                {
                    var response = await _client.RegisterNodeAsync(request, cancellationToken: _cancellationTokenSource.Token);
                    if (response.IsSuccess)
                    {
                        await Task.Factory.StartNew(async () => await ListenForJobsAsync(_node.Id), TaskCreationOptions.LongRunning);
                        var _ = Task.Factory.StartNew(async () => await ListenJobsResultsAsync(), TaskCreationOptions.LongRunning);
                    }
                    else
                    {
                        _cancellationTokenSource.Cancel();
                        _logger.LogError(response.ErrorMessage);
                    }
                    result = response.IsSuccess;
                },
                ex => ex is RpcException rpcEx && rpcEx.StatusCode != StatusCode.Unavailable);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register node");
            result = false;
        }
        return result;
    }

    /// <summary>
    /// 节点取消注册
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public async Task<bool> UnregisterNodeAsync(WorkerItem node)
    {
        var request = new UnregisterNodeRequest
        {
            Node = new Node() { Id = node.Id }
        };
        try
        {
            await _retryPolicy.ExecuteAsync(
                async () =>
                {
                    var response = await _client.UnregisterNodeAsync(request, cancellationToken: _cancellationTokenSource.Token);
                },
                ex => ex is RpcException rpcEx && rpcEx.StatusCode != StatusCode.Unavailable);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to unregister node");
            return false;
        }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    private void InitializeClient()
    {
        _channel = GrpcChannel.ForAddress(_config.GrpcServer, new GrpcChannelOptions()
        {
            HttpHandler = new SocketsHttpHandler()
            {
                //https://learn.microsoft.com/zh-cn/aspnet/core/grpc/performance?view=aspnetcore-7.0#keep-alive-pings
                PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                KeepAlivePingDelay = TimeSpan.FromSeconds(60),
                KeepAlivePingTimeout = TimeSpan.FromSeconds(30),
                //https://learn.microsoft.com/zh-cn/aspnet/core/grpc/performance?view=aspnetcore-7.0#connection-concurrency
                EnableMultipleHttp2Connections = true,
            },
            MaxReceiveMessageSize = int.MaxValue,
            MaxSendMessageSize = int.MaxValue,
        });
        _client = new MasterWorkerService.MasterNodeService.MasterNodeServiceClient(_channel);
    }

    /// <summary>
    /// 实时任务监听
    /// </summary>
    /// <param name="nodeId"></param>
    /// <returns></returns>
    private async Task ListenForJobsAsync(string nodeId)
    {
        var call = _client.ListenForJob(new ListenForJobRequest() { Id = nodeId });
        try
        {
            await foreach (var job in call.ResponseStream.ReadAllAsync(_cancellationTokenSource.Token))
            {
                var result = await _jobRunner.RunJobAsync(new JobItem() { JobId = job.JobId, Data = job.Data.ToByteArray() });
                _blockingCollection.TryAdd(result);
            }
        }
        catch (RpcException ex)
        {
            _logger.LogError(ex, "Failed to listen for jobs");
            if (ex.StatusCode == StatusCode.Unavailable)
            {
                InitializeClient();
                await RegisterNodeAsync(_node);
            }
        }
    }

    /// <summary>
    /// 监听任务结果，将结果推送给master
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task ListenJobsResultsAsync()
    {
        foreach (var item in _blockingCollection.GetConsumingEnumerable())
        {
            try
            {
                // 经权衡，每次创建一个连接通道传输数据，免去数据解码过程，同时能支持大文件分段上传
                var call = _client.ListenForResult();
                await item.Result.SegmentHandlerAsync(async segment =>
                {
                    await call.RequestStream.WriteAsync(new ListenForResultRequest()
                    {
                        Id = item.JobId,
                        Result = ByteString.CopyFrom(segment.Span)
                    });
                });
                await call.RequestStream.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("监听结果失败", ex);
            }
        }
    }

    /// <summary>
    /// 心跳线程
    /// </summary>
    /// <returns></returns>
    private async Task StartHeartbeatAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                var result = await _client.HeartbeatAsync(new HeartbeatRequest() { Id = _config.Id, CurrentTime = DateTime.UtcNow.DateTimeToUnixTimestamp() }, cancellationToken: _cancellationTokenSource.Token);
                if(!result.IsSuccess)
                {
                    InitializeClient();
                    await RegisterNodeAsync(_node);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Heartbeat failed");
                InitializeClient();
                await RegisterNodeAsync(_node);
            }
            await Task.Delay(TimeSpan.FromSeconds(5), _cancellationTokenSource.Token);
        }
    }

    /// <summary>
    /// 汇报节点信息线程
    /// </summary>
    /// <returns></returns>
    private async Task StartReportStatusAsync()
    {
        while (!_cancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                var request = new WorkerInfoRequest
                {
                    Id = _config.Id,
                    CurrentTime = DateTime.UtcNow.DateTimeToUnixTimestamp()
                };
                await _client.WorkerInfoAsync(request, cancellationToken: _cancellationTokenSource.Token);
            }
            catch (RpcException ex)
            {
                _logger.LogError(ex, "Failed to report status");
                if (ex.StatusCode == StatusCode.Unavailable)
                {
                    InitializeClient();
                    await RegisterNodeAsync(_node);
                }
            }
            await Task.Delay(TimeSpan.FromSeconds(5), _cancellationTokenSource.Token);
        }
    }

    /// <summary>
    /// 资源释放
    /// </summary>
    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _channel.ShutdownAsync().Wait(TimeSpan.FromSeconds(60));
        _channel?.Dispose();
        _client = null;
        _channel = null;
    }
}
