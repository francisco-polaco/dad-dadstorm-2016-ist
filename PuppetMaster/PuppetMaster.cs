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
    public class PuppetMaster : RemoteCmdInterface
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

        internal void Exit()
        {
            if(mLogger != null) mLogger.Exit();
        }

        public void Start(int opid)
        {
        }

        public void Interval(int opid, int ms)
        {
        }

        public void Status()
        {
        }

        public void Crash(string url)
        {
        }

        public void Freeze(string url)
        {
        }

        public void Unfreeze(string url)
        {
        }

        public void Wait(int ms)
        {
            System.Threading.Thread.Sleep(ms);
        }
    }


    public class PCSManager
    {
        //private List<string> mUrlOfPcs = new List<string>();

        private static PCSManager mInstance = null;

        public static PCSManager GetInstance()
        {
            if (mInstance == null) mInstance = new PCSManager();
            return mInstance;
        }

        //private PCSManager()
        //{
        //    //mUrlOfPcs.Add("tcp://1.2.3.4:10000/pcs");
        //    //mUrlOfPcs.Add("tcp://1.2.3.5:10000/pcs");
        //    //mUrlOfPcs.Add("tcp://1.2.3.6:10000/pcs");
        //    //mUrlOfPcs.Add("tcp://1.2.3.8:10000/pcs");
        //    //mUrlOfPcs.Add("tcp://1.2.3.9:10000/pcs");
        //    //mUrlOfPcs.Add("tcp://1.2.3.10:10000/pcs");
        //}

        public void SendCommand(string command, List<string> urls)
        {
            foreach(string s in urls)
            {
                int portIndex = s.IndexOf(':', 4); // skip the first : from the tcp protocol
                string pcsUrl = s.Substring(0, portIndex + 1) + "10000/pcs";
                PuppetMaster.GetInstance().Log("PCS URL: " + pcsUrl);
                // Remove comments after pcs implementation
                //try
                //{
                //    IPCSSlaveLaunch remoteObj = (IPCSSlaveLaunch)Activator.GetObject(
                //        typeof(IPCSSlaveLaunch), pcsUrl);
                //    remoteObj.Launch(command);
                //}catch(SocketException e)
                //{
                //    MessageBox.Show("Error connecting to PCS with URL: " + pcsUrl);
                //    PuppetMaster.GetInstance().Log("Error connecting to PCS with URL: " + pcsUrl);
                //}
            }
        }
    }
}
