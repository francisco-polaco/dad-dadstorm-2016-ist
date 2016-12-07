using System.Collections.Generic;

namespace mylib
{
    public class QueryFollowersFile
    {
        private static bool dictLoaded = false;
        private static Dictionary<string, List<string>> dictTweeterFollower = new Dictionary<string, List<string>>();

        private static void LoadFollowersDict()
        {
            string followersFilepath = @"..\..\..\Inputs\followers.dat";

            System.IO.StreamReader followersFile =
            new System.IO.StreamReader(followersFilepath, true);
            string line = followersFile.ReadLine();
            List<string> followers;
            while (line != null)
            {
                if (line[0] != '%')
                {
                    string[] tokens = line.Split(',');
                    followers = new List<string>();
                    if (dictTweeterFollower.ContainsKey(tokens[0]))
                    {
                        followers = dictTweeterFollower[tokens[0]];
                    }
                    for (int i = 1; i < tokens.Length; i++)
                    {
                        followers.Add(tokens[i]);
                    }
                    if (!dictTweeterFollower.ContainsKey(tokens[0]))
                    {
                        dictTweeterFollower.Add(tokens[0], followers);
                    }
                }
                line = followersFile.ReadLine();
            }
            dictLoaded = true;
        }


        public IList<IList<string>> getFollowers(IList<string> inputTuple)
        {
            inputTuple[0] = inputTuple[1];

            IList<IList<string>> outputTuples = new List<IList<string>>();
            List<string> tuple;

            if (!dictLoaded) LoadFollowersDict();
            if (dictTweeterFollower.ContainsKey(inputTuple[0]))
            {
                foreach (string follower in dictTweeterFollower[inputTuple[0]])
                {
                    tuple = new List<string>();
                    tuple.Add(follower);
                    outputTuples.Add(tuple);
                }
            }
            return outputTuples;
        }
    }
}
