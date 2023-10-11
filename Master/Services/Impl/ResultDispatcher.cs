﻿using Google.Protobuf;
using MasterSDKService;
using System.Collections.Concurrent;

namespace NetX.Master;

/// <summary>
/// 结果分发器
/// 根据JobId，将worker处理结果分发给任务的调用者
/// </summary>
public sealed class ResultDispatcher : IResultDispatcher
{
    /// <summary>
    /// 任务执行结果集合
    /// </summary>
    private BlockingCollection<ResultModel> _results = new BlockingCollection<ResultModel>();
    /// <summary>
    /// 任务Id与任务创建者集合
    /// </summary>
    private ConcurrentDictionary<string, ResultDispatcherConsumer> _consumers = new();
    private readonly ILogger _logger;

    /// <summary>
    /// 结果分发器实例对象
    /// </summary>
    /// <param name="logger"></param>
    public ResultDispatcher(ILogger<ResultDispatcher> logger)
    {
        _logger = logger;
        //开启新的线程，监听任务结果集合
        Task.Factory.StartNew(() => HandlingResultListener());
    }

    /// <summary>
    /// 注册结果监听
    /// </summary>
    /// <param name="consumer"></param>
    public void ConsumerRegister(ResultDispatcherConsumer consumer)
    {
        _consumers.AddOrUpdate(consumer.JobId, consumer, (oldKey, oldValue) => consumer);
    }

    /// <summary>
    /// 取消结果监听注册
    /// </summary>
    /// <param name="consumer"></param>
    public void ConsumerUnRegister(ResultDispatcherConsumer consumer)
    {
        _consumers.Remove(consumer.JobId, out _);
    }

    /// <summary>
    /// 记录结果
    /// </summary>
    /// <param name="result"></param>
    public void WriteResult(ResultModel result)
    {
        _results.TryAdd(result);
    }

    /// <summary>
    /// 获取全部订阅的结果消费者
    /// </summary>
    /// <returns></returns>
    public IEnumerable<ResultDispatcherConsumer> GetConsumers()
    {
        foreach (var consumer in _consumers)
            yield return consumer.Value;
    }

    /// <summary>
    /// 处理结果监听
    /// </summary>
    private void HandlingResultListener()
    {
        foreach (var result in _results.GetConsumingEnumerable())
        {
            Task.Run(() =>
            {
                try
                {
                    if (!_consumers.ContainsKey(result.JobId))
                        return;
                    var consumer = _consumers[result.JobId];
                    var _ = consumer.StreamWriter.WriteAsync(new ExecuteTaskResponse() { Result = ByteString.CopyFrom(result.Result) }, consumer.CancellationToken);
                    consumer.Complete();
                }
                catch (Exception ex)
                {
                    _logger.LogError("获取结果失败", ex);
                }
            });
        }
    }
}