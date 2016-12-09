using System.Collections.Generic;

namespace Slave
{
    public class ImportFactory : AbstractFactory
    {
        /// <summary>
        /// </summary>
        /// <param name="specs">
        ///     specs[0] = type of processing operator
        ///     specs[1] = routing type
        ///     specs[2] = field
        ///     filePaths = string[] filePaths
        ///     replicaNumber && total = used to route for the first operator
        /// </param>
        /// <param name="filePaths"></param>
        /// <param name="replicaNumber"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public override Import GetImport(string[] specs, string[] filePaths, int replicaNumber, int total)
        {
            string operatorType = specs[0];
            if (operatorType.ToLower().Equals("fileimport"))
                return specs.Length == 3 ? new FileImport(filePaths, specs[1], specs[2], replicaNumber, total) : new FileImport(filePaths, specs[1], "0", replicaNumber, total);
            if (operatorType.ToLower().Equals("opimport"))
                return new OpImport();
            return null;
        }

        public override Process GetProcessing(string[] specs)
        {
            return null;
        }

        public override Route GetRouting(string[] specs, List<string> urls, string semantic)
        {
            return null;
        }
    }

    public class ProcessingFactory : AbstractFactory
    {
        public override Import GetImport(string[] specs, string[] filePaths, int replicaNumber, int total)
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
            if (operatorType.ToLower().Equals("count"))
                return new Count();
            if (operatorType.ToLower().Equals("dup"))
                return new Dup();
            if (operatorType.ToLower().Equals("filter"))
                return new Filter(specs[1], specs[2], specs[3]);
            if (operatorType.ToLower().Equals("custom"))
                return new Custom(specs[1], specs[2], specs[3]);
            return null;
        }

        public override Route GetRouting(string[] specs, List<string> urls, string semantic)
        {
            return null;
        }
    }

    public class RoutingFactory : AbstractFactory
    {
        public override Import GetImport(string[] specs, string[] filePaths, int replicaNumber, int total)
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
        /// <param name="urls">
        /// urls = downstream replicas urls
        /// </param>
        /// <param name="semantic"></param>
        /// <returns></returns>
        public override Route GetRouting(string[] specs, List<string> urls, string semantic)
        {
            string operatorType = specs[0];
            if (operatorType.ToLower().Equals("primary"))
                return new Primary(urls, semantic);
            if (operatorType.ToLower().Equals("random"))
                return new Random(urls, semantic);
            if (operatorType.ToLower().Equals("hashing"))
                return new Hashing(urls, specs[1], semantic);
            if (operatorType.ToLower().Equals("output"))
                return new Output(semantic);
            return null;
        }
    }
}
