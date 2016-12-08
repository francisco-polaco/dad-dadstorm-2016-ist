using System.Collections.Generic;
using CommonTypes;

namespace Slave
{
    public interface Import
    {
        List<string> Import();
    }

    public interface Route
    {
        void Route(TuplePack input);
        bool IsLast();
    }

    public interface Process
    {
        IList<TuplePack> Process(TuplePack input);
        Process Clone();
    }
}
