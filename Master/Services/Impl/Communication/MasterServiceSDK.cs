using Grpc.Core;
using MasterSDKService;
using NetX.Master.Model;
using NetX.MemoryQueue;

namespace NetX.Master;

public class MasterServiceSDK : MasterSDKService.MasterServiceSDK.MasterServiceSDKBase
{
    private readonly IPublisher _publisher;
    private readonly ILogger _logger;
    private readonly DataTransferCenter _dataTransferCenter;

    public MasterServiceSDK(IPublisher publisher, ILogger<MasterServiceSDK> logger, DataTransferCenter dataTransferCenter)
    {
        _publisher = publisher;
        _logger = logger;
        _dataTransferCenter = dataTransferCenter;
    }

    public override async Task ExecuteTask(ExecuteTaskRequest request, IServerStreamWriter<ExecuteTaskResponse> responseStream, ServerCallContext context)
    {
        //1. Create a job and add it to the queue
        var jobItem = new JobItem(Guid.NewGuid().ToString("N"), request.Data.ToByteArray());
        var consumer = new DataTransferResultConsumer()
        {
            JobId = jobItem.jobId,
            TokenSource = new CancellationTokenSource(),
            StreamWriter = responseStream,
        };
        try
        {
            consumer.TokenSource.CancelAfter(TimeSpan.FromSeconds(30));
            _dataTransferCenter.Regist(consumer);
            await _publisher.Publish<JobItemMessage>(MasterConst.C_QUEUENAME_JOBITEM, new JobItemMessage(jobItem));
            await Task.Delay(Timeout.Infinite, consumer.TokenSource.Token);
        }
        catch (Exception ex)
        {
            //TODO:
        }
        finally
        {
            _dataTransferCenter.UnRegist(consumer);
        }
    }
}
