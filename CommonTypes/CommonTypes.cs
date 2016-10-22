using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface ILogUpdate
    {
        void Update(string log);
    }

    public interface RemoteCmdInterface
    {
        void Start(int opid);
        void Interval(int opid, int ms);
        void Status();
        void Crash(string url);
        void Freeze(string url);
        void Unfreeze(string url);
    }

    public interface ISlave
    {
        void Dispatch(string a);
        void Update(string s);
    }

    public interface ISlaveLaunch
    {
        void Launch(string input);
    }

    public class Replica
    {
        private int opID;
        private string url;

        public int OpID
        {
            get { return opID; }
            set { opID = value; }
        }

        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        public Replica(int opID, string url)
        {
            this.opID = opID;
            this.url = url;
        }
    }

}
