using System;

namespace ProcessCreationService
{
    public class FrozenState : CommonTypes.Slave
    {
        public void Dispatch(string input)
        {
            throw new NotImplementedException();
        }

        public void Update(string toUpdate)
        {
            throw new NotImplementedException();
        }
    }

    public class UnfrozenState : CommonTypes.Slave
    {
        public void Dispatch(string input)
        {
            throw new NotImplementedException();
        }

        public void Update(string toUpdate)
        {
            throw new NotImplementedException();
        }
    }

}
