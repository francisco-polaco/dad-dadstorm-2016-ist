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
            if (operatorType.ToLower().Equals("fileimport"))
                return new FileImport(filePaths);
            else if (operatorType.ToLower().Equals("opimport"))
                return new OpImport();
            return null;
        }

        public override Process GetProcessing(string[] specs)
        {
            return null;
        }

        public override Route GetRouting(string[] specs, List<string> urls)
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
            if (operatorType.ToLower().Equals("uniq"))
                return new Uniq(specs[1]);
            else if (operatorType.ToLower().Equals("count"))
                return new Count();
            else if (operatorType.ToLower().Equals("dup"))
                return new Dup();
            else if (operatorType.ToLower().Equals("filter"))
                return new Filter(specs[1], specs[2], specs[3]);
            else if (operatorType.ToLower().Equals("custom"))
                return new Custom(specs[1], specs[2], specs[3]);
            else
                return null;
        }

        public override Route GetRouting(string[] specs, List<string> urls)
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
        public override Route GetRouting(string[] specs, List<string> urls)
        {
            string operatorType = specs[0];
            if (operatorType.ToLower().Equals("primary"))
                return new Primary(urls);
            else if (operatorType.ToLower().Equals("random"))
                return new Random(urls);
            else if (operatorType.ToLower().Equals("hashing"))
                return new Hashing(urls, specs[1]);
            else
                return null;
        }
    }
}
