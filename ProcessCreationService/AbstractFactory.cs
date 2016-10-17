namespace ProcessCreationService
{
    public abstract class AbstractFactory
    {
        public abstract Import getImport();
        public abstract Route getRouting();
        public abstract Process getProcessing();
    }
}
