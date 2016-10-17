namespace ProcessCreationService
{
    public class ImportFactory : AbstractFactory
    {
        public override Import getImport()
        {
            // TO DO
            return null;
        }

        public override Process getProcessing()
        {
            return null;
        }

        public override Route getRouting()
        {
            return null;
        }
    }

    public class ProcessingFactory : AbstractFactory
    {
        public override Import getImport()
        {
            return null;
        }

        public override Process getProcessing()
        {
            // TO DO
            return null;
        }

        public override Route getRouting()
        {
            return null;
        }
    }

    public class RoutingFactory : AbstractFactory
    {
        public override Import getImport()
        {
            return null;
        }

        public override Process getProcessing()
        {
            return null;
        }

        public override Route getRouting()
        {
            //TO DO
            return null;
        }
    }
}
