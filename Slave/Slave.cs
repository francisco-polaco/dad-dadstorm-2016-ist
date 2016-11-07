using System;
using System.Collections.Generic;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;
using System.Linq;

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

    public class Slave : MarshalByRefObject, ISlave, IRemoteCmdInterface, ILogUpdate
    {
        private string url;
        private Import importObj;
        private Route routeObj;
        private Process processObj;
        private State state;
        private ILogUpdate puppetLogProxy;
        private string puppetMasterUrl;


        public Slave(Import importObj, Route routeObj, Process processObj, string url, string puppetMasterUrl)
        {
            this.importObj = importObj;
            this.routeObj = routeObj;
            this.processObj = processObj;
            this.url = url;
            state = new UnfrozenState(this);
        }

        // getters, setters

        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public string PuppetMasterUrl
        {
            get { return puppetMasterUrl; }
            set { puppetMasterUrl = value; }
        }

        public State State
        {
            get { return state; }
            set { state = value; }
        }
            
        public ILogUpdate PuppetLogProxy
        {
            get { return puppetLogProxy; }
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
        
        // other methods

        public void init()
        {
            // get port
            int portStart = this.url.IndexOf(':', 4) + 1; // index of first digit
            int portLength = this.url.IndexOf("/", portStart) - portStart; // number of digits
            int port = int.Parse(url.Substring(portStart, portLength));

            // init server
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this, "slave", typeof(Slave));
            Console.WriteLine("Slave with url " + url + " is listening!");

            // init client to log puppetLogProxy
           puppetLogProxy = (ILogUpdate) Activator.GetObject(typeof(ILogUpdate), puppetMasterUrl);

            if (puppetLogProxy == null)
                System.Console.WriteLine("Could not connect to PuppetMaster on " + puppetMasterUrl);
        }

        public void Dispatch(string input)
        {
            state.Dispatch(input);
        }

        public void Start()
        {
            init();
            Dispatch(null);
        }

        public void Interval(int ms)
        {
            throw new NotImplementedException();
        }

        public void Status()
        {
            throw new NotImplementedException();
        }

        public void Crash()
        {
            throw new NotImplementedException();
        }

        public void Freeze()
        {
            throw new NotImplementedException();
        }

        public void Unfreeze()
        {
            throw new NotImplementedException();
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        public void ReplicaUpdate(string replicaUrl, List<string> tupleFields)
        {
            throw new NotImplementedException();
        }
    }
}
