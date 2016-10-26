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
            // TO DO

            // Márcio
            // Deves criar os objectos a partir das factories, dentro do ficheiro factories
            // tens um summary que diz o que cada objecto precisa, ou seja que params eu assumo.
            // Depois passas ao constructor do Slave, o url no slave é nome completo dele.
        }
    }
}
