﻿using System;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace Slave
{
    public class ExecEnv
    {
        static void Main(string[] args)
        {
            // Reserved for testing - disabled in the multiple startup project 
            // PCS should be the responsible for the slaves creation
        }
    }

    public class Slave : MarshalByRefObject, ISlave, RemoteCmdInterface
    {
        private int opID;
        private Import importObj;
        private Route routeObj;
        private Process processObj;
        private State state;
        private ISlave slaveProxy;

        public State State
        {
            get { return state; }
            set { state = value; }
        }
            
        public ISlave SlaveProxy
        {
            get { return slaveProxy; }
        }

        public Import ImportObj
        {
            get { return importObj; }
            set { importObj = value; }
        }

        public Process ProcessObj
        {
            get { return processObj; }
            set { processObj = value; }
        }

        public Route RouteObj
        {
            get { return routeObj; }
            set { routeObj = value; }
        }

        public Slave(Import importObj, Route routeObj, Process processObj)
        {
            this.importObj = importObj;
            this.routeObj = routeObj;
            this.processObj = processObj;
            state = new UnfrozenState(this);
        }
        
        public void init(int opid)
        {
            TcpChannel channel = new TcpChannel(9000 + opid);
            ChannelServices.RegisterChannel(channel, false);

            // Init the remote proxy to the update
            slaveProxy = (ISlave)Activator.GetObject(
              typeof(ISlave),
              "tcp://localhost:10001/PuppetMaster");

            // Register Slave as remote object
            RemotingServices.Marshal(this, opid.ToString(), typeof(Slave));
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
            opID = opid;
            init(opid);
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
