using System;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using Slave;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace ProcessCreationService
{
    public class ExecEnv
    {
        static void Main(string[] args)
        {
            //Console.Write("Insert the Process Creation Service name: ");
            ProcessCreationService pcs = new ProcessCreationService();
            Console.WriteLine("<Enter to exit> ");
            Console.ReadLine();    
        }
    }

    public class ProcessCreationService : MarshalByRefObject, IPcsSlaveLaunch
    {

        public ProcessCreationService()
        {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this, "pcs", typeof(ProcessCreationService));
            //Clearing possible output files
            File.WriteAllText(Environment.CurrentDirectory + @"\..\..\..\Output\" + "output.txt", string.Empty); 
            Console.WriteLine("Process Creation Service created!");
        }

        public void Launch(ConnectionPack input)
        {
            bool firstOP = false;
            string cmd = input.Cmd;
            List<string> urls = input.ListUrls; 
            List<string> downstreamUrls = input.ReplicaUrlsOutput;

            // split command by keywords
            string pattern = @"INPUT OPS|REP FACT|ROUTING|ADDRESS|OPERATOR SPEC";
            string[] tokens = Regex.Split(cmd, pattern, RegexOptions.IgnoreCase).Where(s => s != String.Empty).ToArray<string>();

            // splitting by 5 keywords should generate 6 tokens
            if (tokens.Length != 6)
                System.Console.WriteLine("Something went wrong while splitting the command!!!");

            // tokens[0] -> operator name
            // tokens[1] -> input file or previous operator
            // tokens[2] -> replication factor
            // tokens[3] -> routing policy
            // tokens[4] -> list of slave's URLs
            // tokens[5] -> name of the transformation function (and possibly parameters)

            /*** create import object ***/
            AbstractFactory importFactory = new ImportFactory();
            Import importObj = null;

            // tokenize input
            string importPattern = @",|\s";
            string[] importTokens = Regex.Split(tokens[1], importPattern).Where(s => s != String.Empty).ToArray<string>();

            // list to collect possible file paths
            List<string> filePathsList = new List<string>();

            for (int i = 0; i < importTokens.Length; i++) { 
                if (importTokens[i].StartsWith("OP")) // input comes from operator
                {
                    importObj = importFactory.GetImport(new string[] { "OpImport" }, null);
                    break; // assuming only one operator
                }
                else
                {
                    if (importTokens[i].Contains(".")) { // input comes from file
                        filePathsList.Add(importTokens[i]);
                        firstOP = true;
                    }
                    else
                        System.Console.WriteLine("Neither operator nor input file!!!");
                }
            }

            /*** create routing object ***/
            AbstractFactory routingFactory = new RoutingFactory();
            Route routeObj;

            // tokenize routing policy
            string routingPattern = @"[)(\s]";
            string[] routingTokens = Regex.Split(tokens[3], routingPattern).Where(s => s != String.Empty).ToArray<string>();

            routeObj = routingFactory.GetRouting(routingTokens, downstreamUrls);

            /*** create processing object ***/
            AbstractFactory processingFactory = new ProcessingFactory();
            Process processObj;

            // tokenize processing function
            string processingPattern = @",|\s";
            string[] processingTokens = Regex.Split(tokens[5], processingPattern).Where(s => s != String.Empty).ToArray<string>();

            processObj = processingFactory.GetProcessing(processingTokens);

            /*** creating the slaves ***/
            //string urlPattern = @",|\s";
            //string[] urlTokens = Regex.Split(tokens[4], urlPattern).Where(s => s != String.Empty).ToArray<string>();

            // remove empty strings from urls list
            string[] plain_urls = urls.ToArray().Where(s => s != String.Empty).ToArray<string>();

            IDictionary<string, List<string>> tuplesInput = null;
            if (firstOP)
            {
                tuplesInput = ProcessInputFiles(input.RoutingTypeToReadFromFile, urls, filePathsList.ToArray());
            }

            foreach (string url in plain_urls) {
                if (firstOP)
                {
                    importObj = importFactory.GetImport(new string[] { "FileImport" }, tuplesInput[url].ToArray());
                }
                System.Diagnostics.Process.Start(@"Slave.exe", SerializeObject(importObj) + " " + SerializeObject(routeObj) + " " +
                    SerializeObject(processObj) + " " + SerializeObject(url) + " " + SerializeObject(input.PuppetMasterUrl) + " " +
                    SerializeObject(input.IsLogFull));
            }
        }

        private string SerializeObject(object o)
        {
            if (!o.GetType().IsSerializable)
            {
                return null;
            }

            using (MemoryStream stream = new MemoryStream())
            {
                new BinaryFormatter().Serialize(stream, o);
                return Convert.ToBase64String(stream.ToArray());
            }
        }

        /// <summary>
        /// Function used to simulate the pseudo-first operator of importing
        /// </summary>
        /// <param name="routeType"></param>
        /// <param name="urls"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        private IDictionary<string, List<string>> ProcessInputFiles(string routeType, List<string> urls, string[] paths)
        {
            List<string> input = InputImport(paths);
            IDictionary<string, List<string>> output = new Dictionary<string, List<string>>();

            // tokenize routing policy
            string routingPattern = @"[)(\s]";
            string[] routingTokens = Regex.Split(routeType, routingPattern).Where(s => s != String.Empty).ToArray<string>();

            foreach (var url in urls)
            {
                KeyValuePair<string, List<string>> pair = new KeyValuePair<string, List<string>>(url, new List<string>());
                output.Add(pair);
            }

            int index;
            System.Random rnd = new System.Random();
            foreach (string tuple in input)
            {
                if (routingTokens[0].Equals("random"))
                {
                    // rnd.Next(replica.Count) - number between [0;urls.count[
                    index = rnd.Next(urls.Count);
                }
                else if (routingTokens[0].Equals("hashing"))
                {
                    index = tuple.GetHashCode() % urls.Count;
                    if (index < 0)
                        index += urls.Count;
                }
                else
                {
                    index = 0;
                }
                output[urls[index]].Add(tuple);
            }
            return output;
        }

        private List<string> InputImport(string[] filePaths)
        {
            string tuple;
            List<string> tuples = new List<string>();
            System.IO.StreamReader file;
            foreach (string path in filePaths)
            {
                file = new System.IO.StreamReader(Environment.CurrentDirectory + @"\..\..\..\Inputs\" + path);
                while ((tuple = file.ReadLine()) != null)
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
}
