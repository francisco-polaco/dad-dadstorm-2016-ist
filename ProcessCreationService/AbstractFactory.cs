namespace ProcessCreationService
{
    public abstract class AbstractFactory
    {
        public abstract Import GetImport();
        public abstract Route GetRouting();
        public abstract Process GetProcessing();
    }
}
