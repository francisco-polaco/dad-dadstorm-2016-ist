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

        protected IList<ISibling> Init(List<string> siblingsUrls)
        {
            _siblings = new List<ISibling>();
            foreach (var url in _slaveObj.Siblings)
            {
                try
                {
                    var replica = (ISibling) Activator.GetObject(typeof(ISibling), url);
                    _siblings.Add(replica);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception encountered getting siblings " + url);
                }
            }
            return _siblings;
        }

        public abstract void Dispatch(TuplePack input);

        public abstract void ReplicaUpdate(string replicaUrl, IList<string> tupleFields);

        public IList<TuplePack> PollSibling()
        {
            return SlaveObj.SeenTuplePacks;
        }
    }
}
