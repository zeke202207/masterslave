using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master;

public interface IJob
{
    string Id { get; }

    string Cron { get; }

    public Task<bool> RunJob();
}
