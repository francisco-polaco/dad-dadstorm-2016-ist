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

            Console.Title = url;
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
        private int _seqNumber = 0;
        private string _url;
        private Import _importObj;
        private Route _routeObj;
        private Process _processObj;
        private State _state;
        private ILogUpdate _puppetLogProxy;
        private string _puppetMasterUrl;
        bool _isLogFull;
        Queue<TuplePack> _jobQueue;


        public Slave(Import importObj, Route routeObj, Process processObj, string url, string puppetMasterUrl, bool logLevel)
        {
            this._importObj = importObj;
            this._routeObj = routeObj;
            this._processObj = processObj;
            this._url = url;
            this._puppetMasterUrl = puppetMasterUrl;
            this._isLogFull = logLevel;
            _state = new FrozenState(this);
            _jobQueue = new Queue<TuplePack>();
            init();
        }

        // getters, setters

        public int SeqNumber
        {
            get { return _seqNumber; }
            set { _seqNumber = value; }
        }

        public Queue<TuplePack> JobQueue => _jobQueue;

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public string PuppetMasterUrl
        {
            get { return _puppetMasterUrl; }
            set { _puppetMasterUrl = value; }
        }

        public State State
        {
            get { return _state; }
            set { _state = value; }
        }
            
        public ILogUpdate PuppetLogProxy
        {
            get { return _puppetLogProxy; }
        }

        public Import ImportObj
        {
            get { return _importObj; }
            set { _importObj = value; }
        }

        public Process ProcessObj
        {
            get { return _processObj; }
            set { _processObj = value; }
        }

        public Route RouteObj
        {
            get { return _routeObj; }
            set { _routeObj = value; }
        }

        public bool IsLogFull
        {
            get { return _isLogFull;  }
            set { _isLogFull = value; }
        }
        
        // other methods

        public void init()
        {
            // get port
            int portStart = _url.IndexOf(':', 4) + 1; // index of first digit
            int portLength = _url.IndexOf("/", portStart) - portStart; // number of digits
            int port = int.Parse(_url.Substring(portStart, portLength));

            // init server
            TcpChannel channel = new TcpChannel(port);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this, "op", typeof(Slave));
            Console.WriteLine("Slave with url " + _url + " is listening!");

            // init client to log puppetLogProxy
           _puppetLogProxy = (ILogUpdate) Activator.GetObject(typeof(ILogUpdate), _puppetMasterUrl);

            if (_puppetLogProxy == null)
                System.Console.WriteLine("Could not connect to PuppetMaster on " + _puppetMasterUrl);
        }

        // do stuff
        public void Dispatch(TuplePack input)
        {
            Console.WriteLine("Dispatch method called...");
            _state.Dispatch(input);
        }

        // puppet master commands

        // start processing
        public void Start()
        {
            Console.WriteLine("Slave with url " + _url + " is starting!");
            _state = new UnfrozenState(this);
            Dispatch(null);
        }

        // sleep ms milliseconds
        public void Interval(int ms)
        {
            Console.WriteLine("Slave with url " + _url + " going to sleep!");
            Thread.Sleep(ms);
        }

        // print status to console
        public void Status()
        {
            System.Console.WriteLine("Slave at " + _url + " is up and running!");
        }

        // crash process
        public void Crash()
        {
            System.Console.WriteLine("Slave " + _url + " got crash command...");
            Environment.Exit(1);      
        }

        // change state to frozen
        public void Freeze()
        {
            _state = new FrozenState(this);
            Console.WriteLine("Slave with url " + _url + " is now fozen!");
        }

        // change state to unfrozen
        public void Unfreeze()
        {
            _state = new UnfrozenState(this);

            Console.WriteLine("Slave with url " + _url + " is now unfozen!");
            // dispatch all queued jobs
            try
            {
                TuplePack tuple = null;
                for(int i = 0; i < _jobQueue.Count; i++)
                {
                    tuple = GetJob();
                    Console.WriteLine("Slave with url " + _url + " dispatching job #" + i + " in the queue!");
                    _state.Dispatch(tuple);
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

            System.Console.WriteLine("Slave " + _url + " exiting...");
            Environment.Exit(1);
        }

        // log to Puppet Master (or not!)
        public void ReplicaUpdate(string replicaUrl, IList<string> tupleFields)
        {
            _state.ReplicaUpdate(replicaUrl, tupleFields);
        }

        /// auxiliary: add job to jobQueue
        public void AddJob(TuplePack tuple)
        {
            _jobQueue.Enqueue(tuple);
        }

        /// auxiliary: get job from jobQueue
        /// 
        /// <exception cref="InvalidOperationException">Thrown if Queue is empty.</exception>
        public TuplePack GetJob()
        {
            return _jobQueue.Dequeue();
        }
    }
}
