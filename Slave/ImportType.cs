using System;
using System.Collections.Generic;

namespace Slave
{
    [Serializable]
    public class FileImport : Import
    {
        private string[] filePaths;

        public FileImport(string[] filePaths)
        {
            this.filePaths = filePaths;
        }

        public List<string> Import()
        {
            List<string> output = InputImport();
            return output.Count == 0 ? null : output;
        }

        private List<string> InputImport()
        {
            string tuple;
            List<string> tuples = new List<string>();
            System.IO.StreamReader file;
            foreach (string path in filePaths)
            {
                file = new System.IO.StreamReader(Environment.CurrentDirectory + @"\..\..\..\Inputs\" + path);
                while((tuple = file.ReadLine()) != null)
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
