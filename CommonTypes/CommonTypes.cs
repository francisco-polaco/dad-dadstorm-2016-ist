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

    [System.Serializable]
    public class ConnectionPack
    {
        private string _cmd;
        private bool _isLogFull;
        private List<string> _listUrls;
        private List<string> _replicaUrlsOutput;
        private string _puppetMasterUrl;
        private string _routingType;
        private string _routingTypeToReadFromFile;

        public ConnectionPack(string cmd, bool isLogFull, List<string> listUrls, string puppetMasterUrl)
        {
            _cmd = cmd;
            _isLogFull = isLogFull;
            _listUrls = listUrls;
            _puppetMasterUrl = puppetMasterUrl;
            _replicaUrlsOutput = new List<string>();
        }

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

        public string RoutingTypeToReadFromFile
        {
            get
            {
                return _routingTypeToReadFromFile;
            }

            set
            {
                _routingTypeToReadFromFile = value;
            }
        }

        public string RoutingType
        {
            get
            {
                return _routingType;
            }

            set
            {
                _routingType = value;
            }
        }

        public string PuppetMasterUrl
        {
            get
            {
                return _puppetMasterUrl;
            }

            set
            {
                _puppetMasterUrl = value;
            }
        }

        public bool IsLogFull
        {
            get { return _isLogFull; }
            set { _isLogFull = value; }
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
