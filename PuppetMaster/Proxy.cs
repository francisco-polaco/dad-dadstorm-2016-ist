﻿using CommonTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class Operator
    {
        private uint mOpid;
        private List<SlaveProxy> mSlaveProxy;

        Operator(uint opid)
        {
            mOpid = opid;
            mSlaveProxy = new List<SlaveProxy>();
        }
    }

    class SlaveProxy:  ISlave
    {
        private string mUrl;
        private ISlave mRemoteObj;

        SlaveProxy(string url)
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

        public void Update(string s)
        {
            mRemoteObj.Update(s);
        }
    }
}