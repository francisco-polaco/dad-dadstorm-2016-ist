using System;

namespace Slave
{
    public class FrozenState : State
    {
        public FrozenState(Slave slave) : 
            base(slave)
        {
        }

        public override void Dispatch(string input)
        {
            // TO DO
            throw new NotImplementedException();
        }

        public override void Update(string toUpdate)
        {
            throw new NotImplementedException();
        }
    }

    public class UnfrozenState : State
    {
        public UnfrozenState(Slave slave)
            : base(slave)
        {
        }

        public override void Dispatch(string input)
        {
            // TO DO
            throw new NotImplementedException();
        }

        public override void Update(string toUpdate)
        {
            throw new NotImplementedException();
        }
    }

}
