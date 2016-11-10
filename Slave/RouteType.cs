using System;
using System.Collections;
using CommonTypes;
using System.Collections.Generic;
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
                    GetDownstreamReplicas(urls)[0].Dispatch(input);
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
                if(urls.Count != 0)
                    GetDownstreamReplicas(urls)[randomInt].Dispatch(input);
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
            string[] content = input.Split(',');
            int hashNumber = (content[fieldID].Trim().GetHashCode())%(content.Length - 1);
            try
            {
                if (urls.Count != 0)
                    GetDownstreamReplicas(urls)[hashNumber].Dispatch(input);
            }
            catch (SocketException)
            {
                Console.WriteLine("Could not locate " + urls[hashNumber]);
            }
        }
    }
}
