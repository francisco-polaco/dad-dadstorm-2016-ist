using System.Collections.Generic;

namespace CommonTypes
{
    public interface ILogUpdate
    {
        void Update(string log);
    }

    public interface RemoteCmdInterface
    {
        void Start();
        void Interval(int ms);
        void Status();
        void Crash();
        void Freeze();
        void Unfreeze();
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
        private List<string> mReplicaUrlsInput;

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

        public List<string> ReplicaUrlsInput
        {
            get
            {
                return mReplicaUrlsInput;
            }

            set
            {
                mReplicaUrlsInput = value;
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
        }

      
    }
}
