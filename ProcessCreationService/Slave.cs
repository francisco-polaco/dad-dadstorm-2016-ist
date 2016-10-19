using System;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace ProcessCreationService
{
    public class Slave : MarshalByRefObject, ISlave, RemoteCmdInterface
    {
        private ISlave state = new UnfrozenState();
        private Import importObj;
        private Route routeObj;
        private Process processObj;
        private ISlave slaveProxy;

        public ISlave State
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

            slaveProxy = (ISlave)Activator.GetObject(
                typeof(ISlave),
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

        public void Start(int opid)
        {
            throw new NotImplementedException();
        }

        public void Interval(int opid, int ms)
        {
            throw new NotImplementedException();
        }

        public void Status()
        {
            throw new NotImplementedException();
        }

        public void Crash(string url)
        {
            throw new NotImplementedException();
        }

        public void Freeze(string url)
        {
            throw new NotImplementedException();
        }

        public void Unfreeze(string url)
        {
            throw new NotImplementedException();
        }
    }
}
