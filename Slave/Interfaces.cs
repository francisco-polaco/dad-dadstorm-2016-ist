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
    }

    public interface Process
    {
        IList<TuplePack> Process(TuplePack input);
    }
}
