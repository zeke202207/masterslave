using ClassLibrary1;
using NetX.WorkerPlugin.Contract;
using System.Text;

namespace Test1;

[Transient]
public class MyTest : IJobRunner
{
    private Guid _id;
    private readonly IZekeTransient _zeke1;
    private readonly IZekeSingleton _zeke2;

    public MyTest(IZekeTransient zeke1, IZekeSingleton zeke2) 
    { 
        _id = Guid.NewGuid();
        _zeke1 = zeke1;
        _zeke2 = zeke2;
    }

    public Task<JobItemResult> RunJobAsync(JobItem job)
    {
        IZekeLib lib = new ZekeLib();
        lib.Test(false);

        _zeke1.Test();
        _zeke2.Test();
        Console.WriteLine($"{Encoding.Default.GetString(job.Data)}");
        return Task.FromResult(new JobItemResult() { JobId = job.JobId, Result = File.ReadAllBytes(@"1.7z") });
    }

    public void Dispose()
    {
        //TODO:释放资源
    }
}