using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetX.MasterSDK
{
    public class MasterServiceSDKFactory
    {
        private readonly string _host;

        public MasterServiceSDKFactory(string host)
        {
            _host = host;
        }

        public MasterServiceClient CreateClient()
        {
            return new MasterServiceClient(_host);
        }
    }
}
