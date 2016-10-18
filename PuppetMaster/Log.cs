using System;
using System.Collections.Concurrent;
using System.Threading;

namespace PuppetMaster
{
    public abstract class Log : CommonTypes.LogUpdate
    {
        protected string mLogFilePath;
        protected ConcurrentQueue<string> mToLogQueue = new ConcurrentQueue<string>();
        protected static bool mIsThreadToExit = false;
        private Thread mWritingThread;

        public Log(string logFilePath = @"log.log")
        {
            mLogFilePath = logFilePath;
            mWritingThread = new Thread(() =>
                {
                    while (mIsThreadToExit == false)
                    {
                        string s = "";
                        bool wasSucessful = false;
                        do
                        {
                            lock (this) { 
                                wasSucessful = mToLogQueue.TryDequeue(out s);
                                if (wasSucessful == false) Monitor.Wait(this);
                            }
                        } while (wasSucessful == false);
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
        }

        public abstract void Update(string log);

        public void Exit()
        {
            mIsThreadToExit = true;
            mWritingThread.Join();
        }
    }


    public class LightLog : Log
    {
        public override void Update(string log)
        {
            //filter log
            lock (this)
            {
                mToLogQueue.Enqueue(log);
                Monitor.Pulse(this);
            }
        }
    }

    public class FullLog : Log
    {
        public override void Update(string log)
        {
            lock (this)
            {
                mToLogQueue.Enqueue(log);
                Monitor.Pulse(this);
            }
        }
    }
}
