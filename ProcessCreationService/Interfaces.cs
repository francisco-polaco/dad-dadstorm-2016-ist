namespace ProcessCreationService
{
    public interface Import
    {
        void import(params object[] specs);
    }

    public interface Route
    {
        void route(params object[] specs);
    }

    public interface Process
    {
        void process(params object[] specs);
    }
}
