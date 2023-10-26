using Grpc.Core;
using SDK;
using NetX.MemoryQueue;
using Newtonsoft.Json.Linq;
using NetX.Master.Services.Core;

namespace NetX.Master;

/// <summary>
/// grpc服务master提供的SDK
/// </summary>
public class ServiceSDK : SDK.MasterServiceSDK.MasterServiceSDKBase
{
    private readonly IPublisher _publisher;
    private readonly ILogger _logger;
    private readonly IResultDispatcher _resultDispatcher;
    private readonly IJobTrackerCache<JobTrackerItem> _jobTrackerCache;

    /// <summary>
    /// SDK实例
    /// </summary>
    /// <param name="publisher"></param>
    /// <param name="logger"></param>
    /// <param name="dataTransferCenter"></param>
    public ServiceSDK(IPublisher publisher, ILogger<ServiceSDK> logger, IResultDispatcher dataTransferCenter,IJobTrackerCache<JobTrackerItem> jobTrackerCache)
    {
        _publisher = publisher;
        _logger = logger;
        _resultDispatcher = dataTransferCenter;
        _jobTrackerCache = jobTrackerCache;
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
            StreamWriter = responseStream,
        };
        var trackerItem = new JobTrackerItem(jobItem.jobId, DateTime.Now);
        try
        {
            await _jobTrackerCache.AddAsync(trackerItem);
            _resultDispatcher.ConsumerRegister(consumer);
            await _publisher.Publish<JobItemMessage>(MasterConst.C_QUEUENAME_JOBITEM, new JobItemMessage(jobItem));
            await Task.Delay(Timeout.Infinite, consumer.CancellationToken);
        }
        catch(OperationCanceledException ex) when( !consumer.IsComplete)
        {
            _logger.LogError("执行超时", ex);
            await _jobTrackerCache.UpdateAsync(trackerItem.JobId, p =>
            {
                p.Status = TrackerStatus.Timeout;
                p.Message = ex.Message;
                p.EndTime = DateTime.Now;
                return p;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("执行任务失败", ex);
            await _jobTrackerCache.UpdateAsync(trackerItem.JobId, p =>
            {
                p.Status = TrackerStatus.Failure;
                p.Message = ex.Message;
                p.EndTime = DateTime.Now;
                return p;
            });
        }
        finally
        {
            _resultDispatcher.ConsumerUnRegister(consumer);
        }
    }
}
