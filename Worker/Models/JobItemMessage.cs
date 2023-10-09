using NetX.MemoryQueue;
using NetX.WorkerPlugin.Contract;

namespace NetX.Worker.Models;

public class JobItemMessage : MessageArgument
{
    public JobItem Job { get; set; }

    public JobItemMessage()
    {

    }
}
