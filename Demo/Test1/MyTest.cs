using NetX.WorkerPlugin.Contract;
using System.Text;

namespace Test1
{
    [Transient]
    public class MyTest : IJobRunner
    {
        public Task<JobItemResult> RunJobAsync(JobItem job)
        {
            Console.WriteLine($"{Encoding.Default.GetString(job.Data)}");
            return Task.FromResult(new JobItemResult() { JobId = job.JobId, Result = job.Data });
        }
    }
}