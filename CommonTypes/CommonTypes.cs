using System.Collections.Generic;

namespace CommonTypes
{
    public interface ILogUpdate
    {
        void Update(string log);
    }

    public interface IRemoteCmdInterface
    {
        void Start();
        void Interval(int ms);
        void Status();
        void Crash();
        void Freeze();
        void Unfreeze();
        void Exit();
    }

    public interface ISlave
    {
        void Dispatch(string a);
    }

    public interface IPCSSlaveLaunch
    {
        void Launch(ConnectionPack input);
    }

    public class Replica
    {
        private int opID;
        private string url;
        private ISlave proxy;

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

        public ISlave Proxy
        {
            get { return proxy; }
            set { proxy = value; }
        }

        public Replica(int opID, string url, ISlave proxy)
        {
            this.opID = opID;
            this.url = url;
            this.proxy = proxy;
        }

        public Replica()
        {
        }
    }


    [System.Serializable]
    public class ConnectionPack
    {
        private string mCmd;
        private List<string> mListUrls;
        private List<string> mReplicaUrlsOutput;

        public string Cmd
        {
            get
            {
                return mCmd;
            }

            set
            {
                mCmd = value;
            }
        }

        public List<string> ReplicaUrlsOutput
        {
            get
            {
                return mReplicaUrlsOutput;
            }

            set
            {
                mReplicaUrlsOutput = value;
            }
        }

        public List<string> ListUrls
        {
            get
            {
                return mListUrls;
            }

            set
            {
                mListUrls = value;
            }
        }

        public ConnectionPack(string cmd, List<string> listUrls)
        {
            mCmd = cmd;
            mListUrls = listUrls;
            mReplicaUrlsOutput = new List<string>();
        }

        public override string ToString()
        {
            string res;
            res = "Command: " + mCmd;
            foreach(string str in mListUrls)
            {
                res += "\r\nReplica URL: " + str;
            }
            foreach (string str in mReplicaUrlsOutput)
            {
                res += "\r\nReplica Output URL: " + str;
            }
            return res;
        }


    }
}
