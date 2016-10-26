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
        private static int port = 10001;

        private Log mLogger;
        private Queue<string> mLoggerBuffer = new Queue<string>();

        private Dictionary<int, Operator> mOperators;

        private static PuppetMaster mInstance = null;

        public static PuppetMaster GetInstance()
        {
            if (mInstance == null) mInstance = new PuppetMaster();
            return mInstance;
        }

        private PuppetMaster()
        {
            mOperators = new Dictionary<int, Operator>();
            ChannelServices.RegisterChannel(new TcpChannel(port), false);
        }

        public Log Logger
        {
            get
            {
                return mLogger;
            }

            set
            {
                mLogger = value;
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
            if (mLogger != null)
            {
                if (mLoggerBuffer.Count != 0)
                {
                    foreach (string s in mLoggerBuffer) mLogger.Update(s);
                    mLoggerBuffer.Clear();
                }
                mLogger.Update(toLog);
            }
            else
            {
                lock (this)
                {
                    mLoggerBuffer.Enqueue(toLog);
                }
            }
        }

        public void CreateOperator(int id, List<string> urls)
        {
            mOperators.Add(id, new Operator(id, urls));
        }

        public void Start(int opid)
        {
            mOperators[opid].Start();
        }

        public void Interval(int opid, int ms)
        {
            mOperators[opid].Interval(ms);
        }

        public void Status()
        {
            foreach(KeyValuePair<int, Operator> entry in mOperators)
            {
                entry.Value.Status();
            }
        }

        public void Crash(int opid, int replicaid)
        {
            mOperators[opid].Crash(replicaid);
        }

        public void Freeze(int opid, int replicaid)
        {
            mOperators[opid].Freeze(replicaid);
        }

        public void Unfreeze(int opid, int replicaid)
        {
            mOperators[opid].Unfreeze(replicaid);
        }

        public void Wait(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }

        internal void Exit()
        {
            if (mLogger != null) mLogger.Exit();
            foreach(KeyValuePair<int, Operator> entry in mOperators)
            {
                entry.Value.Exit();
            }
        }

    }


    public class PCSManager
    {

        private static PCSManager mInstance = null;

        public static PCSManager GetInstance()
        {
            if (mInstance == null) mInstance = new PCSManager();
            return mInstance;
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
