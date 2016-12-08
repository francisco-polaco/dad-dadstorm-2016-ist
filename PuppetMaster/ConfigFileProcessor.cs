using CommonTypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace PuppetMaster
{
    class ConfigFileProcessor
    {

        private string _configFilePath;
        private List<string> _fileLines;
        private int _lineIndex = 0;
        private volatile uint _isItRunning = 0; // 1 full speed , 2 step by step
        private volatile bool _wereOperatorsCreated = false;
        private bool _isLogFull = false;
        private string _sematic = "";

        private Dictionary<int, ConnectionPack> _whatShouldISentToOperators 
            = new Dictionary<int, ConnectionPack>();

        private static ConfigFileProcessor _instance = null;

        public static ConfigFileProcessor GetInstance(string filepath)
        {
            if(_instance == null)
            {
                _instance = new ConfigFileProcessor(filepath);
            }
            return _instance;
        }

        private ConfigFileProcessor(string filepath = @"config.config")
        {
            _configFilePath = filepath;
            ProcessFile();
        }

        private void ProcessFile()
        {
            StreamReader file =
                new StreamReader(_configFilePath);
            string line;
            uint lineNo = 0;
            _fileLines = new List<string>();
            while ((line = file.ReadLine()) != null)
            {
                lineNo++;
                if (line.StartsWith("%%") || line.StartsWith("%") ||
                    line.StartsWith("\n") || line.StartsWith("\r\n") || 
                    line.Equals(""))
                    continue; // comments or empty lines
                if (line.StartsWith(" ") || line.StartsWith("\t")) // case where string has spaces in the start
                {
                    MessageBox.Show("The file has a line that starts with a space. It will cause problems. Line " + lineNo);
                }
                _fileLines.Add(line);
            }

            file.Close();
        }

        public void ExecuteFullSpeed(Form form, Delegate updateUi, Delegate toPreparePBar, Delegate toIncrementProgBar)
        {
            lock (this)
            {
                if (_isItRunning == 0) _isItRunning = 1;
                else if(_isItRunning != 1) throw new InvalidOperationException();
            }
            PuppetMaster.GetInstance().Log("===== Configuration file loading started =====");
            form.Invoke(toPreparePBar, _fileLines.Count);
            foreach (string cmd in _fileLines)
            {
                ExecuteLine(cmd);
                _lineIndex++;
                // because when you exit you crash everything
                try
                {
                    form.Invoke(toIncrementProgBar, _lineIndex);
                }
                catch(Exception e) { }

            }
            PuppetMaster.GetInstance().Log("===== Configuration file loading complete =====");

        }

        public void ExecuteStepByStep(Form form, Delegate updateUi, Delegate toDisableStepByStep, Delegate toPreparePBar, Delegate toIncrementProgBar)
        {
            lock (this)
            {
                if (_isItRunning == 0) _isItRunning = 2;
                else if (_isItRunning != 2) throw new InvalidOperationException();
            }

            if (_lineIndex == 0)
            {
                form.Invoke(toPreparePBar, _fileLines.Count);
                PuppetMaster.GetInstance().Log("===== Configuration file loading started =====");
            }
            if (_fileLines.Count != 0 && _lineIndex < _fileLines.Count)
            {
                if(_fileLines.ElementAt(_lineIndex).StartsWith("Wait"))
                    PuppetMaster.GetInstance().Log("Wait command is ignored in Step by step mode.");
                ExecuteLine(_fileLines.ElementAt(_lineIndex++));
            }
            form.Invoke(toIncrementProgBar, _lineIndex);

            if (_lineIndex >= _fileLines.Count)
            {
                PuppetMaster.GetInstance().Log("===== Configuration file loading complete =====");
                form.Invoke(toDisableStepByStep);
            }
        }

        private void ExecuteLine(string cmd)
        {
            PuppetMaster.GetInstance().Log("Running config file command: " + cmd);
            if (cmd.StartsWith(" "))
            {
                MessageBox.Show("Invalid line to be processed!");
            }
            else if (cmd.StartsWith("Semantics"))
            {
                string[] pieces = cmd.Split(' ');
                if (pieces.Length == 2)
                {
                    _sematic = pieces[1];
                }
            }
            else if (cmd.StartsWith("LoggingLevel"))
            {
                string[] pieces = cmd.Split(' ');
                if (pieces.Length == 2 && pieces[1].Equals("full"))
                {
                    _isLogFull = true;
                }
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

                string ip = GetOwnIp();

                string myLogUrl = "tcp://" + ip + ":" + PuppetMaster.Port + "/" + PuppetMaster.RemoteName;
                PuppetMaster.GetInstance().Log("IP Sent for PCS: " + myLogUrl);
                if (_sematic.Equals(String.Empty)) _sematic = "at-most-once";
                ConnectionPack thingsToSend = new ConnectionPack(cmd, _isLogFull, listUrls, myLogUrl, _sematic);
                _whatShouldISentToOperators.Add(opId, thingsToSend);

                if (res[3].StartsWith("OP") && !res[3].EndsWith(".dat"))
                {
                    // Put in connection pack of the previous operator the urls of this one. So he can direct its output
                    int op2Id = int.Parse(res[3].Substring(2));
                    _whatShouldISentToOperators[op2Id].ReplicaUrlsOutput = listUrls;
                    _whatShouldISentToOperators[op2Id].RoutingType = res[8];
                }
                if (res[0].Equals("OP1"))
                {
                    _whatShouldISentToOperators[opId].RoutingTypeToReadFromFile = res[8];
                }

                PuppetMaster.GetInstance().CreateOperator(opId, listUrls);
            }
            else
            {
                // Commands to operators
                // Send every creation command to the operators
                if (!_wereOperatorsCreated)
                {
                    PuppetMaster.GetInstance().Log("Creating operators...");
                    foreach (KeyValuePair<int, ConnectionPack> entry in _whatShouldISentToOperators)
                        PcsManager.GetInstance().SendCommand(entry.Value);
                    _wereOperatorsCreated = true;
                }
                ParseCommand(cmd);
            }
            
        }

        private string GetOwnIp()
        {
            IPAddress ipAddr =
                Dns.GetHostEntry(Environment.MachineName)
                    .AddressList.FirstOrDefault(i => i.AddressFamily == AddressFamily.InterNetwork);
            string ip = "";
            try
            {
                using (StreamReader file =
                    new StreamReader(Environment.CurrentDirectory + @"\..\..\..\Inputs\" + "ip_tba_ppm.txt", true))
                {
                    string line = file.ReadLine();
                    if (line != null) ip = line;
                }
            }
            catch (FileNotFoundException e)
            {
                if (ipAddr != null) ip = ipAddr.ToString();
            }
            return ip;
        }

        public void ParseCommand(string cmd)
        {
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
