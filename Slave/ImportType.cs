using System.Collections.Generic;

namespace Slave
{
    public class FileImport : Import
    {
        private string[] filePaths;

        public FileImport(string[] filePaths)
        {
            this.filePaths = filePaths;
        }

        public List<string> Import()
        {
            List<string> output = inputImport();
            return output.Count == 0 ? null : output;
        }

        private List<string> inputImport()
        {
            string tuple;
            List<string> tuples = new List<string>();
            System.IO.StreamReader file;
            foreach (string path in filePaths)
            {
                file = new System.IO.StreamReader(@path);
                while((tuple = file.ReadLine()) != null)
                {
                    if (tuple.StartsWith("%%"))
                        continue;
                    tuples.Add(tuple);
                }
                file.Close();
                tuple = null;
            }
            return tuples;
        }
        

    }

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
