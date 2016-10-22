namespace Slave
{
    public class ImportFactory : AbstractFactory
    {
        public override Import GetImport(string[] specs)
        {
            // TO DO
            return null;
        }

        public override Process GetProcessing(string[] specs)
        {
            return null;
        }

        public override Route GetRouting(string[] specs)
        {
            return null;
        }
    }

    public class ProcessingFactory : AbstractFactory
    {
        public override Import GetImport(string[] specs)
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

        public override Route GetRouting(string[] specs)
        {
            return null;
        }
    }

    public class RoutingFactory : AbstractFactory
    {
        public override Import GetImport(string[] specs)
        {
            return null;
        }

        public override Process GetProcessing(string[] specs)
        {
            return null;
        }

        public override Route GetRouting(string[] specs)
        {
            //TO DO
            return null;
        }
    }
}
