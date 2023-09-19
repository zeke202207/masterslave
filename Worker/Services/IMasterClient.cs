using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Worker
{
    /// <summary>
    /// master通信客户端
    /// </summary>
    public interface IMasterClient
    {
        Task<bool> RegisterNodeAsync(WorkerItem node);

        Task<bool> UnregisterNodeAsync(WorkerItem node);

        Task Start();
    }
}
