using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace PuppetMaster
{
    public class Log : MarshalByRefObject , CommonTypes.ILogUpdate
    {
        private string _logFilePath;
        private Queue<string> _toLogQueue = new Queue<string>();
        private static volatile bool _isThreadToExit = false;
        private Thread _writingThread;
        private Form _form;
        private Delegate _updateForm;

        public Log() {}
        public Log(Form form, Delegate updateForm, string logFilePath = @"log.log")
        {
            _form = form;
            _updateForm = updateForm;
            _logFilePath = logFilePath;
            System.IO.File.WriteAllText(logFilePath, string.Empty);
            _writingThread = new Thread(() =>
                {
                    while (_isThreadToExit == false)
                    {
                        string s = "";

                        lock (this) { 
                            if (_toLogQueue.Count == 0) Monitor.Wait(this);
                            else s = _toLogQueue.Dequeue();
                        }
                        AppendToLog(s);
                    }
                });
            _writingThread.Start();
        }
        private void AppendToLog(string toLog)
        {
            try {
                lock (this)
                {
                    using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(_logFilePath, true))
                    {
                        file.WriteLine(toLog);
                    }
                }
            }
            catch (IOException e)
            {
                MessageBox.Show("Impossible to write Log to file.");
            }
            _form.Invoke(_updateForm, new object[] { toLog });
        }

        public void Update(string log)
        {
            PutInQueue(log);
        }

        public void Exit()
        {
            _writingThread.Abort();
        }

        private void PutInQueue(string toLog)
        {
            string log = "[" + DateTime.Now.ToString("HH:mm") + "] - " + toLog;
            lock (this)
            {
                _toLogQueue.Enqueue(log);
                Monitor.Pulse(this);
            }
        }

        public void ReplicaUpdate(string replicaUrl, IList<string> tupleFields)
        {
            string res = "";
            foreach (string field in tupleFields)
            {
                res += field + ", ";
            }
            Update("tuple " + replicaUrl + " " + res.Substring(0, res.Length - 2));
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }
    }
}
