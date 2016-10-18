using System;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace ProcessCreationService
{
    public class Slave : CommonTypes.Slave
    {
        private CommonTypes.Slave state = new UnfrozenState();
        private Import importObj;
        private Route routeObj;
        private Process processObj;
        private CommonTypes.Slave slaveProxy;

        public CommonTypes.Slave State
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

            CommonTypes.Slave slaveProxy = (CommonTypes.Slave)Activator.GetObject(
                typeof(CommonTypes.Slave),
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
