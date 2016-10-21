using CommonTypes;
using System.Collections;

namespace ProcessCreationService
{
    public abstract class State : ISlave
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

        public abstract void Update(string toUpdate);
    }
}
