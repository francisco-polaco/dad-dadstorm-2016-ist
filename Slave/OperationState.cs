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
            // put job in queue
            SlaveObj.AddJob(input);
            // behave likea slow network/operator
            Thread.Sleep(SlaveObj.RandSeed.Next(1000,5000));
            throw new SlowException();
        }

        public override void ReplicaUpdate(string replicaUrl, IList<string> tupleFields)
        {
            // nothing to do (no jobs processed)
            throw new NotImplementedException();
        }

        public override bool PollTuple(TuplePack toRoute)
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

        public override void Dispatch(TuplePack input)
        {
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
                int packNumber = 0;
                foreach (string s in tuples)
                {
                    ProcessRoutePack(new TuplePack(packNumber++, "File Imported", SplitTuple(s)));
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
            Console.WriteLine("Attempting to process tuple: " + input.ToString());

            if (SlaveObj.SeenTuplePacks.Contains(input))
            {
                Console.WriteLine("Tuple has already been seen by one of my siblings!");
                return;
            }

            // Check if the tuple was already seen
            if (SlaveObj.Semantic.Equals("exactly-once"))
            {
                Console.WriteLine("Deciding with my siblings...");
                // while loop:
                //  break conditions:
                //    -> if one of the decisions is false, its sign that the tuple hasn't been processed.
                //    -> if all the decisions are false and the number of them it's equal to the siblings number - then you can assume that none processed. 
                // so try again until you can match the break conditions. 
                while (true)
                {
                    Thread.Sleep(SlaveObj.RandSeed.Next(800,1500));
                    List<bool> decisions = MayIProcess(input);
                    // Can't know for sure if the input has been processed, since a sibling didn't respond
                    if (AlreadySeen(decisions)) {
                        Console.WriteLine("Tuple has already been seen by one of my siblings!");
                        break;
                    }
                    if (decisions.Count == Siblings.Count || NoneSeen(decisions))
                        break;
                }
            }

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

                    // If I processed I have seen it
                    if (SlaveObj.Semantic.Equals("exactly-once")) {
                        SlaveObj.SeenTuplePacks.Add(input);
                        DestributeTuple(input);
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

        public override bool PollTuple(TuplePack toRoute)
        {
            return SlaveObj.SeenTuplePacks.Contains(toRoute);
        }

        public void DestributeTuple(TuplePack toAnnounce)
        {
            // assures that we don't stay in while loop so often
            foreach (string siblingUrl in SlaveObj.Siblings)
            {
                try
                {
                    var replica = (ISibling) Activator.GetObject(typeof(ISibling), siblingUrl);
                    replica.AnnounceTuple(toAnnounce);
                }
                //Don't do nothing, we are optimists
                catch (SocketException e) {}
            }
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

        private List<bool> MayIProcess(TuplePack input)
        {
            List<bool> decisions = new List<bool>();
            Siblings = new List<ISibling>();
            // Query my brothers
            foreach (string siblingUrl in SlaveObj.Siblings)
            {
                try
                {
                    ISibling replica = (ISibling) Activator.GetObject(typeof(ISibling), siblingUrl);
                    Siblings.Add(replica);
                    decisions.Add(replica.PollTuple(input));
                }
                catch (SocketException e)
                {
                    // Sibling has crashed don't consider it
                    SlaveObj.Siblings.Remove(siblingUrl);
                }
                catch (NotImplementedException e)
                {
                    // Sibling doens't respond
                }

            }
            return decisions;
        }

        private bool NoneSeen(List<bool> decisions)
        {
            return !decisions.Contains(true);
        }

        private bool AlreadySeen(List<bool> decisions)
        {
            return decisions.Contains(true);
        }

    }
}
