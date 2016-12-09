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

        public override bool SendFinalProposal(Proposal x1)
        {
            throw new NotImplementedException();
        }

        public override Proposal Purpose(Proposal x1)
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

            if (SlaveObj.Semantic.ToLower().Equals("exactly-once")) {

                double baseExponent = SlaveObj.RandSeed.Next(10);
                List<Proposal> proposals;
                // exponencial backoff
                Proposal p = new Proposal(DateTime.Now, input);
                while ((proposals = TryToPurpose(p)) == null)
                {
                    Thread.Sleep(Convert.ToInt32((Math.Pow(2, baseExponent) - 1) / 2));
                    baseExponent += SlaveObj.RandSeed.Next(3);
                }
                if (!ChooseProposal(proposals, p))
                    return;
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
                    
                    if(SlaveObj.Semantic.ToLower().Equals("exactly-once"))
                        // if i have processed i have seen it
                        SlaveObj.SeenTuplePacks.Add(input);
                                       
                    ReplicaUpdate(SlaveObj.Url, tuplepack.Content);
                    // Debug purposes
                    processedTuples += tuplepack.Content.Count == 1 ? tuplepack.Content[0] + " " : MergeOutput(tuplepack.Content) + " ";
                }
                Console.WriteLine("Processed from: " + MergeOutput(input.Content) + " : " + processedTuples);
            }
         }

        // if it there is someone that respondes false, then he is processing something
        public List<Proposal> TryToPurpose(Proposal x)
        {
            // A node chooses to become the Leader and selects a sequence number x and 
            // value v to create a proposal P1(x, v).It sends this proposal to the acceptors 
            // and waits till a majority responds.

            List<Proposal> responses = new List<Proposal>();

            List<string> toRemove = new List<string>();
            foreach (string siblingUrl in SlaveObj.Siblings)
            {
                try
                {
                    var replica = (ISibling) Activator.GetObject(typeof(ISibling), siblingUrl);
                    responses.Add(replica.Purpose(x));
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
                if (response == null)
                    rejects++;
            }
            if (rejects >= SlaveObj.Siblings.Count/2 + 1)
                return null;

            // check the non responds
            if (responses.Count < SlaveObj.Siblings.Count/2 + 1)
                return null;

            // remove possible nulls
            List<Proposal> output = new List<Proposal>();
            foreach (var proposal in responses)
            {
                if (proposal != null)
                    output.Add(proposal);
            }

            return output;
        }

        private bool ChooseProposal(List<Proposal> proposals, Proposal x)
        {
            foreach (var proposal in proposals)
            {
                if (!SlaveObj.SeenTuplePacks.Contains(proposal.GetProposal))
                {
                    SlaveObj.SeenTuplePacks.Add(proposal.GetProposal);
                    if (SlaveObj.Stateful)
                    {
                        SlaveObj.ProcessObj.Process(proposal.GetProposal);
                    }
                }
                proposals.Sort();
                return SendProposals(proposals[proposals.Count-1]);
            }
            // if no values have been accepted yet, uses its own
            return SendProposals(x);
        }

        private bool SendProposals(Proposal x)
        {
            List<bool> responses = new List<bool>();

            List<string> toRemove = new List<string>();
            foreach (string siblingUrl in SlaveObj.Siblings)
            {
                try
                {
                    var replica = (ISibling)Activator.GetObject(typeof(ISibling), siblingUrl);
                    responses.Add(replica.SendFinalProposal(x));
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
                return false;

            // check the reject responses
            int rejects = 0;
            foreach (var response in responses)
            {
                if (response == false)
                    rejects++;
            }
            if (rejects >= SlaveObj.Siblings.Count / 2 + 1)
                return false;

            // check the non responds
            if (responses.Count < SlaveObj.Siblings.Count / 2 + 1)
                return false;

            return true;
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

        public override bool SendFinalProposal(Proposal x1)
        {
            if (SlaveObj.SeenTuplePacks.Contains(x1.GetProposal))
                return true;
            bool max = true;
            foreach (var proposals in SlaveObj.PaxosAggreedProposals)
            {
                if (x1.DateTime > proposals.DateTime)
                    max = false;
            }
            return max;
        }

        public override Proposal Purpose(Proposal x1)
        {
            if (SlaveObj.PaxosAggreedProposals.Count == 0) return x1;
            else if (!SlaveObj.PaxosAggreedProposals.Contains(x1)) return x1;
            else
            {
                SlaveObj.PaxosAggreedProposals.Sort();
                Proposal max = SlaveObj.PaxosAggreedProposals[SlaveObj.PaxosAggreedProposals.Count - 1];
                if (x1.DateTime < max.DateTime) return new Proposal(max.DateTime, null);
                else
                {
                    return max;
                }

                //compare x to the highest seq number proposal it has already agreed to, say P2(y, v2)
                //If x < y, reply ‘reject’ along with y
                //If x > y, reply ‘agree’ along with P2(y, v2)
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

     
    }
}
