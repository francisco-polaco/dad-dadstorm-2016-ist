using CommonTypes;
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

        private string mConfigFilePath;
        private List<string> mFileLines;
        private int mLineIndex = 0;
        private volatile uint mIsItRunning = 0; // 1 full speed , 2 step by step
        private volatile bool mWereOperatorsCreated = false;

        private Dictionary<int, ConnectionPack> mWhatShouldISentToOperators 
            = new Dictionary<int, ConnectionPack>();

        private static ConfigFileProcessor mInstance = null;

        public static ConfigFileProcessor GetInstance(string filepath)
        {
            if(mInstance == null)
            {
                mInstance = new ConfigFileProcessor(filepath);
            }
            return mInstance;
        }

        private ConfigFileProcessor(string filepath = @"config.config")
        {
            mConfigFilePath = filepath;
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
            PuppetMaster.GetInstance().Log("===== Configuration file loading started =====");
            foreach (string cmd in mFileLines)
            {
                ExecuteLine(cmd, form, updateUI);
            }
            PuppetMaster.GetInstance().Log("===== Configuration file loading complete =====");

        }

        public void ExecuteStepByStep(Form form, Delegate updateUI, Delegate toDisableStepByStep)
        {
            lock (this)
            {
                if (mIsItRunning == 0) mIsItRunning = 2;
                else if (mIsItRunning != 2) throw new InvalidOperationException();
            }

            if(mLineIndex == 0)
                PuppetMaster.GetInstance().Log("===== Configuration file loading started =====");

            if (mFileLines.Count != 0 && mLineIndex < mFileLines.Count)
                ExecuteLine(mFileLines.ElementAt(mLineIndex++), form, updateUI);

            if (mLineIndex >= mFileLines.Count)
            {
                PuppetMaster.GetInstance().Log("===== Configuration file loading complete =====");
                form.Invoke(toDisableStepByStep);
            }
        }

        private void ExecuteLine(string cmd, Form form, Delegate updateUI)
        {
            PuppetMaster.GetInstance().Log("Running config file command: " + cmd);
            if (cmd.StartsWith("Semantics"))
            {
                // Next time
            }
            else if (cmd.StartsWith("LoggingLevel"))
            {
                string[] pieces = cmd.Split(' ');
                if (pieces.Length == 2 && pieces[1].Equals("light"))
                    PuppetMaster.GetInstance().Logger = new LightLog(form, updateUI);
                else PuppetMaster.GetInstance().Logger = new FullLog(form, updateUI);

                
                RemotingServices.Marshal(PuppetMaster.GetInstance().Logger, "Log",
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

                ConnectionPack thingsToSend = new ConnectionPack(cmd, listUrls);
                mWhatShouldISentToOperators.Add(opId, thingsToSend);

                if (res[3].StartsWith("OP") && !res[3].EndsWith(".data"))
                {
                    // Put in connection pack of the previous operator the urls of this one. So he can direct its output
                    int op2Id = int.Parse(res[3].Substring(2));
                    mWhatShouldISentToOperators[op2Id].ReplicaUrlsInput = listUrls;
                }

                PuppetMaster.GetInstance().CreateOperator(opId, listUrls);

            }
            else
            {
                // Commands to operators
                // Send every creation command to the operators
                if (!mWereOperatorsCreated)
                {
                    foreach(KeyValuePair<int, ConnectionPack> entry in mWhatShouldISentToOperators)
                        PCSManager.GetInstance().SendCommand(entry.Value);
                    mWereOperatorsCreated = true;
                }

                if (cmd.StartsWith("Interval"))
                {
                    string[] res = cmd.Split(' ');
                    if (res.Length == 3)
                    {
                        PuppetMaster.GetInstance()
                            .Interval(int.Parse(res[1].Substring(2)), int.Parse(res[2]));
                    }
                }
                else if (cmd.StartsWith("Start"))
                {
                    string[] res = cmd.Split(' ');
                    if (res.Length == 2)
                    {
                        PuppetMaster.GetInstance()
                            .Start(int.Parse(res[1].Substring(2)));
                    }
                }
                else if (cmd.StartsWith("Status"))
                {
                    PuppetMaster.GetInstance().Status();
                }
                else if (cmd.StartsWith("Wait"))
                {
                    string[] res = cmd.Split(' ');
                    if (res.Length == 2)
                    {
                        PuppetMaster.GetInstance()
                            .Wait(int.Parse(res[1]));
                    }
                }
                else if (cmd.StartsWith("Freeze"))
                {
                    string[] res = cmd.Split(' ');
                    if (res.Length == 3)
                    {
                        PuppetMaster.GetInstance()
                            .Freeze(int.Parse(res[1].Substring(2)), int.Parse(res[2]));
                    }
                }
                else if (cmd.StartsWith("Unfreeze"))
                {
                    string[] res = cmd.Split(' ');
                    if (res.Length == 3)
                    {
                        PuppetMaster.GetInstance()
                            .Unfreeze(int.Parse(res[1].Substring(2)), int.Parse(res[2]));
                    }
                }
                else if (cmd.StartsWith("Crash"))
                {
                    string[] res = cmd.Split(' ');
                    if (res.Length == 3)
                    {
                        PuppetMaster.GetInstance()
                            .Crash(int.Parse(res[1].Substring(2)), int.Parse(res[2]));
                    }
                }
                else
                {
                    PuppetMaster.GetInstance().Log("Invalid command.");
                }

            }
            
        }

    }

}
