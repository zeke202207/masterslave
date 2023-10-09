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

    public JobConsumer(INodeManagement nodeManager, IPublisher publisher, IJobExecutor jobExecutor, ILogger<JobConsumer> logger)
    {
        _nodeManager = nodeManager;
        _publisher = publisher;
        _executor = jobExecutor;
        _logger = logger;
    }

    public async Task Handle(JobItemMessage message)
    {
        Console.WriteLine(message.Timestamp.ToString());
        //获取下一个可用的工作节点
        WorkerNode node = _nodeManager.GetAvailableNode();
        if (node == null)
        {
            //打入队尾
            await _publisher.Publish<JobItemMessage>(MasterConst.C_QUEUENAME_JOBITEM, message);
            //10s后再次处理(可以提取到配置文件)
            await Task.Delay(TimeSpan.FromSeconds(10));
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
