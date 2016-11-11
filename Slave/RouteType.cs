using System;
using System.Collections;
using CommonTypes;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace Slave
{
    [Serializable]
    public abstract class RouteParent : Route
    {
        public abstract void Route(string input);

        /// <summary>
        /// Assumes tcp channel created in Slave, since it is used by it!
        /// </summary>
        /// <param name="urls"></param>
        /// <returns></returns>
        public List<ISlave> GetDownstreamReplicas(List<string> urls)
        {
            List<ISlave> output = new List<ISlave>();
            foreach (var url in urls)
            {
                var replica = (ISlave)Activator.GetObject(typeof(ISlave), url);
                output.Add(replica);
            }
            return output;
        }

        public void WriteToFile(string output)
        {
            FileInfo fileInfo = new FileInfo(Environment.CurrentDirectory + @"\..\..\..\Output\" + "output.txt");

            if (!fileInfo.Exists && fileInfo.Directory != null)
                Directory.CreateDirectory(fileInfo.Directory.FullName);

            File.WriteAllText(Environment.CurrentDirectory + @"\..\..\..\Output\" + "output.txt", string.Empty);

            try
            {
                using (StreamWriter file =
                    new StreamWriter(Environment.CurrentDirectory + @"\..\..\..\Output\" + "output.txt", true))
                {
                    file.WriteLine(output);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }
        }

    }

    [Serializable]
    public class Primary : RouteParent, Route
    {
        private List<string> urls;

        public Primary(List<string> urls)
        {
            this.urls = urls;
        }

        public override void Route(string input)
        {
            try
            {
                if (urls.Count != 0)
                {
                    RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(GetDownstreamReplicas(urls)[0].Dispatch);
                    //AsyncCallback RemoteCallback = new AsyncCallback(this.OnRemoteCallback);
                    // Call remote method
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(input, null, null);
                }
                else
                {
                    WriteToFile(input);
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Could not locate " + urls[0]);
            }
            
        }
    }

    [Serializable]
    public class Random : RouteParent, Route
    {
        private List<string> urls;

        public Random(List<string> urls)
        {
            this.urls = urls;
        }

        public override void Route(string input)
        {
            System.Random rnd = new System.Random();
            // rnd.Next(replica.Count) - number between [0;replica.count[
            int randomInt = rnd.Next(urls.Count);
            try
            {
                if (urls.Count != 0)
                {
                    RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(GetDownstreamReplicas(urls)[randomInt].Dispatch);
                    //AsyncCallback RemoteCallback = new AsyncCallback(this.OnRemoteCallback);
                    // Call remote method
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(input, null, null);
                }
                else
                {
                    WriteToFile(input);
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Could not locate " + urls[randomInt]);
            }
        }
    }

    [Serializable]
    public class Hashing : RouteParent, Route
    {
        private List<string> urls;
        private int fieldID;

        public Hashing(List<string> urls, string fieldID)
        {
            this.urls = urls;
            this.fieldID = int.Parse(fieldID);
        }

        public override void Route(string input)
        {
            int hashNumber = 0;
            if (input.Contains(","))
            {
                string[] content = input.Split(',');
                if(urls.Count != 0)
                    hashNumber = (content[fieldID].Trim().GetHashCode())%(urls.Count);
                else
                {
                    WriteToFile(input);
                }
            }

            try
            {
                if (urls.Count != 0)
                {
                    RemoteAsyncDelegate RemoteDel = new RemoteAsyncDelegate(GetDownstreamReplicas(urls)[hashNumber].Dispatch);
                    //AsyncCallback RemoteCallback = new AsyncCallback(this.OnRemoteCallback);
                    // Call remote method
                    IAsyncResult RemAr = RemoteDel.BeginInvoke(input, null, null);
                }
            }
            catch (SocketException)
            {
                Console.WriteLine("Could not locate " + urls[hashNumber]);
            }
        }
    }

    public delegate void RemoteAsyncDelegate(string tuple);
}
