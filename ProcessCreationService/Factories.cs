namespace ProcessCreationService
{
    public class ImportFactory : AbstractFactory
    {
        public override Import GetImport()
        {
            // TO DO
            return null;
        }

        public override Process GetProcessing()
        {
            return null;
        }

        public override Route GetRouting()
        {
            return null;
        }
    }

    public class ProcessingFactory : AbstractFactory
    {
        public override Import GetImport()
        {
            return null;
        }

        public override Process GetProcessing()
        {
            // TO DO
            return null;
        }

        public override Route GetRouting()
        {
            return null;
        }
    }

    public class RoutingFactory : AbstractFactory
    {
        public override Import GetImport()
        {
            return null;
        }

        public override Process GetProcessing()
        {
            return null;
        }

        public override Route GetRouting()
        {
            //TO DO
            return null;
        }
    }
}
