using NetX.WorkerPlugin.Contract;
using System.Text;

namespace Test1
{
    [Transient]
    public class MyTest : IJobRunner
    {
        public Task<JobItemResult> RunJobAsync(JobItem job)
        {
            Console.WriteLine("hi,zeke");
            return Task.FromResult(new JobItemResult() { JobId = job.JobId, Result = Encoding.Default.GetBytes("zeke") });
        }
    }
}