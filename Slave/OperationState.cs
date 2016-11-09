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
            // Se o input vier a null:
            // -> Tentas SlaveObj.ImportObj.Import():
            //    a) null = não há nada para processar retornas
            //    b) não null = iteras a lista retornada processas (cuidado que o process retorna string.empty quando os requisitos n sao preenchidos) e das route 
            // Caso o input n venha a null:
            // -> fazes process do input e das route

            if (input == null) // start command was issued
            {
                List<string> tuples = SlaveObj.ImportObj.Import(); // try importing

                if (tuples == null) // input comes from upstream operator (via routing)
                    return;

                // input via file
                foreach (string s in tuples)
                {
                    string output = SlaveObj.ProcessObj.Process(s);
                    SlaveObj.RouteObj.Route(output);

                    // log with PuppetMaster
                    ReplicaUpdate(SlaveObj.Url, SplitTuple(output));
                }
            }
            else // upstream operator has sent a tuple
            {
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
