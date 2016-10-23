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
        private List<SlaveProxy> mSlaveProxyList;

        public Operator(int opid, List<string> urls)
        {
            mOpid = opid;
            mSlaveProxyList = new List<SlaveProxy>();
            foreach(string s in urls)
            {

                mSlaveProxyList.Add(new SlaveProxy(s));
            }
        }

        public void Crash(string url)
        {
            throw new NotImplementedException();
        }

        public void Freeze(string url)
        {
            throw new NotImplementedException();
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
            foreach(SlaveProxy sp in mSlaveProxyList)
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

        public void Unfreeze(string url)
        {
            throw new NotImplementedException();
        }
    }

    class SlaveProxy:  ISlave
    {
        private string mUrl;

        public SlaveProxy(string url)
        {
            mUrl = url;
        }

        public void Dispatch(string a)
        {
            ISlave remoteObj = (ISlave)Activator.GetObject(
                typeof(ISlave),
                mUrl);
            remoteObj.Dispatch(a);
        }

        public void Freeze(string url)
        {
            throw new NotImplementedException();
        }

        public void Interval(int ms)
        {
            ISlave remoteObj = (ISlave)Activator.GetObject(
                typeof(ISlave),
                mUrl);
            //remoteObj.Interval(ms);
        }

        public void Start()
        {
            ISlave remoteObj = (ISlave)Activator.GetObject(
                typeof(ISlave),
                mUrl);
            //remoteObj.Start();
        }

        public void Status()
        {
            ISlave remoteObj = (ISlave)Activator.GetObject(
                typeof(ISlave),
                mUrl);
            //remoteObj.Status();
        }

        public void Unfreeze(string url)
        {
            throw new NotImplementedException();
        }

        public void Crash(string url)
        {
            throw new NotImplementedException();
        }
    }
}
