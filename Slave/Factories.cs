using CommonTypes;
using System.Collections.Generic;

namespace Slave
{
    public class ImportFactory : AbstractFactory
    {
        /// <summary>
        /// </summary>
        /// <param name="specs">
        /// specs[0] = type of processing operator
        /// filePaths = string[] filePaths
        /// </param>
        /// <returns></returns>
        public override Import GetImport(string[] specs, string[] filePaths)
        {
            string operatorType = specs[0];
            if (operatorType.Equals("FileImport"))
                return new FileImport(filePaths);
            else if (operatorType.Equals("OpImport"))
                return new OpImport();
            return null;
        }

        public override Process GetProcessing(string[] specs)
        {
            return null;
        }

        public override Route GetRouting(string[] specs, List<Replica> replica)
        {
            return null;
        }
    }

    public class ProcessingFactory : AbstractFactory
    {
        public override Import GetImport(string[] specs, string[] filePaths)
        {
            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="specs">
        /// spces[0] = type of processing operator
        /// specs[1] = field_number | dll 
        /// specs[2] = condition | class
        /// specs[3] = value | method 
        /// </param>
        /// <returns></returns>
        public override Process GetProcessing(string[] specs)
        {
            string operatorType = specs[0];
            if (operatorType.Equals("Uniq"))
                return new Uniq(specs[1]);
            else if (operatorType.Equals("Count"))
                return new Count();
            else if (operatorType.Equals("Dup"))
                return new Dup();
            else if (operatorType.Equals("Filter"))
                return new Filter(specs[1], specs[2], specs[3]);
            else if (operatorType.Equals("Custom"))
                return new Custom(specs[1], specs[2], specs[3]);
            else
                return null;
        }

        public override Route GetRouting(string[] specs, List<Replica> replica)
        {
            return null;
        }
    }

    public class RoutingFactory : AbstractFactory
    {
        public override Import GetImport(string[] specs, string[] filePaths)
        {
            return null;
        }

        public override Process GetProcessing(string[] specs)
        {
            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="specs">
        /// specs[0] = type of routing operator
        /// specs[1] = field_id
        /// </param>
        /// <param name="replica">
        /// replica = List of CommonTypes.Replica filled with the downstream replicas
        /// </param>
        /// <returns></returns>
        public override Route GetRouting(string[] specs, List<Replica> replica)
        {
            string operatorType = specs[0];
            if (operatorType.Equals("Primary"))
                return new Primary(replica);
            else if (operatorType.Equals("Random"))
                return new Random(replica);
            else if (operatorType.Equals("Hashing"))
                return new Hashing(replica, specs[1]);
            else
                return null;
        }
    }
}
