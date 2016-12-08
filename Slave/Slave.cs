using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            string semantic = (string) DeserializeObject(args[6]);
            List<string> siblings = (List<string>) DeserializeObject(args[7]);
            bool stateful = (bool) DeserializeObject(args[8]);

            var slave = new Slave(import,route,process,url,puppetMasterUrl,logLevel,semantic,siblings,stateful);

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

    public class Slave : MarshalByRefObject, ISlave, IRemoteCmdInterface, ILogUpdate, ISibling
    {
        private bool _stateful;
        private System.Random _rnd;
        private int _seqNumber = 0;
        private string _url;
        private Import _importObj;
        private Route _routeObj;
        private Process _processObj;
        private State _state;
        private ILogUpdate _puppetLogProxy;
        private string _puppetMasterUrl;
        bool _isLogFull;
        ConcurrentQueue<TuplePack> _jobQueue;
        private int _interval = 0;
        private IList<TuplePack> _seenTuplePacks;
        private string _semantic;
        private List<string> _siblings;

        public Slave(Import importObj, Route routeObj, Process processObj, string url, string puppetMasterUrl, bool logLevel, string semantic, List<string> siblings, bool stateful)
        {
            _importObj = importObj;
            _routeObj = routeObj;
            _processObj = processObj;
            _url = url;
            _puppetMasterUrl = puppetMasterUrl;
            _isLogFull = logLevel;
            _state = new FrozenState(this);
            _jobQueue = new ConcurrentQueue<TuplePack>();
            _seenTuplePacks = new List<TuplePack>();
            _semantic = semantic;
            _siblings = siblings;
            _stateful = stateful;
            _rnd = new System.Random();
            init();
        }

        // getters, setters

        public bool Stateful
        {
            get { return _stateful; }
            set { _stateful = value; }
        }

        public System.Random RandSeed => _rnd;

        public List<string> Siblings
        {
            get { return _siblings; }
            set { _siblings = value; }
        }

        public string Semantic => _semantic;

        public IList<TuplePack> SeenTuplePacks => _seenTuplePacks;

        public int IntervalValue
        {
            get { return _interval; }
            set { _interval = value; }
        }

        public int SeqNumber
        {
            get { return _seqNumber; }
            set { _seqNumber = value; }
        }

        public ConcurrentQueue<TuplePack> JobQueue
        {
            get { return _jobQueue; }
            set { _jobQueue = value; }
        }

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
        public override object InitializeLifetimeService()
        {
            return null;
        }

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
            if (_semantic.ToLower().Equals("exactly-once")) {
                if(input != null && !
                    RouteObj.IsLast()) {
                    // Exponencial backoff
                    double baseExponent = 9;
                    bool whileCond = true;
                    while (whileCond)
                    {
                        try
                        {
                            // maybe one of my brothers already seen it and i don't need to purpose
                            whileCond = State.TryToPurpose(input);
                            if (SeenTuplePacks.Contains(input))
                                return;
                            Console.WriteLine("Purposing tuple to my sibilings!!");
                            Thread.Sleep(Convert.ToInt32((Math.Pow(2, baseExponent) - 1)/2));
                            baseExponent += _rnd.NextDouble();
                        }
                        catch (Exception e)
                        {
                            whileCond = false;
                        }
                    }
                }
                lock (this)
                {
                    _state.Dispatch(input);
                }
            }
            else 
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
            Console.WriteLine("Slave with url " + _url + " applying " + ms + " interval to operations!");
            _interval = ms;
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
            Console.WriteLine("Slave with url " + _url + " is now frozen!");
        }

        // change state to unfrozen
        public void Unfreeze()
        {
            Console.WriteLine("Slave with url " + _url + " is now unfrozen!");
            _state = new UnfrozenState(this);
            int size = _jobQueue.Count();
            for (int i = 0; i < size; i++)
            {
                var tuple = GetJob();
                Console.WriteLine("Slave with url " + _url + " dispatching job #" + i + " in the queue!");
                Dispatch(tuple);
            }
        }

        // log to Puppet Master (or not!)
        public void ReplicaUpdate(string replicaUrl, IList<string> tupleFields)
        {
            _state.ReplicaUpdate(replicaUrl, tupleFields);
        }

        /// auxiliary: add job to jobQueue
        public void AddJob(TuplePack tuple)
        {
            Console.WriteLine(tuple);
            if(_jobQueue.Contains(tuple))
                return;
            _jobQueue.Enqueue(tuple);
        }

        /// <summary>
        /// It should be used only after testing if the queue is empty!
        /// If the previous condition doens't verify - null to safeguard.
        /// </summary>
        /// <returns></returns>
        public TuplePack GetJob()
        {
            if (_jobQueue.Count == 0)
                return null;
            TuplePack job;
            while (!_jobQueue.TryDequeue(out job)){}
            return job;
        }

        public bool PollTuple(TuplePack toRoute)
        {
            return State.PollTuple(toRoute);
        }

        public void AnnounceTuple(TuplePack toAnnounce)
        {
            State.AnnounceTuple(toAnnounce);
        }

        public bool Purpose(TuplePack toDispatch)
        {
            return State.Purpose(toDispatch);
        }
    }
}
