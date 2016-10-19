using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        void Update(string s);
    }

    public interface ISlaveLaunch
    {
        void Launch(string input);
    }
}
