using System;
using System.Collections.Generic;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Threading;


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
        bool isLogFull;
        Queue<string> jobQueue;


        public Slave(Import importObj, Route routeObj, Process processObj, string url, string puppetMasterUrl, bool logLevel)
        {
            this.importObj = importObj;
            this.routeObj = routeObj;
            this.processObj = processObj;
            this.url = url;
            this.puppetMasterUrl = puppetMasterUrl;
            this.isLogFull = logLevel;
            state = new UnfrozenState(this);
            jobQueue = new Queue<string>();
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

        public bool IsLogFull
        {
            get { return isLogFull;  }
            set { isLogFull = value; }
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

        // do stuff
        public void Dispatch(string input)
        {
            state.Dispatch(input);
        }

        // puppet master commands

        // start processing
        public void Start()
        {
            init();
            Dispatch(null);
        }

        // sleep ms milliseconds
        public void Interval(int ms)
        {
            Thread.Sleep(ms);
        }

        // print status to console
        public void Status()
        {
            System.Console.WriteLine("Slave at " + url + " is up and running!");
        }

        // crash process
        public void Crash()
        {
            try
            {
                System.Console.WriteLine("Slave " + url + " got crash command...");
                //Thread.Abort(); // NOT WORKING!!!
            }
            catch (ThreadAbortException ex)
            {
                System.Console.WriteLine("Slave " + url + " crashing...");
            }
        }

        // change state to frozen
        public void Freeze()
        {
            state = new FrozenState(this);
        }

        // change state to unfrozen
        public void Unfreeze()
        {
            state = new UnfrozenState(this);
            state.Dispatch(null);
        }

        // exit gracefully
        public void Exit()
        {
            try
            {
                System.Console.WriteLine("Slave " + url + " got exit command...");
                //Thread.Abort(); // NOT WORKING!!!
            }
            catch (ThreadAbortException ex)
            {
                System.Console.WriteLine("Slave " + url + " cleaning up...");
                // empty queue if it is in frozen state?
            }
            finally
            {
                System.Console.WriteLine("Slave " + url + " exiting...");
            }
        }

        // log to Puppet Master (or not!)
        public void ReplicaUpdate(string replicaUrl, List<string> tupleFields)
        {
            state.ReplicaUpdate(replicaUrl, tupleFields);
        }
    }
}
