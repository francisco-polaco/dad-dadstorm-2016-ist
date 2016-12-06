using System;
using CommonTypes;
using System.Collections.Generic;

namespace Slave
{
    public abstract class State : ISlave, ILogUpdate, ISibling
    {
        private Slave _slaveObj;
        private IList<ISibling> _siblings;

        // getters and setters
        public Slave SlaveObj => _slaveObj;

        public IList<ISibling> Siblings => _siblings;

        protected State(Slave slave)
        {
            _slaveObj = slave;    
        }

        public abstract void Dispatch(TuplePack input);

        public abstract void ReplicaUpdate(string replicaUrl, IList<string> tupleFields);

        public bool PollTuple(TuplePack toRoute)
        {
            return SlaveObj.SeenTuplePacks.Contains(toRoute);
        }

        public void AnnounceTuple(TuplePack toAnnounce)
        {
            if(!SlaveObj.SeenTuplePacks.Contains(toAnnounce))
                SlaveObj.SeenTuplePacks.Add(toAnnounce);   
        }
    }
}
