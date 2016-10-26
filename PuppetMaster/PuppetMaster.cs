using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private static int PORT = 10001;

        private Log _logger;
        private Queue<string> _loggerBuffer = new Queue<string>();

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
            ChannelServices.RegisterChannel(new TcpChannel(PORT), false);
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

        public void SetupFullSpeed(Form form, Delegate toUpdateUI, string configFilePath = @"config.config")
        {
            ConfigFileProcessor.GetInstance(configFilePath).ExecuteFullSpeed(form, toUpdateUI); 
        }

        public void SetupStepByStep(Form form, Delegate toUpdateUI, Delegate toDisableStepByStep,string configFilePath = @"config.config")
        {
            ConfigFileProcessor.GetInstance(configFilePath).ExecuteStepByStep(form, toUpdateUI, toDisableStepByStep);
        }

        public void Log(string toLog)
        {
            if (_logger != null)
            {
                if (_loggerBuffer.Count != 0)
                {
                    foreach (string s in _loggerBuffer) _logger.Update(s);
                    _loggerBuffer.Clear();
                }
                _logger.Update(toLog);
            }
            else
            {
                lock (this)
                {
                    _loggerBuffer.Enqueue(toLog);
                }
            }
        }

        public void CreateOperator(int id, List<string> urls)
        {
            _operators.Add(id, new Operator(id, urls));
        }

        public void Start(int opid)
        {
            _operators[opid].Start();
        }

        public void Interval(int opid, int ms)
        {
            _operators[opid].Interval(ms);
        }

        public void Status()
        {
            foreach(KeyValuePair<int, Operator> entry in _operators)
            {
                entry.Value.Status();
            }
        }

        public void Crash(int opid, int replicaid)
        {
            _operators[opid].Crash(replicaid);
        }

        public void Freeze(int opid, int replicaid)
        {
            _operators[opid].Freeze(replicaid);
        }

        public void Unfreeze(int opid, int replicaid)
        {
            _operators[opid].Unfreeze(replicaid);
        }

        public void Wait(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        internal void Exit()
        {
            if (_logger != null) _logger.Exit();
            foreach(KeyValuePair<int, Operator> entry in _operators)
            {
                entry.Value.Exit();
            }
        }

    }


    public class PCSManager
    {

        private static PCSManager _instance = null;

        public static PCSManager GetInstance()
        {
            if (_instance == null) _instance = new PCSManager();
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
                //try
                //{
                //    IPCSSlaveLaunch remoteObj = (IPCSSlaveLaunch)Activator.GetObject(
                //        typeof(IPCSSlaveLaunch), pcsUrl);
                //    remoteObj.Launch(cp);
                //}catch(SocketException e)
                //{
                //    MessageBox.Show("Error connecting to PCS with URL: " + pcsUrl);
                //    PuppetMaster.GetInstance().Log("Error connecting to PCS with URL: " + pcsUrl);
                //}
            }

        }
    }
}
