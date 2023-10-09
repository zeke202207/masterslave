namespace NetX.Master;

public interface IJobExecutor
{
    Task ExecuteJobAsync(string workerNodeId, JobItem job);
}
