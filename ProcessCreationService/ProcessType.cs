using System;
using System.Text.RegularExpressions;

namespace ProcessCreationService
{
    public class Uniq : Process
    {
        private int fieldNumber;

        public Uniq(string fieldNumber)
        {
            init(fieldNumber);
        }

        private void init(string fieldNumber)
        {
            int result;
            if (int.TryParse(fieldNumber, out result))
                this.fieldNumber = result;
            // TO DO
            // else : throw exception conversion not possible
        }

        public string Process(string input)
        {
            string[] content = input.Split(',');
            // TO DO
            // Exception custom?
            if (fieldNumber >= content.Length)
                throw new IndexOutOfRangeException();

            // Corta o espaço vazio, visto que os fields estão separados por espaços
            string needle = content[fieldNumber].Trim();
            return Regex.Matches(input, needle).Count == 1 ? input : string.Empty;
        }
    }

    public class Count : Process
    {
        public string Process(string input)
        {
            return !input.Equals(string.Empty) ? input.Split(',').Length.ToString() : "0";
        }
    }

    public class Dup : Process
    {
        public string Process(string input)
        {
            return input;
        }
    }

    public class Filter : Process
    {
        private string fieldNumber;
        private string condition;
        private string value;

        public Filter(string fieldNumber, string condition, string value)
        {
            this.fieldNumber = fieldNumber;
            this.condition = condition;
            this.value = value;
        }

        public string Process(string input)
        {
            // TO DO
            return string.Empty;
        }
    }

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
            // TO DO
            return string.Empty;
        }
    }
}
