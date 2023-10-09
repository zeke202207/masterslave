using NetX.MemoryQueue;

namespace NetX.Master.Model;

public class JobItemMessage : MessageArgument
{
    public JobItem Job { get; set; }

    public JobItemMessage(JobItem jobItem)
    {
        Job = jobItem;
    }
}
