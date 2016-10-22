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
            // TO DO
        }
    }

    public class Random : Route
    {
        public void Route(string input)
        {
            // TO DO
        }
    }

    public class Hashing : Route
    {
        public void Route(string input)
        {
            // TO DO
        }
    }
}
