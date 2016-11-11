using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppetMaster
{
    public class PuppetMaster
    {
        public const int Port = 10001;
        public const string RemoteName = "log";

        private Log _logger;
        private bool _loggerOnline = false;
        // private Queue<string> _loggerBuffer = new Queue<string>();

        private Dictionary<int, Operator> _operators;

        private static PuppetMaster _instance = null;

        public static PuppetMaster GetInstance()
        {
            if (_instance == null) _instance = new PuppetMaster();
            return _instance;
        }

        private PuppetMaster()
        {
            _operators = new Dictionary<int, Operator>();
            ChannelServices.RegisterChannel(new TcpChannel(Port), false);
        }

        public Log Logger
        {
            get
            {
                return _logger;
            }

            set
            {
                _logger = value;
            }
        }

        public void SetupFullSpeed(Form form, Delegate toUpdateUi, Delegate toPreparePBar,
            Delegate toIncrementProgBar, string configFilePath = @"config.config")
        {
            if (!_loggerOnline)
            {
                _logger = new Log(form, toUpdateUi);
                RemotingServices.Marshal(_logger, RemoteName, typeof(ILogUpdate));
                _loggerOnline = true;
            }
            ConfigFileProcessor.GetInstance(configFilePath)
                .ExecuteFullSpeed(form, toUpdateUi, toPreparePBar, toIncrementProgBar);

        }

        public void SetupStepByStep(Form form, Delegate toUpdateUi, Delegate toDisableStepByStep,
            Delegate toPreparePBar, Delegate toIncrementProgBar, string configFilePath = @"config.config")
        {
            if (!_loggerOnline)
            {
                _logger = new Log(form, toUpdateUi);
                RemotingServices.Marshal(_logger, "Log", typeof(ILogUpdate));
                _loggerOnline = true;
            }
            ConfigFileProcessor.GetInstance(configFilePath)
                .ExecuteStepByStep(form, toUpdateUi, toDisableStepByStep, toPreparePBar, toIncrementProgBar);
        }

        public void Log(string toLog)
        {
            _logger.Update(toLog);
            //if (_logger != null)
            //{
            //    lock (this)
            //    {
            //        if (_loggerBuffer.Count != 0){
            //            foreach (string s in _loggerBuffer) _logger.Update(s);
            //            _loggerBuffer.Clear();
            //        }
            //    }
            //    _logger.Update(toLog);
            //}
            //else
            //{
            //    lock (this)
            //    {
            //        _loggerBuffer.Enqueue(toLog);
            //    }
            //}
        }

        public void CreateOperator(int id, List<string> urls)
        {
            _operators.Add(id, new Operator(id, urls));
        }

        public void RunCommand(string cmd)
        {
            if (cmd.StartsWith("Wait"))
            {
                Log("You cannot run: " + cmd);
            }
            else
            {
                Log("Executing user's command: " + cmd);
                ConfigFileProcessor.GetInstance(null).ParseCommand(cmd);
            }
        }

        public void Start(int opid)
        {
            try
            {
                _operators[opid].Start();
            }
            catch (SocketException e)
            {
                Log(e.Message);
            }
        }

        public void Interval(int opid, int ms)
        {
            try
            {
                _operators[opid].Interval(ms);
            }
            catch (SocketException e)
            {
                Log(e.Message);
            }
        }


        public void Status()
        {
            foreach (KeyValuePair<int, Operator> entry in _operators)
            {
                try { 
                    entry.Value.Status();
                }
                catch (SocketException e)
                {
                    Log(e.Message);
                }
            }
        }

        public void Crash(int opid, int replicaid)
        {
            try {   _operators[opid].Crash(replicaid);}
            catch (SocketException e)
            {
                Log(e.Message);
            }
        }

        public void Freeze(int opid, int replicaid)
        {
            try { _operators[opid].Freeze(replicaid);}
            catch (SocketException e)
            {
                Log(e.Message);
            }
        }

        public void Unfreeze(int opid, int replicaid)
        {
            try { _operators[opid].Unfreeze(replicaid);}
            catch (SocketException e)
            {
                Log(e.Message);
            }
        }

        public void Wait(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        internal void Exit()
        {
            if (_logger != null) _logger.Exit();
            foreach (KeyValuePair<int, Operator> entry in _operators)
            {
                try { entry.Value.Exit();}
                catch (SocketException e)
                {
                    Log(e.Message);
                }
            }
        }

    }


    public class PcsManager
    {

        private static PcsManager _instance = null;

        public static PcsManager GetInstance()
        {
            if (_instance == null) _instance = new PcsManager();
            return _instance;
        }

        public void SendCommand(ConnectionPack cp)
        {
            //PuppetMaster.GetInstance().Log("============== Connection Pack ============");
            //PuppetMaster.GetInstance().Log(cp.ToString());
            //PuppetMaster.GetInstance().Log("===========================================");
            HashSet<string> set = new HashSet<string>();
            foreach (string url in cp.ListUrls)
            {
                int portIndex = url.IndexOf(':', 4); // skip the first : from the tcp protocol
                string pcsUrl = url.Substring(0, portIndex + 1) + "10000/pcs";
                set.Add(pcsUrl);
            }

            foreach (string pcsUrl in set)
            {
                PuppetMaster.GetInstance().Log("PCS URL: " + pcsUrl);
                // Remove comments after pcs implementation
                try
                {
                    IPcsSlaveLaunch remoteObj = (IPcsSlaveLaunch)Activator.GetObject(
                        typeof(IPcsSlaveLaunch), pcsUrl);
                    remoteObj.Launch(cp);
                }
                catch (SocketException e)
                {
                    MessageBox.Show("Error connecting to PCS with URL: " + pcsUrl);
                    PuppetMaster.GetInstance().Log("Error connecting to PCS with URL: " + pcsUrl);
                }
            }

        }
    }
}
