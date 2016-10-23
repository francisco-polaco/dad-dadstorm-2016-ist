using System;
using System.Collections.Generic;
using System.Linq;
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
        

        private Log mLogger;
        private Queue<string> mLoggerBuffer = new Queue<string>();

        private Dictionary<uint, Operator> mOperators;
        private List<string> mUrlOfPcs = new List<string>();


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

        public PuppetMaster()
        {
            mOperators = new Dictionary<uint, Operator>();
            mUrlOfPcs.Add("tcp://1.2.3.4:10000/pcs");
            mUrlOfPcs.Add("tcp://1.2.3.5:10000/pcs");
            mUrlOfPcs.Add("tcp://1.2.3.6:10000/pcs");
            mUrlOfPcs.Add("tcp://1.2.3.8:10000/pcs");
            mUrlOfPcs.Add("tcp://1.2.3.9:10000/pcs");
            mUrlOfPcs.Add("tcp://1.2.3.10:10000/pcs");
        }

        public PuppetMaster(List<String> urlsOfPCS)
        {
            mOperators = new Dictionary<uint, Operator>();
            mUrlOfPcs = urlsOfPCS;
        }

        public void SetupFullSpeed(Form form, Delegate toUpdateUI, string configFilePath = @"config.config")
        {
            ConfigFileProcessor.GetInstance(this, configFilePath).ExecuteFullSpeed(form, toUpdateUI); 
        }

        public void SetupStepByStep(Form form, Delegate toUpdateUI, Delegate toDisableStepByStep,string configFilePath = @"config.config")
        {
            ConfigFileProcessor.GetInstance(this, configFilePath).ExecuteStepByStep(form, toUpdateUI, toDisableStepByStep);
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

        void init()
        {
            //parse
            //launch
        }

        internal void Exit()
        {
            if(mLogger != null) mLogger.Exit();
        }
    }
}
