using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Worker
{

    public class WorkerItem
    {
        public string Id { get; set; }

        public bool IsBusy { get; set; }

        public DateTime LastActiveTime { get; set; }
    }
}
