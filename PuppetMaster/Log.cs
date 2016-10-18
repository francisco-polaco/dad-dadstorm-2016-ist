using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public abstract class Log
    {
        protected string mLogFilePath;

        public Log(string logFilePath = @"log.log")
        {
            mLogFilePath = logFilePath;
        }
        public abstract void AppendToLog(string toLog);
    }


    public class LightLog : Log
    {
        public override void AppendToLog(string toLog)
        {
            throw new NotImplementedException();
        }
    }

    public class FullLog : Log
    {
        public override void AppendToLog(string toLog)
        {
            throw new NotImplementedException();
        }
    }
}
