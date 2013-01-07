using System;
using System.ServiceProcess;

namespace MasterAgentService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main()
        {

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
                    {
                        new MasterAgentService()
                    };
            ServiceBase.Run(ServicesToRun);

        }
    }
}