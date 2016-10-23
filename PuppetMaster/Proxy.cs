using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class Operator 
    {
        private int mOpid;
        private Dictionary<string, SlaveProxy> mSlaveProxyList;

        public Operator(int opid, List<string> urls)
        {
            mOpid = opid;
            mSlaveProxyList = new Dictionary<string, SlaveProxy>();
            foreach(string s in urls)
            {

                mSlaveProxyList.Add(s, new SlaveProxy(s));
            }
        }

        public void Crash(string url)
        {
            mSlaveProxyList[url].Crash();
        }

        public void Freeze(string url)
        {
            mSlaveProxyList[url].Freeze();
        }

        public void Interval(int ms)
        {
            foreach (KeyValuePair<string, SlaveProxy> entry in mSlaveProxyList)
            {
                entry.Value.Interval(ms);
            }
        }

        public void Start()
        {
            foreach (KeyValuePair<string, SlaveProxy> entry in mSlaveProxyList)
            {
                entry.Value.Start();
            }
        }

        public void Status()
        {
            foreach (KeyValuePair<string, SlaveProxy> entry in mSlaveProxyList)
            {
                entry.Value.Status();
            }
        }

        public void Unfreeze(string url)
        {
            mSlaveProxyList[url].Unfreeze();
        }
    }

    class SlaveProxy: RemoteCmdInterface
    {
        private string mUrl;

        public SlaveProxy(string url)
        {
            mUrl = url;
        }

        public void Freeze()
        {
            RemoteCmdInterface remoteObj = (RemoteCmdInterface)Activator.GetObject(
                typeof(RemoteCmdInterface),
                mUrl);
            //remoteObj.Freeze();
        }

        public void Interval(int ms)
        {
            RemoteCmdInterface remoteObj = (RemoteCmdInterface)Activator.GetObject(
                typeof(RemoteCmdInterface),
                mUrl);
            //remoteObj.Interval(ms);
        }

        public void Start()
        {
            RemoteCmdInterface remoteObj = (RemoteCmdInterface)Activator.GetObject(
                typeof(RemoteCmdInterface),
                mUrl);
            //remoteObj.Start();
        }

        public void Status()
        {
            RemoteCmdInterface remoteObj = (RemoteCmdInterface)Activator.GetObject(
                typeof(RemoteCmdInterface),
                mUrl);
            //remoteObj.Status();
        }

        public void Unfreeze()
        {
            RemoteCmdInterface remoteObj = (RemoteCmdInterface)Activator.GetObject(
                typeof(RemoteCmdInterface),
                mUrl);
            //remoteObj.Unfreeze();
        }

        public void Crash()
        {
            RemoteCmdInterface remoteObj = (RemoteCmdInterface)Activator.GetObject(
                typeof(RemoteCmdInterface),
                mUrl);
            //remoteObj.Crash();
        }
    }
}
