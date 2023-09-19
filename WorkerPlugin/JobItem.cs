using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.WorkerPlugin.Contract
{
    public class JobItem
    {
        public string JobId { get; set; }

        public byte[] Data { get; set; }
    }
}
