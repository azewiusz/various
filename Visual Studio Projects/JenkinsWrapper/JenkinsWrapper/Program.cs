﻿using System.ServiceProcess;

namespace JenkinsWrapper
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
				new Service1()
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}