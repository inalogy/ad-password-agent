using Microsoft.Win32;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Security;
using System.Threading;
using Common;
using System.Text;

namespace ADPasswordAgent
{
    class Program
    {
        static void Main(string[] args)
        {
            // default folder name of the queue
            string queuebasepath = @"Midpoint.ADPassword.Queue"; 
            long useHeapOnly = 0;

            string[] argvs = Environment.GetCommandLineArgs();

            if (argvs.Length != 3)
            {
                if (EnvironmentHelper.GetAgentLogging()<5)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format("Error - invalid call. Usage: {0} \"username\" \"password\"", argvs[0]), EventLogEntryType.Error, 301, 1);
                    }
                }
                return;
            }


            try
            {
                queuebasepath = EnvironmentHelper.GetQueueFolder() ?? queuebasepath;
            }
            catch
            {
                if (EnvironmentHelper.GetAgentLogging()<5)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format("Error - invalid call. Missing parameters in config file: {0}.config", argvs[0]), EventLogEntryType.Error, 302, 1);
                    }
                }
            }

            try
            {
                useHeapOnly = EnvironmentHelper.GetMidpointUseOnlyHeap();
            }
            catch
            {
                if (EnvironmentHelper.GetAgentLogging() < 5)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format("Error - invalid call. Missing parameters in config file: {0}.config", argvs[0]), EventLogEntryType.Error, 311, 1);
                    }
                }
            }

            try
            {
                MidPointQueueSender mp = new MidPointQueueSender(queuebasepath, useHeapOnly);
                mp.UpdateUserPasswordByName(argvs[1], argvs[2]);
                if (EnvironmentHelper.GetAgentLogging()<1)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        string processIdentity = "-Unknown-";
                        try
                        {
                            processIdentity = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                        }
                        catch { }
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format("Password change has been sent to queue for user {0}", argvs[1]), EventLogEntryType.Information, 101, 1);
                        eventLog.WriteEntry(String.Format("Password change Security Context is {0}", processIdentity), EventLogEntryType.Information, 101, 1);
                        }
                }
            }
            catch (Exception e)
            {
                if (EnvironmentHelper.GetAgentLogging()<5)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format("Error calling AD Password agent: {0}", e.Message), EventLogEntryType.Error, 303, 1);
                    }
                }
            }
        }
    }
}
