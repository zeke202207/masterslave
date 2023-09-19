using NetX.WorkerPlugin.Contract;

namespace Test1
{
    [Transient]
    public class MyTest : IJobRunner
    {
        public Task<JobItemResult> RunJobAsync(JobItem job)
        {
            Console.WriteLine("hi,zeke");
            return Task.FromResult(new JobItemResult() { JobId = job.JobId, Result = new byte[] { 0x01, 0x02 } });
        }
    }
}