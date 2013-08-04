using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using Microsoft.Win32;
using System.Diagnostics;
namespace JenkinsWrapper
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

           


            this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
            this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller1
            // 
            this.serviceProcessInstaller1.Password = null;
            this.serviceProcessInstaller1.Username = null;
            // 
            // serviceInstaller1
            // 
            this.serviceInstaller1.Description = "Optionally start with parameters like: -runBinary \"C:\\Program Files (x86)\\Jenkins" +
    "\\service.bat\" -workDir \"C:\\Program Files (x86)\\Jenkins\"";
            // 
            // ProjectInstaller
            // 
            SetServicePropertiesFromCommandLine(serviceInstaller1);
                        
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.serviceProcessInstaller1,
            this.serviceInstaller1});

        }

        private void SetServicePropertiesFromCommandLine(System.ServiceProcess.ServiceInstaller serviceInstaller)
        {
            string[] commandlineArgs = Environment.GetCommandLineArgs();

            string servicename;
            string servicedisplayname;
            string runb, workd;
            ParseServiceNameSwitches(commandlineArgs, out servicename, out servicedisplayname, out runb,out workd);

            serviceInstaller.ServiceName = servicename;           
            serviceInstaller.DisplayName = servicedisplayname;
            // Custom optional parameters
            this.runBinary = runb;
            this.workDir = workd;
        }

        private void ParseServiceNameSwitches(string [] commandlineArgs, out string serviceName, out string serviceDisplayName, out string runB, out string workD)
        {
            var servicenameswitch = (from s in commandlineArgs where s.StartsWith("/servicename") select s).FirstOrDefault();
            var servicedisplaynameswitch = (from s in commandlineArgs where s.StartsWith("/servicedisplayname") select s).FirstOrDefault();

            // The below two parameters are optional during installation, but are mandatory during Startup of Service.

            var runBinary = (from s in commandlineArgs where s.StartsWith("/runbinary") select s).FirstOrDefault();
            var workDir = (from s in commandlineArgs where s.StartsWith("/workdir") select s).FirstOrDefault();

            if (servicenameswitch == null)
                throw new ArgumentException("Argument 'servicename' is missing");
            if (servicedisplaynameswitch == null)
                throw new ArgumentException("Argument 'servicedisplayname' is missing");
            if (!(servicenameswitch.Contains('=') || servicenameswitch.Split('=').Length < 2))
                throw new ArgumentNullException("The /servicename switch is malformed");

            if (!(servicedisplaynameswitch.Contains('=') || servicedisplaynameswitch.Split('=').Length < 2))
                throw new ArgumentNullException("The /servicedisplayname switch is malformed");

            serviceName = servicenameswitch.Split('=')[1];
            serviceDisplayName = servicedisplaynameswitch.Split('=')[1];

            serviceName = serviceName.Trim('"');
            serviceDisplayName = serviceDisplayName.Trim('"');

            runB = "";
            workD = "";

            if (runBinary != null)
            {
                runB = runBinary.Split('=')[1];
                runB = runB.Trim('"');
            }

            if (workDir != null)
            {
                workD = workDir.Split('=')[1];
                workD = workD.Trim('"');
            }

        }



        /// <summary>
        /// Modify the registry to install the new service
        /// </summary>
        /// <PARAM name="stateServer"></PARAM>
        public override void Install(IDictionary stateServer)
        {
            RegistryKey system,
                //HKEY_LOCAL_MACHINE\Services\CurrentControlSet
                currentControlSet,
                //...\Services
                services,
                //...\<Service Name>
                service,
                //...\Parameters - this is where you can 
                //put service-specific configuration
                config
               ;

            base.Install(stateServer);

            // Define the registry keys
            // Navigate to services
            system = Registry.LocalMachine.OpenSubKey("System");
            currentControlSet = system.OpenSubKey("CurrentControlSet");
            services = currentControlSet.OpenSubKey("Services");
            // Add the service
            service =
              services.OpenSubKey(this.serviceInstaller1.ServiceName, true);
            config = service.CreateSubKey("Parameters");

            config.SetValue("runBinary",this.runBinary);
            config.SetValue("workDir", this.workDir);


            // Close keys
            config.Close();
            service.Close();
            services.Close();
            currentControlSet.Close();
            system.Close();
        }

        /// <summary>
        /// Uninstall based on the service name
        /// </summary>
        /// <PARAM name="savedState"></PARAM>
      

        /// <summary>
        /// Modify the registry to remove the service
        /// </summary>
        /// <PARAM name="stateServer"></PARAM>
        public override void Uninstall(IDictionary stateServer)
        {
            RegistryKey system,
                //HKEY_LOCAL_MACHINE\Services\CurrentControlSet
                currentControlSet,
                //...\Services
                services,
                //...\<Service Name>
                service;
            //...\Parameters - this is where you can 
            //put service-specific configuration

            base.Uninstall(stateServer);

            // Navigate down the registry path
            system = Registry.LocalMachine.OpenSubKey("System");
            currentControlSet = system.OpenSubKey("CurrentControlSet");
            services = currentControlSet.OpenSubKey("Services");
            service =
               services.OpenSubKey(this.serviceInstaller1.ServiceName, true);
            // Remove the parameters key
            service.DeleteSubKeyTree("Parameters");

            // Close keys
            service.Close();
            services.Close();
            currentControlSet.Close();
            system.Close();
        }
        #endregion

        private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
        private System.ServiceProcess.ServiceInstaller serviceInstaller1;
        // Below parameters are optional, if provided duting installation are stored in System Registry.
        // The same parameters can be provided duting Service startup and will take precedence as an input to Service OnStart method
        private string workDir;
        private string runBinary;
    }
}