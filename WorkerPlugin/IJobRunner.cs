namespace NetX.WorkerPlugin.Contract;

public interface IJobRunner
{
    Task<JobItemResult> RunJobAsync(JobItem job);
}
