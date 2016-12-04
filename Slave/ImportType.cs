using System;
using System.Collections.Generic;
using System.Linq;

namespace Slave
{
    [Serializable]
    public class Input : Import
    {
        private string[] _inputs;
        private bool _used = false;

        public Input(string[] inputs)
        {
            _inputs = inputs;
        }

        public List<string> Import()
        {
            // the inputs can only be used once
            if (!_used) { 
                _used = true;
                return _inputs.ToList().Count == 0 ? null : _inputs.ToList();
            }
            return null;
        }
    }

    [Serializable]
    public class FileImport : Import
    {
        private string[] _filePaths;

        public FileImport(string[] filePaths)
        {
            _filePaths = filePaths;
        }

        public List<string> Import()
        {
            return InputImport(_filePaths);
        }

        private List<string> InputImport(string[] filePaths)
        {
            string tuple;
            List<string> tuples = new List<string>();
            System.IO.StreamReader file;
            foreach (string path in filePaths)
            {
                file = new System.IO.StreamReader(Environment.CurrentDirectory + @"\..\..\..\Inputs\" + path);
                while ((tuple = file.ReadLine()) != null)
                {
                    if (tuple.StartsWith("%%"))
                        continue;
                    tuples.Add(tuple);
                }
                file.Close();
            }
            return tuples;
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
