using System.Collections;

namespace ProcessCreationService
{
    public abstract class State
    {
        private Import importObj;
        private Route routeObj;
        private Process processObj;

        public Import ImportObj
        {
            get { return importObj; }
            set { importObj = value; }
        }

        public Route RouteObj
        {
            get { return RouteObj; }
            set { routeObj = value; }
        }

        public Process ProcessObj
        {
            get { return ProcessObj; }
            set { processObj = value; }
        }

        public State(Import importObj, Route routeObj, Process processObj)
        {
            this.ImportObj = importObj;
            this.RouteObj = routeObj;
            this.ProcessObj = processObj;
        }

        protected ArrayList TupleToArrayList(string input)
        {
            ArrayList output = new ArrayList();
            char[] delimiterChars = { ',' };
            string[] content = input.Split(delimiterChars);

            foreach (string s in content)
                output.Add(s);
            return output;
        }

    }
}
