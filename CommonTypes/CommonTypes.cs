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
    }

    public interface IPCSSlaveLaunch
    {
        void Launch(string input);
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
    }

}
