using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PuppetMaster
    {
        private static int port = 10001;

        private Log mLogger;
        private Dictionary<uint, Operator> mOperators;

        public PuppetMaster(List<String> urlsOfPCS, string configFilePath = @"config.cfg")
        {
            //if (log == full) mLogger = new FullLog();
            //else mLogger = new LightLog();
            mOperators = new Dictionary<uint, Operator>();
            // ...
            ChannelServices.RegisterChannel(new TcpChannel(port), false);

        }

        void init()
        {
            //parse
            //launch
        }
    }
}
