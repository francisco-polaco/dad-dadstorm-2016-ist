using System;
using System.Collections.Generic;
using System.Linq;

namespace Slave
{
    [Serializable]
    public class FileImport : Import
    {
        private string[] inputs;

        public FileImport(string[] inputs)
        {
            this.inputs = inputs;
        }

        public List<string> Import()
        {
            return inputs.ToList().Count == 0 ? null : inputs.ToList();
        }
    }

    [Serializable]
    public class OpImport : Import
    {
        /// <summary>
        /// Waits to be dispatched by the other operators, when they route! Therefore 
        /// it returns a null List!
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public List<string> Import()
        {
            return null;
        }
    }
}
