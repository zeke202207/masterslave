using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using MasterWorkerService;
using Microsoft.Extensions.Options;
using NetX.Common;
using NetX.WorkerPlugin.Contract;
using System.Collections.Concurrent;

namespace NetX.Worker;

/// <summary>
/// 与grpc服务主机通信类
/// </summary>
public class MasterClient : IMasterClient, IDisposable
{
    private WorkerItem _node;
    private GrpcChannel _channel;
    private WorkerNode _config;
    private readonly RetryPolicy _retryPolicy;
    private readonly ILogger<MasterClient> _logger;
    private readonly IServiceProvider _serviceProvider;
    private CancellationTokenSource _cancellationTokenSource;
    private MasterWorkerService.MasterNodeService.MasterNodeServiceClient _client;
    private readonly BlockingCollection<JobItemResult> _blockingCollection = new BlockingCollection<JobItemResult>();
    private NetX.Common.CPUTime _lastCpuTime;

    /// <summary>
    /// 通信类实例
    /// </summary>
    /// <param name="config"></param>
    /// <param name="jobRunner"></param>
    /// <param name="logger"></param>
    public MasterClient(IOptions<WorkerNode> config, ILogger<MasterClient> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _cancellationTokenSource = new CancellationTokenSource();
        _config = config.Value;
        InitializeClient();
        _retryPolicy = new RetryPolicy(maxRetryCount: 5, initialRetryInterval: TimeSpan.FromSeconds(1));
        _serviceProvider = serviceProvider;
        _lastCpuTime = CPUHelper.GetCPUTime();
    }

    /// <summary>
    /// 启动客户端链接
    /// </summary>
    /// <returns></returns>
    public Task Start()
    {
        var _ = Task.Run(() => StartHeartbeatAsync());
        var __ = Task.Run(() => StartReportStatusAsync());
        return Task.CompletedTask;
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
            Node = new Node()
            {
                Id = node.Id,
                Name = node.Name,
                MetaData = { node.MetaData },
                IsBusy = node.IsBusy,
                LastUsed = node.LastActiveTime.DateTimeToUnixTimestamp()
            }
        };
        try
        {
            await _retryPolicy.ExecuteAsync(
                async () =>
                {
                    var response = await _client.RegisterNodeAsync(request, cancellationToken: _cancellationTokenSource.Token);
                    if (response.IsSuccess)
                    {
                        var _ = Task.Run(() => ListenForJobsAsync(_node.Id));
                        var __ = Task.Run(() => ListenJobsResultsAsync());
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
    /// 初始化grpc客户端
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
                using (var jobRunner = _serviceProvider.GetService<IJobRunner>())
                {
                    var result = await jobRunner.RunJobAsync(new JobItem() { JobId = job.JobId, Data = job.Data.ToByteArray() });
                    _blockingCollection.TryAdd(result);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "任务监听失败");
            await ConnectToServer(_node);
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
                        Result = ByteString.CopyFrom(segment.Span),
                        WorkerId = _node.Id
                    });
                });
                await call.RequestStream.CompleteAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "监听结果失败");
            }
        }
    }

    /// <summary>
    /// 连接到grpc服务器
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private async Task ConnectToServer(WorkerItem node)
    {
        InitializeClient();
        await RegisterNodeAsync(node);
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
                var result = await _client.HeartbeatAsync(new HeartbeatRequest()
                {
                    Id = _config.Id,
                    CurrentTime = DateTime.UtcNow.DateTimeToUnixTimestamp()
                }, cancellationToken: _cancellationTokenSource.Token).ConfigureAwait(false);
                if (!result.IsSuccess)
                    await ConnectToServer(_node);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Heartbeat failed");
                await ConnectToServer(_node);
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
                    CurrentTime = DateTime.UtcNow.DateTimeToUnixTimestamp(),
                    CpuInfo = GetCpuInfo(),
                    MemoryInfo = GetMemoryInfo(),
                    PlatformInfo = GetPlatformInfo(),
                };
                request.DiskInfo.AddRange(GetDiskInfo());
                await _client.WorkerInfoAsync(request, cancellationToken: _cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to report status");
            }
            await Task.Delay(TimeSpan.FromSeconds(5), _cancellationTokenSource.Token);
        }
    }

    /// <summary>
    /// 获取平台信息
    /// </summary>
    /// <returns></returns>
    private MasterWorkerService.PlatformInfo GetPlatformInfo()
    {
        return new PlatformInfo()
        {
            FrameworkDescription = SystemPlatformInfo.FrameworkDescription,
            FrameworkVersion = SystemPlatformInfo.FrameworkVersion,
            IsUserInteractive = SystemPlatformInfo.IsUserInteractive,
            MachineName = SystemPlatformInfo.MachineName,
            OSArchitecture = SystemPlatformInfo.OSArchitecture,
            OSDescription = SystemPlatformInfo.OSDescription,
            OSPlatformID = SystemPlatformInfo.OSPlatformID,
            OSVersion = SystemPlatformInfo.OSVersion,
            ProcessArchitecture = SystemPlatformInfo.ProcessArchitecture,
            ProcessorCount = SystemPlatformInfo.ProcessorCount,
            UserDomainName = SystemPlatformInfo.UserDomainName,
            UserName = SystemPlatformInfo.UserName,
        };
    }

    /// <summary>
    /// 获取cpu信息
    /// </summary>
    /// <returns></returns>
    private MasterWorkerService.CpuInfo GetCpuInfo()
    {
        var cpuload = CPUHelper.CalculateCPULoad(_lastCpuTime, _lastCpuTime);
        _lastCpuTime = CPUHelper.GetCPUTime();
        return new CpuInfo()
        {
            CpuLoad = cpuload
        };
    }

    /// <summary>
    /// 获取内存信息
    /// </summary>
    /// <returns></returns>
    private MasterWorkerService.MemoryInfo GetMemoryInfo()
    {
        var memory = MemoryHelper.GetMemoryValue();
        return new MemoryInfo()
        {
            AvailablePhysicalMemory = memory.AvailablePhysicalMemory,
            AvailableVirtualMemory = memory.AvailableVirtualMemory,
            TotalPhysicalMemory = memory.TotalPhysicalMemory,
            TotalVirtualMemory = memory.TotalVirtualMemory,
            UsedPhysicalMemory = memory.UsedPhysicalMemory,
            UsedVirtualMemory = memory.UsedVirtualMemory,
        };
    }

    /// <summary>
    /// 获取硬盘信息
    /// </summary>
    /// <returns></returns>
    private List<MasterWorkerService.DiskInfo> GetDiskInfo()
    {
        var disks = NetX.Common.DiskInfo.GetDisks();
        return disks.Select(p => new MasterWorkerService.DiskInfo()
        {
            DriveType = p.DriveType.ToString(),
            FileSystem = p.FileSystem,
            FreeSpace = p.FreeSpace,
            Id = p.Id,
            Name = p.Name,
            TotalSize = p.TotalSize,
            UsedSize = p.UsedSize
        }).ToList();
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
