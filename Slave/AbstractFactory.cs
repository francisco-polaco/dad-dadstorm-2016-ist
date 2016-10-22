namespace Slave
{
    public abstract class AbstractFactory
    {
        public abstract Import GetImport(string[] specs);
        public abstract Route GetRouting(string[] specs);
        public abstract Process GetProcessing(string[] specs);
    }
}
