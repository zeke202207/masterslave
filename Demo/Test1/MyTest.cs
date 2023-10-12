using NetX.WorkerPlugin.Contract;
using System.Text;

namespace Test1
{
    [Transient]
    public class MyTest : IJobRunner
    {
        private Guid _id;

        public MyTest() 
        { 
            _id = Guid.NewGuid();
        }

        public Task<JobItemResult> RunJobAsync(JobItem job)
        {
            Console.WriteLine($"{Encoding.Default.GetString(job.Data)}");
            return Task.FromResult(new JobItemResult() { JobId = job.JobId, Result = File.ReadAllBytes(@"1.7z") });
        }
    }
}