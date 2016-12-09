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
            bool _stateful = false;
            bool firstOP = false;
            string cmd = input.Cmd;
            List<string> urls = input.ListUrls; 
            List<string> downstreamUrls = input.ReplicaUrlsOutput;

            // in order to make a custom import for the first operator
            if (input.RoutingTypeToReadFromFile != null)
                firstOP = true;

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
            for (int i = 0; i < importTokens.Length; i++)
            {
                if (importTokens[i].StartsWith("OP")) // input comes from operator
                {
                    importObj = importFactory.GetImport(new string[] { "OpImport" }, null, 0, 0);
                    break; // assuming only one operator
                }
                if (importTokens[i].Contains(".")) { // input comes from file
                    filePathsList.Add(importTokens[i]);
                }
                else
                    Console.WriteLine("Neither operator nor input file!!!");
            }

            /*** create routing object ***/
            AbstractFactory routingFactory = new RoutingFactory();
            Route routeObj;

            // tokenize routing policy
            string[] routingTokens;
            string routingPattern = @"[)(\s]";

            if (input.RoutingType != null)
                routingTokens = Regex.Split(input.RoutingType, routingPattern).Where(s => s != String.Empty).ToArray<string>();
            else
                routingTokens = Regex.Split("Output", routingPattern).Where(s => s != String.Empty).ToArray<string>();

            routeObj = routingFactory.GetRouting(routingTokens, downstreamUrls, input.Semantic.ToLower());

            /*** create processing object ***/
            AbstractFactory processingFactory = new ProcessingFactory();
            Process processObj;

            // tokenize processing function
            string processingPattern = @",|\s";
            string[] processingTokens = Regex.Split(tokens[5], processingPattern).Where(s => s != String.Empty).ToArray<string>();

            // if it's a count operator or a uniq - it needs state - for exactly-once
            _stateful = Stateful(processingTokens[0]);

            processObj = processingFactory.GetProcessing(processingTokens);

            string[] plainUrls = urls.ToArray().Where(s => s != String.Empty).ToArray<string>();

            bool wasNull = importObj == null;
            foreach (string url in plainUrls) {
                if(wasNull)
                {
                    // tokenize routing policy for the first operator
                    string[] merge = firstOP ? FileImportRouting(input.RoutingTypeToReadFromFile) : FileImportRouting(input.RoutingType);
                    // supports both import in the beginning or in the middle
                    if (input.RoutingType != null)
                        importObj = importFactory.GetImport(merge, filePathsList.ToArray(),
                            plainUrls.ToList().IndexOf(url), plainUrls.Length);
                    else
                        importObj = importFactory.GetImport(new string[] {"FileImport", "primary"}, filePathsList.ToArray(), plainUrls.ToList().IndexOf(url), plainUrls.Length);
                }

                System.Diagnostics.Process.Start(@"Slave.exe", SerializeObject(importObj) + " " + SerializeObject(routeObj) + " " +
                    SerializeObject(processObj) + " " + SerializeObject(url) + " " + SerializeObject(input.PuppetMasterUrl) + " " +
                    SerializeObject(input.IsLogFull) + " " + SerializeObject(input.Semantic.ToLower()) + " " + 
                    SerializeObject(getSiblings(plainUrls,url)) + " " + SerializeObject(_stateful));
 
            }
        }

        private bool Stateful(string processingToken)
        {
            if (processingToken.ToLower().Equals("uniq") || processingToken.ToLower().Equals("count"))
                return true;
            return false;
        }

        private string[] FileImportRouting(string routing)
        {
            string rp = @"[)(\s]";
            string[] rt = Regex.Split(routing, rp).Where(s => s != String.Empty).ToArray<string>();
            string[] merge = new string[rt.Length + 1];

            merge[0] = "FileImport";
            for (int i = 0; i < rt.Length; i++)
                merge[i+1] = rt[i];

            return merge;
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

        private List<string> getSiblings(string[] opReplicasUrls, string myself)
        {
            List<string> output = new List<string>();
            foreach (string url in opReplicasUrls)
            {
                if(url.Equals(myself))
                    continue;
                // the url of ISibling finishes op + s
                output.Add(url);
            }
            return output;
        }
    }
}
