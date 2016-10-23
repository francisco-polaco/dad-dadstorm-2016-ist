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
        private List<SlaveProxy> mSlaveProxy;

        public Operator(int opid, List<string> urls)
        {
            mOpid = opid;
            mSlaveProxy = new List<SlaveProxy>();
            foreach(string s in urls)
            {
                mSlaveProxy.Add(new SlaveProxy(s));
            }
        }
    }

    class SlaveProxy:  ISlave
    {
        private string mUrl;
        private ISlave mRemoteObj;

        public SlaveProxy(string url)
        {
            mUrl = url;
            mRemoteObj = (ISlave)Activator.GetObject(
                typeof(ISlave),
                url);
        }

        public void Dispatch(string a)
        {
            mRemoteObj.Dispatch(a);
        }

    }
}
