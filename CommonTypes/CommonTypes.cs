using System.Collections.Generic;

namespace CommonTypes
{
    public interface ILogUpdate
    {
        //void Update(string log);
        void ReplicaUpdate(string replicaUrl, List<string> tupleFields);
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

    public interface IPcsSlaveLaunch
    {
        void Launch(ConnectionPack input);
    }

    public class Replica
    {
        private int _opId;
        private string _url;
        private ISlave _proxy;

        public int OpID
        {
            get { return _opId; }
            set { _opId = value; }
        }

        public string Url
        {
            get { return _url; }
            set { _url = value; }
        }

        public ISlave Proxy
        {
            get { return _proxy; }
            set { _proxy = value; }
        }

        public Replica(int opID, string url, ISlave proxy)
        {
            this._opId = opID;
            this._url = url;
            this._proxy = proxy;
        }

        public Replica()
        {
        }
    }


    [System.Serializable]
    public class ConnectionPack
    {
        private string _cmd;
        private List<string> _listUrls;
        private List<string> _replicaUrlsOutput;

        public string Cmd
        {
            get
            {
                return _cmd;
            }

            set
            {
                _cmd = value;
            }
        }

        public List<string> ReplicaUrlsOutput
        {
            get
            {
                return _replicaUrlsOutput;
            }

            set
            {
                _replicaUrlsOutput = value;
            }
        }

        public List<string> ListUrls
        {
            get
            {
                return _listUrls;
            }

            set
            {
                _listUrls = value;
            }
        }

        public ConnectionPack(string cmd, List<string> listUrls)
        {
            _cmd = cmd;
            _listUrls = listUrls;
            _replicaUrlsOutput = new List<string>();
        }

        public override string ToString()
        {
            string res;
            res = "Command: " + _cmd;
            foreach(string str in _listUrls)
            {
                res += "\r\nReplica URL: " + str;
            }
            foreach (string str in _replicaUrlsOutput)
            {
                res += "\r\nReplica Output URL: " + str;
            }
            return res;
        }


    }
}
