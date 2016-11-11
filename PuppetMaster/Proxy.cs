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
        private int _opId;
        private List<SlaveProxy> _slaveProxyList;

        public Operator(int opid, List<string> urls)
        {
            _opId = opid;
            _slaveProxyList = new List<SlaveProxy>();
            foreach(string s in urls)
            {
                _slaveProxyList.Add(new SlaveProxy(s));
            }
        }

        public void Crash(int repNumber)
        {
            _slaveProxyList[repNumber].Crash();
        }

        public void Freeze(int repNumber)
        {
            _slaveProxyList[repNumber].Freeze();
        }

        public void Interval(int ms)
        {
            foreach (SlaveProxy sp in _slaveProxyList)
            {
                sp.Interval(ms);
            }
        }

        public void Start()
        {
            foreach (SlaveProxy sp in _slaveProxyList)
            {
                sp.Start();
            }
        }

        public void Status()
        {
            foreach (SlaveProxy sp in _slaveProxyList)
            {
                sp.Status();
            }
        }

        public void Unfreeze(int repNumber)
        {
            _slaveProxyList[repNumber].Unfreeze();
        }

        public void Exit()
        {
            foreach (SlaveProxy sp in _slaveProxyList)
            {
                sp.Exit();
            }
        }
    }

    public delegate void RemoteAsyncNoArgsDelegate();
    public delegate void RemoteAsyncArgsDelegate(int ms);


    class SlaveProxy : IRemoteCmdInterface
    {
        private string _url;

        private string _methodRan;

        public SlaveProxy(string url)
        {
            _url = url;
            _methodRan = "";
        }

        public void Freeze()
        {
            IRemoteCmdInterface remoteObj = (IRemoteCmdInterface)Activator.GetObject(
                typeof(IRemoteCmdInterface),
                _url);
            _methodRan = "Freeze";
            RemoteAsyncNoArgsDelegate RemoteDel = new RemoteAsyncNoArgsDelegate(remoteObj.Freeze);
            AsyncCallback RemoteCallback = new AsyncCallback(this.OnRemoteCallback);
            // Call remote method
            IAsyncResult RemAr = RemoteDel.BeginInvoke(RemoteCallback, null);
        }

        public void Interval(int ms)
        {
            IRemoteCmdInterface remoteObj = (IRemoteCmdInterface)Activator.GetObject(
                typeof(IRemoteCmdInterface),
                _url);
            _methodRan = "Interval";
            RemoteAsyncArgsDelegate RemoteDel = new RemoteAsyncArgsDelegate(remoteObj.Interval);
            AsyncCallback RemoteCallback = new AsyncCallback(this.OnRemoteCallback);
            // Call remote method
            IAsyncResult RemAr = RemoteDel.BeginInvoke(ms, RemoteCallback, null);
        }

        public void Start()
        {
            IRemoteCmdInterface remoteObj = (IRemoteCmdInterface)Activator.GetObject(
                typeof(IRemoteCmdInterface),
                _url);
            _methodRan = "Start";
            RemoteAsyncNoArgsDelegate RemoteDel = new RemoteAsyncNoArgsDelegate(remoteObj.Start);
            AsyncCallback RemoteCallback = new AsyncCallback(this.OnRemoteCallback);
            // Call remote method
            IAsyncResult RemAr = RemoteDel.BeginInvoke(RemoteCallback, null);
        }

        public void Status()
        {
            IRemoteCmdInterface remoteObj = (IRemoteCmdInterface)Activator.GetObject(
                typeof(IRemoteCmdInterface),
                _url);
            _methodRan = "Status";
            RemoteAsyncNoArgsDelegate RemoteDel = new RemoteAsyncNoArgsDelegate(remoteObj.Status);
            AsyncCallback RemoteCallback = new AsyncCallback(this.OnRemoteCallback);
            // Call remote method
            IAsyncResult RemAr = RemoteDel.BeginInvoke(RemoteCallback, null);
        }

        public void Unfreeze()
        {
            IRemoteCmdInterface remoteObj = (IRemoteCmdInterface)Activator.GetObject(
                typeof(IRemoteCmdInterface),
                _url);
            _methodRan = "Unfreeze";
            RemoteAsyncNoArgsDelegate RemoteDel = new RemoteAsyncNoArgsDelegate(remoteObj.Unfreeze);
            AsyncCallback RemoteCallback = new AsyncCallback(this.OnRemoteCallback);
            // Call remote method
            IAsyncResult RemAr = RemoteDel.BeginInvoke(RemoteCallback, null);
        }

        public void Crash()
        {
            IRemoteCmdInterface remoteObj = (IRemoteCmdInterface)Activator.GetObject(
                typeof(IRemoteCmdInterface),
                _url);
            _methodRan = "Crash";
            RemoteAsyncNoArgsDelegate RemoteDel = new RemoteAsyncNoArgsDelegate(remoteObj.Crash);
            AsyncCallback RemoteCallback = new AsyncCallback(this.OnRemoteCallback);
            // Call remote method
            IAsyncResult RemAr = RemoteDel.BeginInvoke(RemoteCallback, null);
        }

        public void Exit()
        {
            PuppetMaster.GetInstance().Log("Killing " + _url);
            IRemoteCmdInterface remoteObj = (IRemoteCmdInterface)Activator.GetObject(
                typeof(IRemoteCmdInterface),
                _url);
            remoteObj.Exit();
        }

        public void OnRemoteCallback(IAsyncResult ar)
        {
            PuppetMaster.GetInstance().Log("Callback from method: " + _methodRan);
        }
    }
}
