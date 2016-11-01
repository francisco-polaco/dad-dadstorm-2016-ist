using System;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Slave;
using System.Linq;

namespace ProcessCreationService
{
    public class ExecEnv
    {
        static void Main(string[] args)
        {
            Console.Write("Insert the Process Creation Service name: ");
            ProcessCreationService pcs = new ProcessCreationService(Console.ReadLine());
            Console.WriteLine("<Enter to exit> ");
            Console.ReadLine();    
        }
    }

    public class ProcessCreationService : MarshalByRefObject, IPcsSlaveLaunch
    {

        public ProcessCreationService(string name)
        {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this, name, typeof(ProcessCreationService));
            Console.WriteLine("Process Creation Service " + name + " created!");
        }

        public void Launch(ConnectionPack input)
        {

            string cmd = input.Cmd;
            List<string> urls = input.ListUrls; // contains empty strings right???
            List<string> downstream_urls = input.ReplicaUrlsOutput; // contains empty strings right???

            // split command by keywords
            string pattern = @"INPUT_OPS|REP_FACT|ROUTING|ADDRESS|OPERATOR_SPEC|\s";
            string[] tokens = Regex.Split(cmd, pattern).Where(s => s != String.Empty).ToArray<string>(); ;

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
            string routingPattern = @"(\d+)|\s"; // TO DO - check if pattern works
            string[] routingTokens = Regex.Split(tokens[3], routingPattern).Where(s => s != String.Empty).ToArray<string>(); ;

            //routeObj = routingFactory.GetRouting(routingTokens, downstream_urls);

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
                new Slave.Slave(importObj, routeObj, processObj, url);

            // TO DO

            // Márcio
            // Deves criar os objectos a partir das factories, dentro do ficheiro factories
            // tens um summary que diz o que cada objecto precisa, ou seja que params eu assumo.
            // Depois passas ao constructor do Slave, o url no slave é nome completo dele.

        }
    }
}
