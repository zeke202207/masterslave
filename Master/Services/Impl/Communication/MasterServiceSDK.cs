using Grpc.Core;
using MasterSDKService;
using NetX.Master.Model;
using NetX.MemoryQueue;

namespace NetX.Master;

/// <summary>
/// grpc服务master提供的SDK
/// </summary>
public class MasterServiceSDK : MasterSDKService.MasterServiceSDK.MasterServiceSDKBase
{
    private readonly IPublisher _publisher;
    private readonly ILogger _logger;
    private readonly IResultDispatcher _dataTransferCenter;

    /// <summary>
    /// SDK实例
    /// </summary>
    /// <param name="publisher"></param>
    /// <param name="logger"></param>
    /// <param name="dataTransferCenter"></param>
    public MasterServiceSDK(IPublisher publisher, ILogger<MasterServiceSDK> logger, IResultDispatcher dataTransferCenter)
    {
        _publisher = publisher;
        _logger = logger;
        _dataTransferCenter = dataTransferCenter;
    }

    /// <summary>
    /// 开始执行任务
    /// </summary>
    /// <param name="request"></param>
    /// <param name="responseStream"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public override async Task ExecuteTask(ExecuteTaskRequest request, IServerStreamWriter<ExecuteTaskResponse> responseStream, ServerCallContext context)
    {
        int timeout = request.Timeout <= 0 ? 60 : request.Timeout;
        //1. Create a job and add it to the queue
        var jobItem = new JobItem(Guid.NewGuid().ToString("N"), request.Data.ToByteArray());
        var consumer = new ResultDispatcherConsumer(timeout)
        {
            JobId = jobItem.jobId,
            TokenSource = new CancellationTokenSource(),
            StreamWriter = responseStream,
        };
        try
        {
            consumer.TokenSource.CancelAfter(TimeSpan.FromSeconds(timeout));
            _dataTransferCenter.ConsumerRegister(consumer);
            await _publisher.Publish<JobItemMessage>(MasterConst.C_QUEUENAME_JOBITEM, new JobItemMessage(jobItem));
            await Task.Delay(Timeout.Infinite, consumer.TokenSource.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError("执行任务失败", ex);
        }
        finally
        {
            _dataTransferCenter.ConsumerUnRegister(consumer);
        }
    }
}
