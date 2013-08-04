using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using net.sf.dotnetcli;

/*
 * Author: Tomasz Kosiński
 * email: azewiusz@gmail.com
 * Service's ExitCodes used in this project do not follow any special convention as described e.g. http://www.hiteksoftware.com/knowledge/articles/049.htm
 * Below code can be distributed, modified and resued freely.
 */

namespace JenkinsWrapper
{
    public partial class Service1 : ServiceBase
    {
        // A monitoring thread will run if OnStart succeeds, will be terminated during OnStop call
        // Monitor queries for process with PID = ROOT_PID, if process does not exist the service will be shut down automatically.
        // Check is performed every 5 seconds.
        private Timer childMonitor;

        private StringBuilder errorBuffer = new StringBuilder();

        private StringBuilder outputBuffer = new StringBuilder();

        // PID of process executed to run e.g. Jenkins inside Session 0 with an Interactive Desktop that will remain
        // even afte all users log off from worksation.
        // Service - OnStart
        // Service - OnStop - Kills this PID's process tree
        private string ROOT_PID = null;
        /**
         * For asynchronous reading from first child process std error stream
         **/

        public Service1()
        {
            InitializeComponent();
            // Below ones added manually

            this.eventLog = new System.Diagnostics.EventLog();
            this.eventLog.Source = this.ServiceName;
            this.eventLog.Log = "Application";
            ((System.ComponentModel.ISupportInitialize)(this.eventLog)).BeginInit();
            if (!EventLog.SourceExists(this.eventLog.Source))
            {
                EventLog.CreateEventSource(this.eventLog.Source, this.eventLog.Log);
            }
            ((System.ComponentModel.ISupportInitialize)(this.eventLog)).EndInit();


            this.CanStop = true;
            this.CanPauseAndContinue = false;
        }

        public static string getPID(string consoleoutput)
        {
            bool isPID = consoleoutput.Contains("with process ID");
            // No PID information in console output
            if (!isPID) return null;

            string proc = consoleoutput.Replace("with process ID", "##");
            string[] procID = proc.Split(new string[] { "##" }, StringSplitOptions.RemoveEmptyEntries);
            if (procID.Length > 1)
            {
                return procID[1].Replace(".", "").Trim();
            }
            else
                return null;
        }

        public void ErrorReceiver(object o, DataReceivedEventArgs args)
        {
            errorBuffer.AppendLine(args.Data);
        }

        /**
         * For asynchronous reading from first child process std out stream
         **/

