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
        /// inputs = string[] inputs
        /// filePaths = string[] filePaths
        /// </param>
        /// <returns></returns>
        public override Import GetImport(string[] specs, string[] inputs, string[] filePaths)
        {
            string operatorType = specs[0];
            if (operatorType.ToLower().Equals("input"))
                return new Input(inputs);
            if (operatorType.ToLower().Equals("fileimport"))
                return new FileImport(filePaths);
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
        public override Import GetImport(string[] specs, string[] inputs, string[] filePaths)
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
        public override Import GetImport(string[] specs, string[] inputs, string[] filePaths)
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
                return new Output(null);
            return null;
        }
    }
}
