using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Slave
{
    [Serializable]
    public class Uniq : Process
    {
        private int fieldNumber;

        public Uniq(string fieldNumber)
        {
            this.fieldNumber = int.Parse(fieldNumber) - 1;
        }

        public string Process(string input)
        {
            if (input.Contains(","))
            {
                string[] content = input.Split(',');
                // Since fields in tuple are separated by white spaces, trim them
                string needle = content[fieldNumber].Trim();
                return Regex.Matches(input, needle).Count == 1 ? input : string.Empty;
            }
            return input;
        }
    }

    [Serializable]
    public class Count : Process
    {
        private int seenTuples = 0;

        public string Process(string input)
        {
            seenTuples++;
            return seenTuples.ToString();
        }
    }

    [Serializable]
    public class Dup : Process
    {
        public string Process(string input)
        {
            return input;
        }
    }

    [Serializable]
    public class Filter : Process
    {
        private int fieldNumber;
        private string condition;
        private string value;

        public Filter(string fieldNumber, string condition, string value)
        {
            this.fieldNumber = int.Parse(fieldNumber) - 1;
            this.condition = condition;
            this.value = value;
        }

        public string Process(string input)
        {
            string toCompare;
            if (!input.Contains(",") && fieldNumber == 0)
            {
                toCompare = input;
            }
            else
            {
                string[] content = input.Split(',');
                toCompare = content[fieldNumber].Trim();
            }
            
            switch (condition)
            {
                case ">": return String.Compare(toCompare, value) > 0 ? input : string.Empty;
                case "<": return String.Compare(toCompare, value) < 0 ? input : string.Empty;
                case "=": return String.Compare(toCompare, value) == 0 ? input : string.Empty;
                default: return string.Empty;
            }
        }
    }

    [Serializable]
    public class Custom : Process
    {
        private string dll;
        private string invokeClass;
        private string method;

        public Custom(string dll, string invokeClass, string method)
        {
            this.dll = dll;
            this.invokeClass = invokeClass;
            this.method = method;
        }

        public string Process(string input)
        {
            // C# Reflection 
            var importDLL = Assembly.LoadFile(Environment.CurrentDirectory +  @"\..\..\..\Inputs\" + dll);
            // The substring is used to cut the .dll, since we need the namespace
            string ns = dll.Substring(0, dll.IndexOf("."));
            Type type = importDLL.GetType(ns + "." + invokeClass);
            object instance = Activator.CreateInstance(type);
               
            object output = type.InvokeMember(
                                            method, 
                                            BindingFlags.Public | 
                                            BindingFlags.Instance | 
                                            BindingFlags.InvokeMethod, 
                                            null, instance, new object[] {input});

            return (string)output;
        }
    }
}
