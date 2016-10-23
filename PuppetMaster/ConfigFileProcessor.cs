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
    class ConfigFileProcessor
    {
        private static int port = 10001;

        private PuppetMaster mPm;
        private string mConfigFilePath;
        private List<string> mFileLines;
        private int mLineIndex = 0;
        private volatile uint mIsItRunning = 0; // 1 full speed , 2 step by step

        private static ConfigFileProcessor mInstance = null;

        public static ConfigFileProcessor GetInstance(PuppetMaster pm, string filepath)
        {
            if(mInstance == null)
            {
                mInstance = new ConfigFileProcessor(pm, filepath);
            }
            return mInstance;
        }

        private ConfigFileProcessor(PuppetMaster pm, string filepath = @"config.config")
        {
            mConfigFilePath = filepath;
            mPm = pm;
            ProcessFile();
        }

        private void ProcessFile()
        {
            System.IO.StreamReader file =
                new System.IO.StreamReader(@mConfigFilePath);
            string line;
            mFileLines = new List<string>();
            while ((line = file.ReadLine()) != null)
            {
                if (line.StartsWith("%%") || line.StartsWith("%") ||
                    line.StartsWith("\n") || line.StartsWith("\r\n") || 
                    line.Equals(""))
                    continue; // comments or empty lines
                mFileLines.Add(line);
            }

            file.Close();
        }

        public void ExecuteFullSpeed(Form form, Delegate updateUI)
        {
            lock (this)
            {
                if (mIsItRunning == 0) mIsItRunning = 1;
                else if(mIsItRunning != 1) throw new InvalidOperationException();
            }
            mPm.Log("===== Configuration file loading started =====");
            foreach (string cmd in mFileLines)
            {
                ExecuteLine(cmd, form, updateUI);
            }
            mPm.Log("===== Configuration file loading complete =====");

        }

        public void ExecuteStepByStep(Form form, Delegate updateUI, Delegate toDisableStepByStep)
        {
            lock (this)
            {
                if (mIsItRunning == 0) mIsItRunning = 2;
                else if (mIsItRunning != 2) throw new InvalidOperationException();
            }
            mPm.Log("===== Configuration file loading started =====");
            if (mFileLines.Count != 0 && mLineIndex < mFileLines.Count)
                ExecuteLine(mFileLines.ElementAt(mLineIndex++), form, updateUI);
            if (mLineIndex >= mFileLines.Count)
            {
                mPm.Log("===== Configuration file loading complete =====");
                form.Invoke(toDisableStepByStep);
            }
        }

        private void ExecuteLine(string cmd, Form form, Delegate updateUI)
        {
            if (cmd.StartsWith("Semantics"))
            {
                // Next time
            }
            else if (cmd.StartsWith("LoggingLevel"))
            {
                string[] pieces = cmd.Split(' ');
                if (pieces.Length == 2 && pieces[1].Equals("light"))
                    mPm.Logger = new LightLog(form, updateUI);
                else mPm.Logger = new FullLog(form, updateUI);

                ChannelServices.RegisterChannel(new TcpChannel(port), false);
                RemotingServices.Marshal(mPm.Logger, "Log",
                    typeof(Log));
            }
            else if (cmd.StartsWith("OP"))
            {
                // Operators setup
                string[] res = cmd.Split(' ');

                int opId = int.Parse(res[0].Substring(2));
                List<string> listUrls = new List<string>();
                for(int i = 10; i < res.Length && res[i].StartsWith("tcp://"); i++)
                {
                    listUrls.Add(res[i].Replace(",", string.Empty));
                }
                mPm.CreateOperator(opId, listUrls);

            }
            else
            {
                // Commands to operators
            }
            mPm.Log("Config file command ran: " + cmd);
        }

    }

}
