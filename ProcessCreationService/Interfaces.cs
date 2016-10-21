using System.Collections;

namespace ProcessCreationService
{
    public interface Import
    {
        void Import(string input);
    }

    public interface Route
    {
        void Route(string input);
    }

    public interface Process
    {
        string Process(string input);
    }
}
