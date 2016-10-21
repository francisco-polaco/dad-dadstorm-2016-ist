using System;
using CommonTypes;
using System.Collections;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;

namespace ProcessCreationService
{
    public class Slave : MarshalByRefObject, ISlave, RemoteCmdInterface
    {
        private State state;
        private FrozenState frozenState;
        private UnfrozenState unfrozenState;
        private ISlave slaveProxy;

        public State State
        {
            get { return state; }
            set { state = value; }
        }

        public Slave(Import importObj, Route routeObj, Process processObj)
        {
            frozenState = new FrozenState(importObj, routeObj, processObj);
            unfrozenState = new UnfrozenState(importObj, routeObj, processObj);
            state = unfrozenState;
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
