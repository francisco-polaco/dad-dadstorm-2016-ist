using System;
using CommonTypes;

namespace ProcessCreationService
{
    public class FrozenState : State, ISlave
    {
        public FrozenState(Import importObj, Route routeObj, Process processObj) : 
            base(importObj, routeObj, processObj)
        {
        }

        public void Dispatch(string input)
        {
            throw new NotImplementedException();
        }

        public void Update(string toUpdate)
        {
            throw new NotImplementedException();
        }
    }

    public class UnfrozenState : State, ISlave
    {
        public UnfrozenState(Import importObj, Route routeObj, Process processObj)
            : base(importObj, routeObj, processObj)
        {
        }

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
