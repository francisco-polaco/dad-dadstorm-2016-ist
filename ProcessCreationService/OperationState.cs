using System;

namespace ProcessCreationService
{
    public class FrozenState : State
    {
        public FrozenState(Import importObj, Route routeObj, Process processObj) : 
            base(importObj, routeObj, processObj)
        {
        }

        public override void Dispatch(string input)
        {
            throw new NotImplementedException();
        }

        public override void Update(string toUpdate)
        {
            throw new NotImplementedException();
        }
    }

    public class UnfrozenState : State
    {
        public UnfrozenState(Import importObj, Route routeObj, Process processObj)
            : base(importObj, routeObj, processObj)
        {
        }

        public override void Dispatch(string input)
        {
            throw new NotImplementedException();
        }

        public override void Update(string toUpdate)
        {
            throw new NotImplementedException();
        }
    }

}
