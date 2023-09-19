using Grpc.Core;
using MasterWorkerService;

namespace NetX.Master
{
    /// <summary>
    /// 任务观察者
    /// 将任务下发到节点处理
    /// </summary>
    public class JobObserver : IObserver<WorkerJob>, IEquatable<IObserver<WorkerJob>>
    {
        private readonly IServerStreamWriter<ListenForJobResponse> _responseStream;
        private readonly string _workerId;

        public event Action<WorkerJob> OnBeforeExcuteJob;
        public event Action<Exception> OnErroredJob;
        public event Action OnCompletedJob;

        public string WorkerId => _workerId;

        public JobObserver(string workerId, IServerStreamWriter<ListenForJobResponse> responseStream)
        {
            _responseStream = responseStream;
            _workerId = workerId;
        }

        /// <summary>
        ///  Provides the observer with new data.
        /// </summary>
        /// <param name="workJob"></param>
        public async void OnNext(WorkerJob workJob)
        {
            try
            {
                if (!_workerId.Equals(workJob.WorkerId))
                    throw new NotSupportedException("任务下发节点与观察者关注节点不一致");
                OnBeforeExcuteJob?.Invoke(workJob);
                var response = new ListenForJobResponse { JobId = workJob.JobItem.jobId, Data = Google.Protobuf.ByteString.CopyFrom(workJob.JobItem.jobData) };
                await _responseStream.WriteAsync(response);
            }
            catch (Exception ex)
            {
                OnErroredJob?.Invoke(ex);
            }
        }

        /// <summary>
        ///  Notifies the observer that the provider has experienced an error condition.
        /// </summary>
        /// <param name="error"></param>
        public void OnError(Exception error)
        {
            OnErroredJob?.Invoke(error);
        }

        /// <summary>
        /// Notifies the observer that the provider has finished sending push-based notifications.
        /// </summary>
        public void OnCompleted()
        {
            OnCompletedJob?.Invoke();
        }

        public bool Equals(IObserver<WorkerJob> other)
        {
            if (other == null)
                return false;
            JobObserver jobObserver = other as JobObserver;
            if (jobObserver == null)
                return false;
            return WorkerId.Equals(jobObserver.WorkerId);
        }
    }
}
