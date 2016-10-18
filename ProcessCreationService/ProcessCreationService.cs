using System;
using CommonTypes;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting;

namespace ProcessCreationService
{
    public class ExecEnv
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Choose the import mode:\n1) Import via File\n2) Import via command line\n#) Exit");

            int caseSwitch;

            while (!Int32.TryParse(Console.ReadLine(), out caseSwitch))
                Console.WriteLine("Input must be a integer!");

            switch (caseSwitch)
            {
                case 1:
                    Console.Write("Insert the filepath to the config file: ");
                    importViaFile(Console.ReadLine());
                    break;
                case 2:
                    importViaConsole();
                    break;
                default:
                    break;
            }
        }

        public static void importViaFile(string filename)
        {
            Console.WriteLine("IMPORT VIA FILE - " + filename);
        }

        public static void importViaConsole()
        {
           Console.WriteLine("IMPORT VIA CONSOLE");
        }
    }

    public class ProcessCreationService : MarshalByRefObject, SlaveLaunch
    {

        public ProcessCreationService(string name)
        {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this, name, typeof(ProcessCreationService));
        }

        public void launch(string input)
        {
            // TO DO
        }
    }
}
