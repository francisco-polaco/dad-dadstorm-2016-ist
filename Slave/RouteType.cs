using System;
using CommonTypes;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Slave
{
    public delegate void RemoteAsyncDelegate(TuplePack tuple);

    [Serializable]
    public abstract class RouteParent : Route
    {
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
            IAsyncResult remAr = remoteDel.BeginInvoke(inputPack, null, null);
        }

        protected void SendTuplePack(int index, List<string> urls, TuplePack inputPack)
        {
            try
            {
                CallNextReplica(index, urls, inputPack);
            }
            catch (SocketException)
            {
                Console.WriteLine("Could not locate " + urls[index]);
            }
        }

        protected void WriteTuplePack(IList<string> content)
        {
            WriteToFile(MergeOutput(content));
        }
    }

    [Serializable]
    public class Primary : RouteParent
    {
        private List<string> _urls;

        public Primary(List<string> urls)
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

        public Random(List<string> urls)
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

        public Hashing(List<string> urls, string fieldId)
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
        public override void Route(TuplePack input)
        {
            WriteTuplePack(input.Content);
        }
    }
}
