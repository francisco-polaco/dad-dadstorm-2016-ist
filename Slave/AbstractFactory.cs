using CommonTypes;
using System.Collections.Generic;

namespace Slave
{
    public abstract class AbstractFactory
    {
        public abstract Import GetImport(string[] specs, string[] filePaths);
        public abstract Route GetRouting(string[] specs, List<string> urls);
        public abstract Process GetProcessing(string[] specs);
    }
}
