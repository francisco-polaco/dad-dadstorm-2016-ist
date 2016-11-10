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
            ProcessCreationService pcs = new ProcessCreationService(/*Console.ReadLine()*/);
            Console.WriteLine("<Enter to exit> ");
            Console.ReadLine();    
        }
    }

    public class ProcessCreationService : MarshalByRefObject, IPcsSlaveLaunch
    {

        public ProcessCreationService(/*string name*/)
        {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, false);
            RemotingServices.Marshal(this, "pcs", typeof(ProcessCreationService));
            Console.WriteLine("Process Creation Service " /*+ name +*/ +  " created!");
        }

        public void Launch(ConnectionPack input)
        {
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

            for (int i = 0; i < importTokens.Length; i++)
                if (importTokens[i].StartsWith("OP")) // input comes from operator
                {
                    importObj = importFactory.GetImport(new string[] { "OpImport" }, null);
                    break; // assuming only one operator
                }
                else
                {
                    if (importTokens[i].Contains(".")) // input comes from file
                        filePathsList.Add(importTokens[i]);
                    else
                        System.Console.WriteLine("Neither operator nor input file!!!");
                }

            // check if there were input files
            if(filePathsList.Count != 0)
                importObj = importFactory.GetImport(new string[] { "FileImport" }, filePathsList.ToArray());

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

            foreach (string url in plain_urls)
                System.Diagnostics.Process.Start(@"Slave.exe",SerializeObject(importObj) + " " + SerializeObject(routeObj) + " " +
                    SerializeObject(processObj) + " " + SerializeObject(url) + " " + SerializeObject(input.PuppetMasterUrl) + " " +
                    SerializeObject(input.IsLogFull));
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
    }
}
