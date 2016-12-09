using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Slave
{
    [Serializable]
    public class FileImport : Import
    {
        private string[] _filePaths;
        private int _field;
        private int _replicaNumber;
        private int _total;
        private string _routing;
        private System.Random _rnd = new System.Random();

        public FileImport(string[] filePaths, string routing, string field, int replicaNumber, int total)
        { 
            // index purposes
            _field = Int32.Parse(field) - 1;
            _filePaths = filePaths;
            _routing = routing;
            _replicaNumber = replicaNumber;
            _total = total;
        }

        public IList<Dictionary<int, string>> Import()
        {
            return DecideMyLines(InputImport(_filePaths));
        }
 
        private IList<Dictionary<int, string>> DecideMyLines(Dictionary<int, string> input)
        {
            Dictionary<int,string> myLines = new Dictionary<int, string>();
            Dictionary<int,string> siblingsLines = new Dictionary<int, string>();

            int index;
            foreach (KeyValuePair<int,string> pair in input)
            {
                if (_routing.ToLower().Equals("random"))
                {
                    // rnd.Next(replica.Count) - number between [0;urls.count[
                    index = _rnd.Next(0,_total);
                }
                else if (_routing.ToLower().Equals("hashing"))
                {
                    string pattern = @",|\s";
                    string[] tupleFields = Regex.Split(pair.Value, pattern).Where(s => s != String.Empty).ToArray<string>();
                    index = _total != 0 ? tupleFields[_field].GetHashCode() % _total : 0;
                    // modulus operation
                    if (index < 0)
                        index += _total;
                }
                else
                    index = 0;

                if(index == _replicaNumber)
                    myLines.Add(pair.Key, pair.Value);
                else 
                    siblingsLines.Add(pair.Key,pair.Value);
            }
            return new List<Dictionary<int, string>>() {myLines, siblingsLines};
        } 

        private Dictionary<int, string> InputImport(string[] filePaths)
        {
            string tuple;
            Dictionary<int,string> tuples = new Dictionary<int, string>();
            System.IO.StreamReader file;
            foreach (string path in filePaths)
            {
                if(path.Contains(":"))
                    file = new System.IO.StreamReader(path);
                else
                    file = new System.IO.StreamReader(Environment.CurrentDirectory + @"\..\..\..\Inputs\" + path);
                int i = 0;
                while ((tuple = file.ReadLine()) != null)
                {
                    if (tuple.StartsWith("%%"))
                        continue;
                    tuples[i++] = tuple;
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
        public IList<Dictionary<int, string>> Import()
        {
            return null;
        }
    }
}
