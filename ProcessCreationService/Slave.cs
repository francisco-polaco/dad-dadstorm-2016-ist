using System;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace ProcessCreationService
{
    public class Slave : CommonTypes.ISlave
    {
        private CommonTypes.ISlave state = new UnfrozenState();
        private Import importObj;
        private Route routeObj;
        private Process processObj;
        private CommonTypes.ISlave slaveProxy;

        public CommonTypes.ISlave State
        {
            get { return state; }
            set { state = value; }
        }

        public Slave(Import importObj, Route routeObj, Process processObj)
        {
            this.importObj = importObj;
            this.routeObj = routeObj;
            this.processObj = processObj;
            init();
        }

        private void init() {
            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);

            slaveProxy = (CommonTypes.ISlave)Activator.GetObject(
                typeof(CommonTypes.ISlave),
                "tcp://localhost:10001/PuppetMaster");
        }

        public void Dispatch(string input)
        {
            state.Dispatch(input);
        }

        public void Update(string toUpdate)
        {
            if (slaveProxy != null)
                state.Update(toUpdate);
        }
    }
}
