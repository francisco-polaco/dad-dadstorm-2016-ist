using System;
using CommonTypes;
using System.Collections.Generic;

namespace Slave
{
    public abstract class State : ISlave, ILogUpdate, ISibling
    {
        private Slave _slaveObj;
        private IList<ISibling> _siblings = new List<ISibling>();

        // getters and setters
        public Slave SlaveObj => _slaveObj;

        public IList<ISibling> Siblings { get; set; }

        protected State(Slave slave)
        {
            _slaveObj = slave;    
        }

        public abstract void Dispatch(TuplePack input);

        public abstract void ReplicaUpdate(string replicaUrl, IList<string> tupleFields);

        public abstract bool PollTuple(TuplePack toRoute);
      
        public void AnnounceTuple(TuplePack toAnnounce)
        {
            if (!SlaveObj.SeenTuplePacks.Contains(toAnnounce)) {
                SlaveObj.SeenTuplePacks.Add(toAnnounce);
                // if I belong to a operator that needs to mantain state I need to process
                if (SlaveObj.Stateful)
                    SlaveObj.ProcessObj.Process(toAnnounce);
            }
        }
    }
}
