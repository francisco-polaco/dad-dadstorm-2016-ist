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
            Console.WriteLine(input);
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

        public override bool SendFinalProposal(DateTime x, TuplePack toPropose)
        {
            throw new NotImplementedException();
        }

        public override IList<TuplePack> Purpose(DateTime x, TuplePack toDispatch)
        {
            throw new NotImplementedException();
        }

        public override bool TryToPurpose(TuplePack purpose)
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
                // try importing
                IList<Dictionary<int, string>> tuplesLists = SlaveObj.ImportObj.Import();

                // input comes from upstream operator (via routing) || 
                // already imported my tuples and I got unfrozen
                if (tuplesLists == null)
                    return;

                // first list my tuples
                // second list siblings tuples
                if (tuplesLists.Count == 2)
                    SlaveObj.BufferFirstOperatorLines = tuplesLists[1];

                // input via file
                foreach (KeyValuePair<int,string> tuple in tuplesLists[1])
                {
                    ProcessRoutePack(new TuplePack(tuple.Key, SlaveObj.Url, SplitTuple(tuple.Value)));
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
             Console.WriteLine("Let's see if I have to process: " + input);

            double baseExponent = SlaveObj.RandSeed.Next(10);
            Dictionary<string, IList<TuplePack>> proposals;
            // exponencial backoff
             DateTime currentDate = DateTime.Now;
            while ((proposals = TryToPurpose(currentDate, input)) == null)
            {
                Thread.Sleep(Convert.ToInt32((Math.Pow(2, baseExponent) - 1) / 2));
                baseExponent += SlaveObj.RandSeed.Next(3);
            }
             ChooseProposal(currentDate, proposals);


             /*if (SlaveObj.SeenTuplePacks.Contains(input))
             {
                 Console.WriteLine("I already seen that tuple!");
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
                    Thread.Sleep(SlaveObj.RandSeed.Next(800, 1500));
                    List<bool> decisions = MayIProcess(input);
                    // Can't know for sure if the input has been processed, since a sibling didn't respond
                    if (AlreadySeen(decisions))
                    {
                        Console.WriteLine("Tuple has already been seen by one of my siblings!");
                        return;
                    }
                    if (decisions.Count == Siblings.Count && NoneSeen(decisions))
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
                        List<string> sucesffulySent = DestributeTuple(input, SlaveObj.Siblings);
                        // if I need to mantain the state I need to assure that all of my siblings get the tuple
                        if (SlaveObj.Stateful)
                        {
                            // avoid resend to the siblings that already received
                            int difference = SlaveObj.Siblings.Count - sucesffulySent.Count;
                            while (difference != 0)
                            {
                                sucesffulySent = DestributeTuple(input, sucesffulySent);
                                difference = difference - sucesffulySent.Count;
                            }
                        }
                    }
 
                    ReplicaUpdate(SlaveObj.Url, tuplepack.Content);
                    // Debug purposes
                    processedTuples += tuplepack.Content.Count == 1 ? tuplepack.Content[0] + " " : MergeOutput(tuplepack.Content) + " ";
                }
                Console.WriteLine("Processed from: " + MergeOutput(input.Content) + " : " + processedTuples);
            }*/
         }

        // if it there is someone that respondes false, then he is processing something
        public Dictionary<string, IList<TuplePack>> TryToPurpose(DateTime x, TuplePack input)
        {
            // A node chooses to become the Leader and selects a sequence number x and 
            // value v to create a proposal P1(x, v).It sends this proposal to the acceptors 
            // and waits till a majority responds.

            Dictionary<string,IList<TuplePack>> responses = new Dictionary<string, IList<TuplePack>>();

            List<string> toRemove = new List<string>();
            foreach (string siblingUrl in SlaveObj.Siblings)
            {
                try
                {
                    var replica = (ISibling) Activator.GetObject(typeof(ISibling), siblingUrl);
                    responses[siblingUrl] = replica.Purpose(x, input);
                }
                // he is dead or slowed so it's opinion doens't count 
                catch (SocketException e)
                {
                    toRemove.Add(siblingUrl);
                }
                catch (Exception e)
                {
                    toRemove.Add(siblingUrl);
                }
            }

            // remove crashed replicas
            foreach (var crashed in toRemove)
            {
                SlaveObj.Siblings.Remove(crashed);
            }

            // no responses abort
            if (responses.Count == 0)
                return null;

            // check the reject responses
            int rejects = 0;
            foreach (var response in responses)
            {
                if (response.Value == null)
                    rejects++;
            }
            if (rejects >= SlaveObj.Siblings.Count/2 + 1)
                return null;

            // check the non responds
            if (responses.Count < SlaveObj.Siblings.Count/2 + 1)
                return null;

            // remove possible nulls
            Dictionary<string, IList<TuplePack>> output = new Dictionary<string, IList<TuplePack>>();
            foreach (var proposal in responses)
            {
                if (proposal.Value != null)
                    output[proposal.Key] = proposal.Value;
            }

            return output;
        }

        private TuplePack ChooseProposal(DateTime x, Dictionary<string, IList<TuplePack>> proposals, TuplePack toPropose)
        {
            foreach (var proposal in proposals)
            {
                foreach (TuplePack pack in proposal.Value)
                {
                    if (!SlaveObj.SeenTuplePacks.Contains(pack))
                    {
                        SlaveObj.SeenTuplePacks.Add(pack);
                        if (SlaveObj.Stateful)
                        {
                            SlaveObj.ProcessObj.Process(pack);
                        }

                    }
                }
            }
            return null;
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

        public override bool SendFinalProposal(DateTime x, TuplePack toPropose)
        {
            if (!SlaveObj.SeenTuplePacks.Contains(toPropose))
            {
                SlaveObj.SeenTuplePacks.Add(toPropose);
                // if I belong to a operator that needs to mantain state I need to process
                if (SlaveObj.Stateful)
                    SlaveObj.ProcessObj.Process(toPropose);
            }
        }

        public override IList<TuplePack> Purpose(DateTime x, TuplePack toDispatch)
        {
            return Monitor.IsEntered(SlaveObj);
        }

        public List<string> DestributeTuple(TuplePack toAnnounce, List<string> siblingsUrls)
        {
            // assures that we don't stay in while loop so often
            // and in the case of need to mantain state we get the tuple asap!
            List<string> sucessfullySent = new List<string>();
            foreach (string siblingUrl in siblingsUrls)
            {
                try
                {
                    var replica = (ISibling) Activator.GetObject(typeof(ISibling), siblingUrl);
                    replica.SendFinalProposal(TODO, toAnnounce);
                    sucessfullySent.Add(siblingUrl);
                }
                // crashed - theres nothing we can do
                catch (SocketException e)
                {
                    sucessfullySent.Add(siblingUrl);
                }
                // maybe slowed - we need to assure that it gets there
                catch (SlowException e)
                {
                }
            }
            return sucessfullySent;
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
            List<string> toRemove = new List<string>();
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
                    toRemove.Add(siblingUrl);
                }
                catch (NotImplementedException e)
                {
                    // Sibling doens't respond
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.GetBaseException());
                }

            }
            // remove the siblings that don't need to be considered
            foreach (var siblingurl in toRemove)
            {
                SlaveObj.Siblings.Remove(siblingurl);
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
