using CommonTypes;
using System.Collections.Generic;

namespace Slave
{
    public abstract class State : ISlave, ILogUpdate
    {
        private Slave _slaveObj;

        public Slave SlaveObj => _slaveObj;

        protected State(Slave slave)
        {
            _slaveObj = slave;        
        }

        public abstract void Dispatch(TuplePack input);

        public abstract void ReplicaUpdate(string replicaUrl, IList<string> tupleFields);

    }
}
