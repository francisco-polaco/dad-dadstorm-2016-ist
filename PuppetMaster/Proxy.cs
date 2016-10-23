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
        private int mOpId;
        private List<SlaveProxy> mSlaveProxyList;

        public Operator(int opid, List<string> urls)
        {
            mOpId = opid;
            mSlaveProxyList = new List<SlaveProxy>();
            foreach(string s in urls)
            {
                mSlaveProxyList.Add(new SlaveProxy(s));
            }
        }

        public void Crash(int repNumber)
        {
            mSlaveProxyList[repNumber].Crash();
        }

        public void Freeze(int repNumber)
        {
            mSlaveProxyList[repNumber].Freeze();
        }

        public void Interval(int ms)
        {
            foreach (SlaveProxy sp in mSlaveProxyList)
            {
                sp.Interval(ms);
            }
        }

        public void Start()
        {
            foreach (SlaveProxy sp in mSlaveProxyList)
            {
                sp.Start();
            }
        }

        public void Status()
        {
            foreach (SlaveProxy sp in mSlaveProxyList)
            {
                sp.Status();
            }
        }

        public void Unfreeze(int repNumber)
        {
            mSlaveProxyList[repNumber].Unfreeze();
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
