﻿using System;
using System.Collections.Concurrent;
using CommonTypes;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;

namespace Slave
{
    public delegate void RemoteAsyncDelegate(TuplePack tuple);

    [Serializable]
    public abstract class RouteParent : Route
    {
        private ConcurrentQueue<int> _invalidIndexes = new ConcurrentQueue<int>();
        private string _semantic;

        protected RouteParent(string semantic)
        {
            _semantic = semantic;
        }

        public abstract void Route(TuplePack input);

        /// <summary>
        /// Assumes tcp channel created in Slave, since it is used by it!
        /// </summary>
        /// <param name="urls"></param>
        /// <returns></returns>
        private List<ISlave> GetDownstreamReplicas(List<string> urls)
        {
            List<ISlave> output = new List<ISlave>();
            foreach (var url in urls)
            {
                var replica = (ISlave)Activator.GetObject(typeof(ISlave), url);
                output.Add(replica);
            }
            return output;
        }

        private void WriteToFile(string output)
        {
            lock (this)
            {
                FileInfo fileInfo = new FileInfo(Environment.CurrentDirectory + @"\..\..\..\Output\" + "output.txt");

                if (!fileInfo.Exists && fileInfo.Directory != null)
                    Directory.CreateDirectory(fileInfo.Directory.FullName);
            }

            try
            {
                lock (this)
                {
                    using (StreamWriter file =
                        new StreamWriter(Environment.CurrentDirectory + @"\..\..\..\Output\" + "output.txt", true))
                    {
                        file.WriteLine(output);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }

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

        private void CallNextReplica(int index, List<string> urls, TuplePack inputPack)
        {
            RemoteAsyncDelegate remoteDel = new RemoteAsyncDelegate(GetDownstreamReplicas(urls)[index].Dispatch);
            if (_semantic.Equals("at-most-once"))
                remoteDel.BeginInvoke(inputPack, null, null);
            if (_semantic.Equals("at-most-once"))
            {
                remoteDel.BeginInvoke(
                    inputPack,
                    (IAsyncResult ar) =>
                    {
                        TimerTask tt = new TimerTask(3000,false,TimeoutHandler);
                    },
                    null);
            }
            if(_semantic.Equals("exactly-once")) { 
                remoteDel.BeginInvoke(
                    inputPack,
                    (IAsyncResult ar) =>
                    {
                        try
                        {
                            remoteDel.EndInvoke(ar);
                        }
                        catch (SocketException e)
                        {
                            TryAgain(inputPack, index, urls);
                        }
                    },
                    null);
            }
            else
            {
                Console.WriteLine("I don't support such semantic!");
            }
        }

        private void TryAgain(TuplePack inputPack, int index, List<string> urls)
        {
            Console.WriteLine("Could not locate " + urls[index]);
            // try again dynamic reconfiguration
            if (!_invalidIndexes.Contains(index))
                _invalidIndexes.Enqueue(index);
            if (_invalidIndexes.Count == urls.Count)
            {
                Console.WriteLine("All downstream replicas are down!");
                return;
            }
            for (int i = 0; i < urls.Count; i++)
            {
                if (!_invalidIndexes.Contains(i))
                {
                    CallNextReplica(i, urls, inputPack);
                    return;
                }
            }
        }

        protected void SendTuplePack(int index, List<string> urls, TuplePack inputPack)
        {
            CallNextReplica(index, urls, inputPack);
        }

        protected void WriteTuplePack(IList<string> content)
        {
            WriteToFile(MergeOutput(content));
        }

        private void TimeoutHandler(object sender, EventArgs eventArgs)
        {
            
        }
    }

    [Serializable]
    public class Primary : RouteParent
    {
        private List<string> _urls;

        public Primary(List<string> urls, string semantic) : base(semantic)
        {
            _urls = urls;
        }

        public override void Route(TuplePack input)
        {
            SendTuplePack(0, _urls, input);
        }
    }

    [Serializable]
    public class Random : RouteParent
    {
        private List<string> _urls;

        public Random(List<string> urls, string semantic) : base(semantic)
        {
            _urls = urls;
        }

        public override void Route(TuplePack input)
        {
            System.Random rnd = new System.Random();
            // rnd.Next(replica.Count) - number between [0;replica.count[
            int randomInt = rnd.Next(_urls.Count);
            SendTuplePack(randomInt, _urls, input);
        }
    }

    [Serializable]
    public class Hashing : RouteParent
    {
        private List<string> _urls;
        private int _fieldId;

        public Hashing(List<string> urls, string fieldId, string semantic) : base(semantic)
        {
            _urls = urls;
            _fieldId = int.Parse(fieldId);
        }

        public override void Route(TuplePack input)
        {
            int hashNumber = 0;
            if (_urls.Count != 0)
                hashNumber = (input.Content[_fieldId - 1].GetHashCode()%(_urls.Count));
            SendTuplePack(hashNumber, _urls, input);
        }
    }

    [Serializable]
    public class Output : RouteParent
    {
        public Output(string semantic) : base(semantic)
        {
        }

        public override void Route(TuplePack input)
        {
            WriteTuplePack(input.Content);
        }
    }
}
