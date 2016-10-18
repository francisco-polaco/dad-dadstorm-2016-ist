using System;

namespace ProcessCreationService
{
    public class FrozenState : CommonTypes.ISlave
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

    public class UnfrozenState : CommonTypes.ISlave
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
