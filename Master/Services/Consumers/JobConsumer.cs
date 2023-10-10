using NetX.Common;
using NetX.Master.Model;
using NetX.MemoryQueue;

namespace NetX.Master;

public class JobConsumer : IConsumer<JobItemMessage>
{
    public string QueueName => MasterConst.C_QUEUENAME_JOBITEM;
    private readonly INodeManagement _nodeManager;
    private readonly IPublisher _publisher;
    private readonly IJobExecutor _executor;
    private readonly ILogger _logger;
    private readonly RetryPolicy _retryPolicy;

    public JobConsumer(INodeManagement nodeManager, IPublisher publisher, IJobExecutor jobExecutor, ILogger<JobConsumer> logger)
    {
        _nodeManager = nodeManager;
        _publisher = publisher;
        _executor = jobExecutor;
        _logger = logger;
        _retryPolicy = new RetryPolicy(maxRetryCount: 10, initialRetryInterval: TimeSpan.FromSeconds(1)); ;
    }

    public async Task Handle(JobItemMessage message)
    {
        Console.WriteLine(message.Timestamp.ToString());
        //获取下一个可用的工作节点
        WorkerNode node = null;
        await _retryPolicy.ExecuteAsync(
               async () =>
               {
                   node = _nodeManager.GetAvailableNode();
                   if (null == node)
                       throw new NodeNotFoundException();
                   await Task.CompletedTask;
               },
               ex => ex is NodeNotFoundException nodeEx);
        if (node == null)
        {
            //无可用节点，job打入队首等待下一次处理
            message.Priority -= 1;
            await _publisher.Publish<JobItemMessage>(MasterConst.C_QUEUENAME_JOBITEM, message);
            return;
        }
        try
        {
            await _executor.ExecuteJobAsync(node.Id, message.Job);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "处理队列job失败");
        }
    }
}
