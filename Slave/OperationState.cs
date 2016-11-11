using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
            // put job in queue
            SlaveObj.addJob(input);
        }

        public override void ReplicaUpdate(string replicaUrl, List<string> tupleFields)
        {
            // nothing to do (no jobs processed)
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
            if (input == null) // start command was issued
            {
                string tuple;
                string outp;
                while (SlaveObj.JobQueue.Count != 0)
                {
                    tuple = SlaveObj.getJob();
                    outp = SlaveObj.ProcessObj.Process(tuple);

                    if (outp.Equals(string.Empty))
                        continue;

                    SlaveObj.RouteObj.Route(outp);

                    // log with PuppetMaster
                    ReplicaUpdate(SlaveObj.Url, SplitTuple(outp));
                }

                List<string> tuples = SlaveObj.ImportObj.Import(); // try importing

                if (tuples == null) // input comes from upstream operator (via routing)
                    return;

                // input via file
                foreach (string s in tuples)
                {
                    Console.WriteLine("Processing tuple: " + s);
                    string output = SlaveObj.ProcessObj.Process(s);

                    
                    if (output.Equals(string.Empty))
                        continue;

                    SlaveObj.RouteObj.Route(output);

                    // log with PuppetMaster
                    ReplicaUpdate(SlaveObj.Url, SplitTuple(output));
                }
            }
            else // upstream operator has sent a tuple
            {
                Console.WriteLine("Processing tuple: " + input);

                string output = SlaveObj.ProcessObj.Process(input);
                SlaveObj.RouteObj.Route(output);

                // log with PuppetMaster
                ReplicaUpdate(SlaveObj.Url, SplitTuple(output));
            }
        }

        public override void ReplicaUpdate(string replicaUrl, List<string> tupleFields)
        {
            // send log
            SlaveObj.PuppetLogProxy.ReplicaUpdate(replicaUrl, tupleFields);
        }

        // split tuple in fields
        public List<string> SplitTuple(string tuple)
        {
            
            string pattern = @",|\s";
            string[] tuple_fields = Regex.Split(tuple, pattern).Where(s => s != String.Empty).ToArray<string>();

            return tuple_fields.ToList<string>();
        }
    }

}
