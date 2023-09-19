using Grpc.Core;
using MasterSDKService;
using NetX.Master.Model;
using NetX.MemoryQueue;

namespace NetX.Master
{
    public class MasterServiceSDK :  MasterSDKService.MasterServiceSDK.MasterServiceSDKBase
    {
        private readonly IPublisher _publisher;
        private readonly ILogger _logger;

        public MasterServiceSDK(IPublisher publisher, ILogger<MasterServiceSDK> logger) 
        {
            _publisher = publisher;
            _logger = logger;
        }

        public override async Task<ExecuteTaskResponse> ExecuteTask(ExecuteTaskRequest request, ServerCallContext context)
        {
            try
            {
                //1. Create a job and add it to the queue
                var jobItem = new JobItem(Guid.NewGuid().ToString("N"), request.Data.ToByteArray());
                await _publisher.Publish<JobItemMessage>(MasterConst.C_QUEUENAME_JOBITEM, new JobItemMessage(jobItem));
                //2. Return the job id
                return await Task.FromResult(new ExecuteTaskResponse() { JobId = jobItem.jobId });
            }
            catch (Exception ex)
            {
                _logger.LogError("执行任务失败", ex);
                return await Task.FromResult(new ExecuteTaskResponse() { ErrorMessage = $"执行任务失败：{ex.ToString()}" });
            }
        }

        public override Task<CancelTaskResponse> CancelTask(CancelTaskRequest request, ServerCallContext context)
        {
            return base.CancelTask(request, context);
        }

        public override Task<GetJobResultResponse> GetJobResult(GetJobResultRequest request, ServerCallContext context)
        {
            return base.GetJobResult(request, context);
        }

        public override Task<GetJobStatusResponse> GetJobStatus(GetJobStatusRequest request, ServerCallContext context)
        {
            return base.GetJobStatus(request, context);
        }
    }
}
