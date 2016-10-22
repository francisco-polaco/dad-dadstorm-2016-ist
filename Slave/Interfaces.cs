using System.Collections;

namespace Slave
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
        /// <summary>
        /// </summary>
        /// <param name="input"></param>
        /// <returns>
        /// Empty string is returned when the logic of the operator isn't fulfilled!
        /// </returns>
        string Process(string input);
    }
}
