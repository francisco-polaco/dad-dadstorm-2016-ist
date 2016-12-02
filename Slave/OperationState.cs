using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using CommonTypes;

namespace Slave
{
    public class FrozenState : State
    {
        public FrozenState(Slave slave) : 
            base(slave)
        {
        }

        public override void Dispatch(TuplePack input)
        {
            // put job in queue
            SlaveObj.AddJob(input);
            throw new SocketException();
        }

        public override void ReplicaUpdate(string replicaUrl, IList<string> tupleFields)
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

        public override void Dispatch(TuplePack input)
        { 
            if (input == null) // start command was issued
            {
                while (SlaveObj.JobQueue.Count != 0)
                {
                    ProcessRoutePack(SlaveObj.GetJob());
                }

                List<string> tuples = SlaveObj.ImportObj.Import(); // try importing

                if (tuples == null) // input comes from upstream operator (via routing)
                    return;

                // input via file
                foreach (string s in tuples)
                {
                    ProcessRoutePack(new TuplePack(-2, SlaveObj.Url, SplitTuple(s)));
                }
            }
            else // upstream operator has sent a tuple
            {
                ProcessRoutePack(input);
            }
        }

        // Responsible to process and route the tuples
        private void ProcessRoutePack(TuplePack input)
        {
            SleepInterval(SlaveObj.IntervalValue);
            Console.WriteLine("Attempting to process tuple: " + MergeOutput(input.Content));
            IList<TuplePack> tuplesList = SlaveObj.ProcessObj.Process(input);

            string processedTuples = string.Empty;
            if (tuplesList != null)
            {
                foreach (var tuplepack in tuplesList)
                {
                    tuplepack.OpUrl = SlaveObj.Url;
                    tuplepack.SeqNumber = SlaveObj.SeqNumber;
                    SlaveObj.SeqNumber++;
                    // Route
                    SlaveObj.RouteObj.Route(tuplepack);
                    ReplicaUpdate(SlaveObj.Url, tuplepack.Content);
                    // Debug purposes
                    processedTuples += tuplepack.Content.Count == 1 ? tuplepack.Content[0] + " " : MergeOutput(tuplepack.Content) + " ";
                }
                Console.WriteLine("Processed from: " + MergeOutput(input.Content) + " : " + processedTuples);
            }
        }

        // Log the events
        public override void ReplicaUpdate(string replicaUrl, IList<string> tupleFields)
        {
            SlaveObj.PuppetLogProxy.ReplicaUpdate(replicaUrl, tupleFields);
        }

        // Split tuple in fields
        private List<string> SplitTuple(string tuple)
        {
            string pattern = @",|\s";
            string[] tupleFields = Regex.Split(tuple, pattern).Where(s => s != String.Empty).ToArray<string>();

            return tupleFields.ToList<string>();
        }

        // Merge the tuple
        private string MergeOutput(IList<string> content)
        {
            string output = string.Empty;

            if (content.Count == 1)
                return content[0];

            for (int i = 0; i < content.Count; i++)
            {
                output += i == (content.Count - 1) ? content[i] : content[i] + ", ";
            }
            return output;
        }

        private void SleepInterval(int ms)
        {
            if (SlaveObj.IntervalValue != 0)
                Thread.Sleep(SlaveObj.IntervalValue);
        }

    }
}
