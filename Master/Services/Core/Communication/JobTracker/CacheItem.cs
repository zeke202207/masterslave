using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.Master.Services.Core;

public abstract class CacheItem
{
    public string JobId { get; protected set; }
}
