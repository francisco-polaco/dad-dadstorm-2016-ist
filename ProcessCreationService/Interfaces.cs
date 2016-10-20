namespace ProcessCreationService
{
    public interface Import
    {
        void Import(params object[] specs);
    }

    public interface Route
    {
        void Route(params object[] specs);
    }

    public interface Process
    {
        string Process(params object[] specs);
    }
}
