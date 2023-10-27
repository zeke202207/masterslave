using NetX.Master.Services.Core;
using NetX.MemoryQueue;
using SDK;

namespace NetX.Master;

/// <summary>
/// 执行job中间件
/// </summary>
public class ExecuteTaskMiddleware : IApplicationMiddleware<GrpcContext<ExecuteTaskRequest, ExecuteTaskResponse>>
{
    private readonly IPublisher _publisher;
    private readonly ILogger _logger;
    private readonly IResultDispatcher _resultDispatcher;
    private readonly IJobTrackerCache<JobTrackerItem> _jobTrackerCache;

    public ExecuteTaskMiddleware(
        IPublisher publisher, 
        ILogger<ExecuteTaskMiddleware> logger, 
        IResultDispatcher resultDispatcher, 
        IJobTrackerCache<JobTrackerItem> jobTrackerCache)
    {
        _publisher = publisher;
        _logger = logger;
        _resultDispatcher = resultDispatcher;
        _jobTrackerCache = jobTrackerCache;
    }

    public async Task InvokeAsync(
        ApplicationDelegate<GrpcContext<ExecuteTaskRequest, ExecuteTaskResponse>> next,
        GrpcContext<ExecuteTaskRequest, ExecuteTaskResponse> context)
    {
        int timeout = context.Reqeust.Request.Timeout <= 0 ? 60 : context.Reqeust.Request.Timeout;
        //1. Create a job and add it to the queue
        var jobItem = new JobItem(Guid.NewGuid().ToString("N"), context.Reqeust.Request.Data.ToByteArray(), context.Reqeust.Request.Metadata.ToDictionary(kv => kv.Key, kv => kv.Value));
        var consumer = new ResultDispatcherConsumer(timeout, _resultDispatcher, _jobTrackerCache)
        {
            JobId = jobItem.jobId,
            StreamWriter = context.Response.ResponseStream,
        };
        var trackerItem = new JobTrackerItem(jobItem.jobId, DateTime.Now);
        try
        {
            await _jobTrackerCache.AddAsync(trackerItem);
            _resultDispatcher.ConsumerRegister(consumer);
            await _publisher.Publish<JobItemMessage>(MasterConst.C_QUEUENAME_JOBITEM, new JobItemMessage(jobItem));
            await Waiting(consumer.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError("执行任务失败", ex);
            consumer.FailedCompleted(ex);
        }
        finally
        {
            _resultDispatcher.ConsumerUnRegister(consumer);
        }
    }

    /// <summary>
    /// 等待任务执行
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task Waiting(CancellationToken cancellationToken)
    {
        //Task.Delay(Timeout.Infinite, cancellationToken), 会抛出异常,性能有损
        var delayTask = Task.Delay(Timeout.Infinite, cancellationToken);
        var cancelTask = Task.Delay(Timeout.Infinite, CancellationToken.None).ContinueWith(t => { });
        var completedTask = await Task.WhenAny(delayTask, cancelTask);
        //if (completedTask == delayTask)
        //{
        //    // 取消令牌被触发，不执行后续代码
        //    return;
        //}
    }
}
