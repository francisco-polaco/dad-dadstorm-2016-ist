using CommonTypes;
using System.Collections.Generic;

namespace Slave
{
    public class Primary : Route
    {
        private List<Replica> replica;

        public Primary(List<Replica> replica)
        {
            this.replica = replica;
        }

        public void Route(string input)
        {
            replica[0].Proxy.Dispatch(input);
        }
    }

    public class Random : Route
    {
        private List<Replica> replica;

        public Random(List<Replica> replica)
        {
            this.replica = replica;
        }

        public void Route(string input)
        {
            System.Random rnd = new System.Random();
            // rnd.Next(replica.Count) - number between [0;replica.count[
            int randomInt = rnd.Next(replica.Count);
            replica[randomInt].Proxy.Dispatch(input);
        }
    }

    public class Hashing : Route
    {
        private List<Replica> replica;
        private int fieldID;

        public Hashing(List<Replica> replica, string fieldID)
        {
            this.replica = replica;
            this.fieldID = int.Parse(fieldID);
        }

        public void Route(string input)
        {
            string[] content = input.Split(',');
            int hashNumber = content[fieldID].GetHashCode();
            replica[hashNumber].Proxy.Dispatch(input);
        }
    }
}