        public void OutputReceiver(object o, DataReceivedEventArgs args)
        {
            outputBuffer.AppendLine(args.Data);
        }
        protected override void OnStart(string[] args)
        {
            string reg_runBinary = null, reg_workDir = null;

            eventLog.WriteEntry(this.ServiceName + " - Service Start procedure initiated.", EventLogEntryType.Information);

            RegistryKey system,
              
                currentControlSet,
                
                services,
               
                service,
           
                config
               ;

            // Define the registry keys
            // Navigate to services
            system = Registry.LocalMachine.OpenSubKey("System");
            currentControlSet = system.OpenSubKey("CurrentControlSet");
            services = currentControlSet.OpenSubKey("Services");
            // Add the service
            service = services.OpenSubKey(this.ServiceName);

            // Create a parameters subkey
            if (service == null)
            {
                eventLog.WriteEntry(this.ServiceName + " - Cannot obtain Service Registry Key.", EventLogEntryType.Warning);
            }
            else
            {
                config = service.OpenSubKey("Parameters");

                // Read startup parameters as defined during installation phase
                reg_runBinary = config.GetValue("runBinary").ToString();
                reg_workDir = config.GetValue("workDir").ToString();
                config.Close();
                service.Close();
            }

            // Close keys

            services.Close();
            currentControlSet.Close();
            system.Close();

            Options options = new Options();

            // add t option
            Option runBinaryO = OptionBuilder.Factory.WithArgName("runBinary").HasArg().WithDescription(
                               "Indicates an exact executable file or script that will be run as a Service.").Create("runBinary");

            Option workDirO = OptionBuilder.Factory.WithArgName("workDir").HasArg().WithDescription(
                                 "Indicates this service's working directory.").Create("workDir");

            options.AddOption(runBinaryO);
            options.AddOption(workDirO);

            GnuParser parser = new GnuParser();
            CommandLine cmd = parser.Parse(options, args);
            string runBinary = cmd.GetOptionValue("runBinary");
            string workDir = cmd.GetOptionValue("workDir");

            if (runBinary == null || workDir == null)
            {
                // try to use Windows Registry startup parameters
                if (reg_runBinary == null || reg_workDir == null || reg_runBinary == "" || reg_workDir == "")
                {
                    // We cannot start Service
                    eventLog.WriteEntry(this.ServiceName + " - START - FAILED, the startup parameters -runBinary and/or -workDir were not defined during startup or during installation of this service.", EventLogEntryType.Error);
                    this.ExitCode = 3;
                    this.Stop();
                }
                else
                {
                    // Pick up values from Windows Registry
                    runBinary = reg_runBinary;
                    workDir = reg_workDir;
                    eventLog.WriteEntry(this.ServiceName + " - using startup parameters from Windows Registry.", EventLogEntryType.Information);
                }
            }
            else
            {
                // Do nothing, the default Service Startup parameters as provided during service start will be used.
                eventLog.WriteEntry(this.ServiceName + " - using startup parameters from Service startup.", EventLogEntryType.Information);
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;

            startInfo.UseShellExecute = false;
            startInfo.FileName = runBinary;
            //  startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "";
            startInfo.WorkingDirectory = workDir;
            startInfo.ErrorDialog = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;
            startInfo.RedirectStandardError = true;
            startInfo.StandardErrorEncoding = Encoding.UTF8;
            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    //
                    // Read in all the text from the process with the async read.
                    //
                    process.ErrorDataReceived += new DataReceivedEventHandler(ErrorReceiver);
                    process.OutputDataReceived += new DataReceivedEventHandler(OutputReceiver);
                    process.BeginErrorReadLine();
                    process.BeginOutputReadLine();

                    process.WaitForExit();

                    // Write some log information from the first Process called inside this service.

                    StreamWriter outfile = new StreamWriter(workDir + @"\process.pid");

                    outfile.WriteLine("**** STD OUT STREAM OUTPUT BELOW ****");
                    outfile.Write(outputBuffer.ToString());
                    outfile.WriteLine("**** ERROR STREAM OUTPUT BELOW ****");
                    outfile.Write(errorBuffer.ToString());
                    outfile.Flush();
                    outfile.Close();

                    string pidCan = Service1.getPID(outputBuffer.ToString());

                    if (pidCan == null)
                    {
                        // Try extract PID from the error output stream.
                        pidCan = Service1.getPID(errorBuffer.ToString());
                    }

                    ROOT_PID = pidCan;

                    if (ROOT_PID == null)
                    {
                        // Exit application - startup of service failed, process ID was not present in the
                        // target process console output. (It is still possible that the process has started)
                        eventLog.WriteEntry(this.ServiceName + " - START - FAILED for : runBinary : " + runBinary + ", workDir : " + workDir + " first child process could not be obatained. Service's Stop procedure may not be able to shutdown this service entirely.", EventLogEntryType.Error);
                        this.ExitCode = 2;
                        this.Stop();
                    }
                }
            }
            catch (Exception e)
            {
                eventLog.WriteEntry("First child process start FAILED : " + e.ToString(), EventLogEntryType.Error);
                this.ExitCode = 5;
                return;
            }

            eventLog.WriteEntry(this.ServiceName + " - STARTED [Child Process PID : " + ROOT_PID + "]: runBinary : " + runBinary + ", workDir : " + workDir, EventLogEntryType.Information);

            childMonitor = new Timer(new TimerCallback(MonitorChildren), null, 5000, 5000);

            this.ExitCode = 0;
        }

        /**
         * Method used to performs monitoring on child process.
         *
         * **/

        protected override void OnStop()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "taskkill";
            //  startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            if (ROOT_PID == null)
            {
                return;
            }
            eventLog.WriteEntry(this.ServiceName + " - Service Stop procedure initiated. [Killing first child Process PID:" + ROOT_PID + "]", EventLogEntryType.Information);

            //  startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "/F /T /PID " + ROOT_PID;
            startInfo.ErrorDialog = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;

            using (Process process = Process.Start(startInfo))
            {
                try
                {
                    // Wait for process to exit
                    process.WaitForExit();
                }
                catch (Exception E)
                {
                    eventLog.WriteEntry("Stop procedure of Process " + ROOT_PID + " FAILED.", EventLogEntryType.Error);
                    this.ExitCode = 3;
                    this.Stop();
                }
            }

            eventLog.WriteEntry(this.ServiceName + " - Service has been STOPPED succesfully.", EventLogEntryType.Information);
            this.ExitCode = 0;
        }

        private void MonitorChildren(object state)
        {
            if (ROOT_PID == null)
            {
                // Abnormal execution
                childMonitor.Dispose();
                // Nothing to monitor
                return;
            }
            Process childProcess = null;
            try
            {
                childProcess = Process.GetProcessById(int.Parse(ROOT_PID));
            }
            catch (ArgumentException ex)
            {
                childProcess = null;
            }

            if (childProcess == null)
            {
                // Initiate Shutdown of this service
                childMonitor.Dispose();
                eventLog.WriteEntry(this.ServiceName + " - the child process died before Service was stopped, initiating Service Stop [Dead process PID:" + ROOT_PID + "]", EventLogEntryType.Error);
                this.ROOT_PID = null;
                this.ExitCode = 5;
                this.Stop();
            }
        }
        /**
         * A helper function that extracts Process ID as reported by PSEXEC tool. This could be customized.
         * **/
    }
}