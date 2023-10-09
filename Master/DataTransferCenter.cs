using Google.Protobuf;
using Grpc.Core;
using MasterSDKService;
using System.Collections.Concurrent;

namespace NetX.Master;

public class DataTransferCenter
{
    private BlockingCollection<ResultModel> _results = new BlockingCollection<ResultModel>();
    private ConcurrentDictionary<string, DataTransferResultConsumer> _consumers = new();
    private readonly ILogger _logger;
    private int i = 0;

    public DataTransferCenter(ILogger<DataTransferCenter> logger)
    {
        _logger = logger;
        Task.Factory.StartNew(async () =>
        {
            foreach (var result in _results.GetConsumingEnumerable())
            {
                try
                {
                    if (!_consumers.ContainsKey(result.JobId))
                        continue;
                    var consumer = _consumers[result.JobId];
                    await consumer.StreamWriter.WriteAsync(new ExecuteTaskResponse() { Result = ByteString.CopyFrom(result.Result) });
                    consumer.TokenSource.Cancel();
                }
                catch (Exception ex)
                {

                }
            }
        });
    }


    public void Regist(DataTransferResultConsumer consumer)
    {
        _consumers.AddOrUpdate(consumer.JobId, consumer, (oldKey, oldValue) => consumer);
    }

    public void UnRegist(DataTransferResultConsumer consumer)
    {
        _consumers.Remove(consumer.JobId, out _);
    }

    public void ProcessData(ResultModel result)
    {
        _results.TryAdd(result);
    }
}

public class ResultModel
{
    public string JobId { get; set; }

    public byte[] Result { get; set; }
}

public class DataTransferResultConsumer
{
    public string JobId { get; set; }

    public CancellationTokenSource TokenSource { get; set; }

    public IServerStreamWriter<ExecuteTaskResponse> StreamWriter { get; set; }
}
