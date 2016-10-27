using CommonTypes;
using System.Collections;
using System.Collections.Generic;

namespace Slave
{
    public abstract class State : ISlave, ILogUpdate
    {
        private Slave slaveObj;

        public Slave SlaveObj
        {
            get { return slaveObj; }
        }

        public State(Slave slave)
        {
            this.slaveObj = slave;        
        }

        public abstract void Dispatch(string input);

        public abstract void ReplicaUpdate(string replicaUrl, List<string> tupleFields);

    }
}
