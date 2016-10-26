using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace PuppetMaster
{
    public abstract class Log : MarshalByRefObject , CommonTypes.ILogUpdate
    {
        protected string mLogFilePath;
        protected Queue<string> mToLogQueue = new Queue<string>();
        protected volatile static bool mIsThreadToExit = false;
        private Thread mWritingThread;

        private Form mForm;
        private Delegate mUpdateForm;

        public Log() {}

        public Log(Form form, Delegate updateForm, string logFilePath = @"log.log")
        {
            mForm = form;
            mUpdateForm = updateForm;
            mLogFilePath = logFilePath;
            mWritingThread = new Thread(() =>
                {
                    while (mIsThreadToExit == false)
                    {
                        string s = "";

                        lock (this) { 
                            if (mToLogQueue.Count == 0) Monitor.Wait(this);
                            else s = mToLogQueue.Dequeue();
                        }
                        AppendToLog(s);
                    }
                });
            mWritingThread.Start();
        }
        private void AppendToLog(string toLog)
        {
            using (System.IO.StreamWriter file =
            new System.IO.StreamWriter(mLogFilePath, true))
            {
                file.WriteLine(toLog);
            }
            mForm.Invoke(mUpdateForm, new object[] { toLog });
        }

        public void Update(string log)
        {
            PutInQueue(log);
        }

        public void Exit()
        {
            //mIsThreadToExit = true;
            //lock (this)
            //{
            //    Monitor.PulseAll(this);
            //}
            //// This need to be fixed
            // mWritingThread.Join(); 
            mWritingThread.Abort();
        }

        private void PutInQueue(string toLog)
        {
            string log = "[" + DateTime.Now.ToString("HH:mm") + "] - " + toLog;
            lock (this)
            {
                mToLogQueue.Enqueue(log);
                Monitor.Pulse(this);
            }
        }

        public void ReplicaUpdate(string replicaUrl, List<string> tupleFields)
        {
            string res = "";
            foreach (string field in tupleFields)
            {
                res += field + ", ";
            }
            Update("tuple " + replicaUrl + " " + res.Substring(0, res.Length - 2));
        }
    }


    public class LightLog : Log
    {
        public LightLog(Form form, Delegate updateForm, string logFilePath = @"log.log"):base(form, updateForm, logFilePath)
        {
        }

   
    }

    public class FullLog : Log
    {
        public FullLog(Form form, Delegate updateForm, string logFilePath = @"log.log"):base(form, updateForm, logFilePath)
        {
        }

    }
}
