using System;
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
            string[] content = input.Split(',');
            // Since fields in tuple are separated by white spaces, trim them
            string needle = content[fieldNumber].Trim();
            return Regex.Matches(input, needle).Count == 1 ? input : string.Empty;
        }
    }

    [Serializable]
    public class Count : Process
    {
        public string Process(string input)
        {
            return !input.Equals(string.Empty) ? input.Split(',').Length.ToString() : "0";
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
            string[] content = input.Split(',');
            switch (condition)
            {
                case ">": return String.Compare(content[fieldNumber].Trim(), value) > 0 ? input : string.Empty;
                case "<": return String.Compare(content[fieldNumber].Trim(), value) < 0 ? input : string.Empty;
                case "=": return String.Compare(content[fieldNumber].Trim(), value) == 0 ? input : string.Empty;
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
            string[] content = input.Split(',');
            for (int i = 0; i < content.Length; i++)
                content[i] = content[i].Trim();
            
            // C# Reflection 
            var importDLL = Assembly.LoadFile(dll);
            Type type = importDLL.GetType(invokeClass);
            object instance = Activator.CreateInstance(type);
            object output  = type.InvokeMember(method, BindingFlags.InvokeMethod, null, instance, content);

            return output == null ? string.Empty : (string)output;
        }
    }
}
