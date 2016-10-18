using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    //public class CommonTypes
    //{
    //}

    public interface LogUpdate
    {
        void Update(string log);
    }

    interface RemoteCmdInterface
    {
        void Start(int opid);
        void Interval(int opid, int ms);
        void Status();
        void Crash(string url);
        void Freeze(string url);
        void Unfreeze(string url);
    }

    public interface Slave
    {
        void Dispatch(string a);
        void Update(string s);
    }
}
