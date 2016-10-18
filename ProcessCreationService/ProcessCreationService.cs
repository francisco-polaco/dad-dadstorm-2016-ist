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
            Console.Write("Insert the Process Creation Service name: ");
            ProcessCreationService pcs = new ProcessCreationService(Console.ReadLine());
            Console.WriteLine("<Enter to exit> ");
            Console.ReadLine();    
        }

    }

    public class ProcessCreationService : MarshalByRefObject, SlaveLaunch
    {

        public ProcessCreationService(string name)
        {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, true);
            RemotingServices.Marshal(this, name, typeof(ProcessCreationService));
            Console.WriteLine("Process Creation Service " + name + " created!");
        }

        public void Launch(string input)
        {
            // TO DO
        }
    }
}
