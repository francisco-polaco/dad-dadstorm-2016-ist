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

        public abstract void Update(string log);

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

        protected void putInQueue(string toLog)
        {
            string log = "[" + DateTime.Now.ToString("HH:mm") + "] - " + toLog;
            lock (this)
            {
                mToLogQueue.Enqueue(log);
                Monitor.Pulse(this);
            }
        }
    }


    public class LightLog : Log
    {
        public LightLog(Form form, Delegate updateForm, string logFilePath = @"log.log"):base(form, updateForm, logFilePath)
        {
        }

        public override void Update(string log)
        {
            //filter log
            putInQueue(log);
        }
    }

    public class FullLog : Log
    {
        public FullLog(Form form, Delegate updateForm, string logFilePath = @"log.log"):base(form, updateForm, logFilePath)
        {
        }

        public override void Update(string log)
        {
            putInQueue(log);
        }
    }
}
