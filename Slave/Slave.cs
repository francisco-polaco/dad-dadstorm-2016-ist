using System;
using System.Collections.Generic;
using System.IO;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;


namespace Slave
{
    public class ExecEnv
    {
        static void Main(string[] args)
        {
            Import import = (Import)DeserializeObject(args[0]);
            Route route = (Route)DeserializeObject(args[1]);
            Process process = (Process)DeserializeObject(args[2]);
            string url = (string)DeserializeObject(args[3]);
            string puppetMasterUrl = (string)DeserializeObject(args[4]);
            bool logLevel = (bool)DeserializeObject(args[5]);

            var slave = new Slave(import,route,process,url,puppetMasterUrl,logLevel);
            Console.ReadLine();
        }

        private static object DeserializeObject(string str)
        {
            byte[] bytes = Convert.FromBase64String(str);

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                return new BinaryFormatter().Deserialize(stream);
            }
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
            state = new FrozenState(this);
            jobQueue = new Queue<string>();
            init();
        }

        // getters, setters

        public Queue<string> JobQueue
        {
            get { return jobQueue;}
        }

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
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this, "op", typeof(Slave));
            Console.WriteLine("Slave with url " + url + " is listening!");

            // init client to log puppetLogProxy
           puppetLogProxy = (ILogUpdate) Activator.GetObject(typeof(ILogUpdate), puppetMasterUrl);

            if (puppetLogProxy == null)
                System.Console.WriteLine("Could not connect to PuppetMaster on " + puppetMasterUrl);
        }

        // do stuff
        public void Dispatch(string input)
        {
            Console.WriteLine("Dispatch method called...");
            state.Dispatch(input);
        }

        // puppet master commands

        // start processing
        public void Start()
        {
            Console.WriteLine("Slave with url " + url + " is starting!");
            state = new UnfrozenState(this);
            Dispatch(null);
        }

        // sleep ms milliseconds
        public void Interval(int ms)
        {
            Console.WriteLine("Slave with url " + url + " going to sleep!");
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
            System.Console.WriteLine("Slave " + url + " got crash command...");
            Environment.Exit(1);      
        }

        // change state to frozen
        public void Freeze()
        {
            state = new FrozenState(this);
            Console.WriteLine("Slave with url " + url + " is now fozen!");
        }

        // change state to unfrozen
        public void Unfreeze()
        {
            state = new UnfrozenState(this);

            Console.WriteLine("Slave with url " + url + " is now unfozen!");
            // dispatch all queued jobs
            try
            {
                string tuple = null;
                for(int i = 0; i < jobQueue.Count; i++)
                {
                    tuple = getJob();
                    Console.WriteLine("Slave with url " + url + " dispatching job #" + i + " in the queue!");
                    state.Dispatch(tuple);
                }  
            }
            catch (InvalidOperationException ex)
            {
                System.Console.WriteLine("jobQueue empty!");
            }
        }

        // exit gracefully
        public void Exit()
        {
            Unfreeze(); // even if the current state is unfrozen, it unfrozes again to dispatch queued jobs

            System.Console.WriteLine("Slave " + url + " exiting...");
            Environment.Exit(1);
        }

        // log to Puppet Master (or not!)
        public void ReplicaUpdate(string replicaUrl, List<string> tupleFields)
        {
            state.ReplicaUpdate(replicaUrl, tupleFields);
        }

        /// auxiliary: add job to jobQueue
        public void addJob(string tuple)
        {
            jobQueue.Enqueue(tuple);
        }

        /// auxiliary: get job from jobQueue
        /// 
        /// <exception cref="InvalidOperationException">Thrown if Queue is empty.</exception>
        public string getJob()
        {
            return jobQueue.Dequeue();
        }
    }
}
