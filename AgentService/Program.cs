using System;
using System.ServiceProcess;

namespace AgentService
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
                        new AgentService()
                    };
                ServiceBase.Run(ServicesToRun);
            
        }
    }
}