namespace ProcessCreationService
{
    public class Uniq : Process
    {
        /// <summary>
        /// Emit the tuple again if field_number is unique
        /// Not unique return empty string 
        /// </summary>
        /// <param name="specs"></param>
        public string Process(params object[] specs)
        {
            string inputTuple = (string)specs[0];
            string fieldNumber = (string)specs[1];
            return !inputTuple.Contains(fieldNumber) ? inputTuple : string.Empty;
        }
    }

    public class Count : Process
    {
        public string Process(params object[] specs)
        {
            // TO DO
            return string.Empty;
        }
    }

    public class Dup : Process
    {
        public string Process(params object[] specs)
        {
            // TO DO
            return string.Empty;
        }
    }

    public class Filter : Process
    {
        public string Process(params object[] specs)
        {
            // TO DO
            return string.Empty;
        }
    }

    public class Custom : Process
    {
        public string Process(params object[] specs)
        {
            // TO DO
            return string.Empty;
        }
    }
}
