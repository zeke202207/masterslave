using NetX.WorkerPlugin.Contract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.WorkerPlugin.Contract
{
    public interface IJobRunner
    {
        Task<JobItemResult> RunJobAsync(JobItem job);
    }
}
