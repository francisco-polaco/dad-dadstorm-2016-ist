namespace mylib
{
    public class QueryFollowersFile
    {
        public string getFollowers(string input)
        {
            string tuple;

            string[] content = input.Split(',');
            string user = string.Empty;

            foreach (string s in content)
            {
                var variable = s.Trim();
                if (variable.StartsWith("user"))
                    user = variable;
            }
        
            System.IO.StreamReader file = new System.IO.StreamReader(@"..\..\..\Inputs\followers.dat");
            while ((tuple = file.ReadLine()) != null)
            {
                if (tuple.StartsWith("%%"))
                    continue;
                if (tuple.StartsWith(user))
                    return tuple;
            }
            file.Close();

            return string.Empty;
        }
    }
}
