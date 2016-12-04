using System;
using System.Collections;
using System.Collections.Concurrent;
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
            SlaveObj.Slept = true;
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
            // If I "slept" I need to poll my sibilings for the last tuples processed
            if (SlaveObj.Slept && SlaveObj.Semantic.Equals("exactly-once"))
            {
                List<IList<TuplePack>> polledTuples = new List<IList<TuplePack>>();
                // init the proxies to the siblings
                IList<ISibling> siblings = Init(SlaveObj.Siblings);
                foreach (ISibling sibling in siblings)
                {
                    polledTuples.Add(sibling.PollSibling());
                }
                ConcurrentQueue<TuplePack> validTuples = new ConcurrentQueue<TuplePack>();
                foreach (IList<TuplePack> tuplesList in polledTuples)
                {
                    foreach (TuplePack tuple in tuplesList)
                    {
                        if(!SlaveObj.JobQueue.Contains(tuple))
                            validTuples.Enqueue(tuple);
                        Console.WriteLine("My brother already processed " + tuple.ToString());
                    }
                }
                SlaveObj.JobQueue = validTuples;
                SlaveObj.Slept = false;
            }

            // start command was issued || unfreeze happened
            if (input == null) 
            {
                while (SlaveObj.JobQueue.Count != 0)
                {
                    ProcessRoutePack(SlaveObj.GetJob());
                }

                // try importing
                List<string> tuples = SlaveObj.ImportObj.Import();

                // input comes from upstream operator (via routing) || 
                // already imported my tuples and I got unfrozen
                if (tuples == null) 
                    return;

                // input via file
                foreach (string s in tuples)
                {
                    ProcessRoutePack(new TuplePack(-2, SlaveObj.Url, SplitTuple(s)));
                }
            }
            // upstream operator has sent a tuple
            else
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

            if (tuplesList != null)
            {
                string processedTuples = string.Empty;
                foreach (var tuplepack in tuplesList)
                {
                    tuplepack.OpUrl = SlaveObj.Url;
                    tuplepack.SeqNumber = SlaveObj.SeqNumber;
                    SlaveObj.SeqNumber++;
                    
                    // Route
                    SlaveObj.RouteObj.Route(tuplepack);

                    // Seen the tuple
                    if (SlaveObj.Semantic.Equals("exactly-once")) {
                        lock (SlaveObj.SeenTuplePacks) { 
                            SlaveObj.SeenTuplePacks.Add(input);
                        }
                    }

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
