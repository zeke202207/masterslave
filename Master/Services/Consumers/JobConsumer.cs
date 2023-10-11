﻿using NetX.Common;
using NetX.MemoryQueue;

namespace NetX.Master;

/// <summary>
/// work节点消费者
/// 任务的执行者
/// </summary>
public class JobConsumer : IConsumer<JobItemMessage>
{
    public string QueueName => MasterConst.C_QUEUENAME_JOBITEM;
    private readonly INodeManagement _nodeManager;
    private readonly IPublisher _publisher;
    private readonly IJobExecutor _executor;
    private readonly ILogger _logger;
    private readonly RetryPolicy _retryPolicy;

    /// <summary>
    /// 任务执行者实例对象
    /// </summary>
    /// <param name="nodeManager"></param>
    /// <param name="publisher"></param>
    /// <param name="jobExecutor"></param>
    /// <param name="logger"></param>
    public JobConsumer(INodeManagement nodeManager, IPublisher publisher, IJobExecutor jobExecutor, ILogger<JobConsumer> logger)
    {
        _nodeManager = nodeManager;
        _publisher = publisher;
        _executor = jobExecutor;
        _logger = logger;
        _retryPolicy = new RetryPolicy(maxRetryCount: 10, initialRetryInterval: TimeSpan.FromSeconds(1)); ;
    }

    /// <summary>
    /// 开始执行任务
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
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
            //无可用节点，job的message优先级升高并打入优先级队列队首，等待下一次处理
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
