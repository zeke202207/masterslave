using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.MasterSDK;

public class ConnectedResult
{
    public bool Success { get; set; } = false;

    public Exception Exception { get; set; }

    public string Token { get; set; }
}
