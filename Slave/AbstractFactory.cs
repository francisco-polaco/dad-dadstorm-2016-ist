using CommonTypes;
using System.Collections.Generic;

namespace Slave
{
    public abstract class AbstractFactory
    {
        public abstract Import GetImport(string[] specs);
        public abstract Route GetRouting(string[] specs, List<Replica> replica);
        public abstract Process GetProcessing(string[] specs);
    }
}
