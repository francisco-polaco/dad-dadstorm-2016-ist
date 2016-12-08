using System;
using System.Collections.Generic;
using System.Reflection;
using CommonTypes;

namespace Slave
{
    [Serializable]
    public class Uniq : Process
    {
        private int _fieldNumber;
        private IList<string> _seenTuples = new List<string>();

        public Uniq(string fieldNumber) 
        {
            _fieldNumber = int.Parse(fieldNumber) - 1;
        }

        public IList<TuplePack> Process(TuplePack input)
        {
            if (_fieldNumber+1 <= input.Content.Count)
            {
                if (!_seenTuples.Contains(input.Content[_fieldNumber]))
                {
                        _seenTuples.Add(input.Content[_fieldNumber]);
                    return new List<TuplePack>() {input};
                }
            }
            return null;
        }
    }

    [Serializable]
    public class Count : Process
    {
        private int _seenTuples = 0;

        private int SeenTuples
        {
            get { return _seenTuples; }
            set { _seenTuples = value; }
        }

        public IList<TuplePack> Process(TuplePack input)
        {
            _seenTuples++;
            List<string> output = new List<string>() {_seenTuples.ToString()};
            return new List<TuplePack>() {new TuplePack(0, null, output)};
        }
    }

    [Serializable]
    public class Dup : Process
    {
        public IList<TuplePack> Process(TuplePack input)
        {
            return new List<TuplePack>() {input};
        }
    }

    [Serializable]
    public class Filter : Process
    {
        private int _fieldNumber;
        private string _condition;
        private string _value;

        public Filter(string fieldNumber, string condition, string value) 
        {
            _fieldNumber = int.Parse(fieldNumber) - 1;
            _condition = condition;
            _value = value;
        }

        public IList<TuplePack> Process(TuplePack input)
        {
            // Check if the index is valid in the list
            int size = _fieldNumber + 1;
            if (!(size <= input.Content.Count))
                return null;

            IList<TuplePack> outputList = new List<TuplePack>() {input};
            switch (_condition)
            {
                case ">": return String.Compare(input.Content[_fieldNumber], _value) > 0 ? outputList : null;
                case "<": return String.Compare(input.Content[_fieldNumber], _value) < 0 ? outputList : null;
                case "=": return String.Compare(input.Content[_fieldNumber], _value) == 0 ? outputList : null;
                default: return null;
            }
        }
    }

    [Serializable]
    public class Custom : Process
    {
        private string _dll;
        private string _invokeClass;
        private string _method;

        public Custom(string dll, string invokeClass, string method)
        {
            _dll = dll;
            _invokeClass = invokeClass;
            _method = method;
        }

        public IList<TuplePack> Process(TuplePack input)
        {
            object output = null;
            try
            {
                // C# Reflection 
                var importDll = Assembly.LoadFile(Environment.CurrentDirectory + @"\..\..\..\Inputs\" + _dll);
                // The substring is used to cut the .dll, since we need the namespace
                string ns = _dll.Substring(0, _dll.IndexOf("."));
                Type type = importDll.GetType(ns + "." + _invokeClass);
                object instance = Activator.CreateInstance(type);
                output = type.InvokeMember(
                    _method,
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.InvokeMethod,
                    null, instance, new object[] {input.Content});
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception at custom operator: " + e.GetBaseException());
            }

            IList<IList<string>> res = (IList<IList<string>>) output;
            IList<TuplePack> outputTuplePacks = new List<TuplePack>();

            if (res != null) { 
                foreach (var var in res)
                {
                    List<string> trimmed = new List<string>();
                    foreach (var str in var)
                    {
                        trimmed.Add(str.Trim());
                    }
                    TuplePack newTuplePack = new TuplePack(0,string.Empty,trimmed);
                    outputTuplePacks.Add(newTuplePack);
                }
            }

            return outputTuplePacks.Count == 0 ?  null : outputTuplePacks;
        }
    }
}
