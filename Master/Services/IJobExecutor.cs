using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master
{
    public interface IJobExecutor
    {
        Task ExecuteJobAsync(string workerNodeId, JobItem job);
    }
}
