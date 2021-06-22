using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class EnvironmentHelper
    {
        public const string loggingHive = @"SOFTWARE\ADPasswordFilter";

        public static Int64 GetAgentLogging()
        {
            Int64 value;
            try
            {
                //get the 64-bit view first
                RegistryKey key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                key = key.OpenSubKey(loggingHive);

                if (key == null)
                {
                    //we couldn't find the value in the 64-bit view so grab the 32-bit view
                    key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                    key = key.OpenSubKey(loggingHive);
                }

                if (key != null)
                {
                    value = Convert.ToInt64(key.GetValue("AgentLogging").ToString());
                }
                else
                {
                    value = 2;
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\AgentLogging - full logging enabled."), EventLogEntryType.Warning, 201, 1);
                    }

                }
            }
            catch (Exception ex)
            {
                value = 2;
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Error accessing registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\AgentLogging : {0}", ex.Message), EventLogEntryType.Error, 302, 1);
                    eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\AgentLogging - full logging enabled."), EventLogEntryType.Warning, 201, 1);
                }
            }

            return value;
        }

        public static string GetMidpointBaseUrl()
        {
            string value = null;
            try
            {
                //get the 64-bit view first
                RegistryKey key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                key = key.OpenSubKey(loggingHive);

                if (key == null)
                {
                    //we couldn't find the value in the 64-bit view so grab the 32-bit view
                    key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                    key = key.OpenSubKey(loggingHive);
                }

                if (key != null)
                {
                    value = key.GetValue("MidpointBaseUrl").ToString();
                }
                else
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointBaseUrl - using default url."), EventLogEntryType.Warning, 201, 1);
                    }

                }
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Error accessing registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointBaseUrl : {0}", ex.Message), EventLogEntryType.Error, 302, 1);
                    eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointBaseUrl - using default url."), EventLogEntryType.Warning, 201, 1);
                }
            }
            return value;
        }

        public static string GetMidpointAuthUser()
        {
            string value = null;
            try
            {
                //get the 64-bit view first
                RegistryKey key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                key = key.OpenSubKey(loggingHive);

                if (key == null)
                {
                    //we couldn't find the value in the 64-bit view so grab the 32-bit view
                    key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                    key = key.OpenSubKey(loggingHive);
                }

                if (key != null)
                {
                    value = key.GetValue("MidpointAuthUser").ToString();
                }
                else
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointAuthUser - using default user."), EventLogEntryType.Warning, 201, 1);
                    }

                }
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Error accessing registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointAuthUser : {0}", ex.Message), EventLogEntryType.Error, 302, 1);
                    eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointAuthUser - using default user."), EventLogEntryType.Warning, 201, 1);
                }
            }
            return value;
        }

        public static string GetMidpointAuthPwd()
        {
            string value = null;
            try
            {
                //get the 64-bit view first
                RegistryKey key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                key = key.OpenSubKey(loggingHive);

                if (key == null)
                {
                    //we couldn't find the value in the 64-bit view so grab the 32-bit view
                    key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                    key = key.OpenSubKey(loggingHive);
                }

                if (key != null)
                {
                    value = key.GetValue("MidpointAuthPwd").ToString();
                }
                else
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointAuthPwd - using default password."), EventLogEntryType.Warning, 201, 1);
                    }

                }
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Error accessing registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointAuthPwd : {0}", ex.Message), EventLogEntryType.Error, 302, 1);
                    eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointAuthPwd - using default password."), EventLogEntryType.Warning, 201, 1);
                }
            }
            return value;
        }


        public static string GetQueueFolder()
        {
            string value = null;
            try
            {
                //get the 64-bit view first
                RegistryKey key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                key = key.OpenSubKey(loggingHive);

                if (key == null)
                {
                    //we couldn't find the value in the 64-bit view so grab the 32-bit view
                    key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                    key = key.OpenSubKey(loggingHive);
                }

                if (key != null)
                {
                    value = key.GetValue("QueueFolder").ToString();
                }
                else
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\QueueFolder - using default folder."), EventLogEntryType.Warning, 201, 1);
                    }

                }
            }
            catch (Exception ex)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Error accessing registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\QueueFolder : {0}", ex.Message), EventLogEntryType.Error, 302, 1);
                    eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\QueueFolder - using default folder."), EventLogEntryType.Warning, 201, 1);
                }
            }
            return value;
        }

        public static Int64 GetRetryCount()
        {
            Int64 value;
            try
            {
                //get the 64-bit view first
                RegistryKey key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                key = key.OpenSubKey(loggingHive);

                if (key == null)
                {
                    //we couldn't find the value in the 64-bit view so grab the 32-bit view
                    key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                    key = key.OpenSubKey(loggingHive);
                }

                if (key != null)
                {
                    value = Convert.ToInt64(key.GetValue("RetryCount").ToString());
                }
                else
                {
                    value = 50;
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\RetryCount - using default 50 retries."), EventLogEntryType.Warning, 201, 1);
                    }

                }
            }
            catch (Exception ex)
            {
                value = 50;
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Error accessing registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\RetryCount : {0}", ex.Message), EventLogEntryType.Error, 302, 1);
                    eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\RetryCount - using default 50 retries."), EventLogEntryType.Warning, 201, 1);
                }
            }
            return value;
        }

        public static Int64 GetQueueWaitSeconds()
        {
            Int64 value;
            try
            {
                //get the 64-bit view first
                RegistryKey key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                key = key.OpenSubKey(loggingHive);

                if (key == null)
                {
                    //we couldn't find the value in the 64-bit view so grab the 32-bit view
                    key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                    key = key.OpenSubKey(loggingHive);
                }

                if (key != null)
                {
                    value = Convert.ToInt64(key.GetValue("QueueWaitSeconds").ToString());
                }
                else
                {
                    value = 30;
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\QueueWaitSeconds - using default 30 seconds."), EventLogEntryType.Warning, 201, 1);
                    }

                }
            }
            catch (Exception ex)
            {
                value = 30;
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Error accessing registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\QueueWaitSeconds : {0}", ex.Message), EventLogEntryType.Error, 302, 1);
                    eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\QueueWaitSeconds - using default 30 seconds."), EventLogEntryType.Warning, 201, 1);
                }
            }
            return value;
        }

        public static Int64 GetMidpointServiceLogLevel()
        {
            Int64 value;
            try
            {
                //get the 64-bit view first
                RegistryKey key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                key = key.OpenSubKey(loggingHive);

                if (key == null)
                {
                    //we couldn't find the value in the 64-bit view so grab the 32-bit view
                    key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                    key = key.OpenSubKey(loggingHive);
                }

                if (key != null)
                {
                    value = Convert.ToInt64(key.GetValue("MidpointServiceLogLevel").ToString());
                }
                else
                {
                    value = 0;
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointServiceLogLevel - using default level 0."), EventLogEntryType.Warning, 201, 1);
                    }

                }
            }
            catch (Exception ex)
            {
                value = 0;
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Error accessing registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointServiceLogLevel : {0}", ex.Message), EventLogEntryType.Error, 302, 1);
                    eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointServiceLogLevel - using default level 0."), EventLogEntryType.Warning, 201, 1);
                }
            }
            return value;
        }

        

        public static string GetMidpointCertName()
        {
            string value;
            try
            {
                //get the 64-bit view first
                RegistryKey key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                key = key.OpenSubKey(loggingHive);

                if (key == null)
                {
                    //we couldn't find the value in the 64-bit view so grab the 32-bit view
                    key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                    key = key.OpenSubKey(loggingHive);
                }

                if (key != null)
                {
                    value = key.GetValue("MidpointCertificate").ToString();
                }
                else
                {
                    value = null;
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointCertificate - no certificate identification found"), EventLogEntryType.Warning, 206, 1);
                    }

                }
            }
            catch (Exception ex)
            {
                value = null;
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Error accessing registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointCertificate : {0}", ex.Message), EventLogEntryType.Error, 306, 1);
                    eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointCertificate - no certificate identification found"), EventLogEntryType.Warning, 206, 1);
                }
            }
            return value;
        }



        public static string GetMidpointServiceLogPath()
        {
            string value;
            try
            {
                //get the 64-bit view first
                RegistryKey key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                key = key.OpenSubKey(loggingHive);

                if (key == null)
                {
                    //we couldn't find the value in the 64-bit view so grab the 32-bit view
                    key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                    key = key.OpenSubKey(loggingHive);
                }

                if (key != null)
                {
                    value = key.GetValue("MidpointServiceLogPath").ToString();
                }
                else
                {
                    value = @"Logs\";
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointServiceLogPath - using default path .\Logs."), EventLogEntryType.Warning, 201, 1);
                    }

                }
            }
            catch (Exception ex)
            {
                value = @"Logs\";
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Error accessing registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointServiceLogPath : {0}", ex.Message), EventLogEntryType.Error, 302, 1);
                    eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointServiceLogPath - using default path .\Logs."), EventLogEntryType.Warning, 201, 1);
                }
            }
            return value;
        }


        public static Int64 GetMidpointSsl()
        {
            Int64 value;
            try
            {
                //get the 64-bit view first
                RegistryKey key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry64);
                key = key.OpenSubKey(loggingHive);

                if (key == null)
                {
                    //we couldn't find the value in the 64-bit view so grab the 32-bit view
                    key = RegistryKey.OpenBaseKey(Microsoft.Win32.RegistryHive.LocalMachine, RegistryView.Registry32);
                    key = key.OpenSubKey(loggingHive);
                }

                if (key != null)
                {
                    value = Convert.ToInt64(key.GetValue("MidpointSSL").ToString());
                }
                else
                {
                    value = 0;
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointSSL - using HTTP connection"), EventLogEntryType.Warning, 205, 1);
                    }

                }
            }
            catch (Exception ex)
            {
                value = 0;
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Error accessing registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointSSL : {0}", ex.Message), EventLogEntryType.Error, 305, 1);
                    eventLog.WriteEntry(String.Format(@"Warning - unable to read registry key HKEY_LOCAL_MACHINE\SOFTWARE\ADPasswordFilter\MidpointSSL - using HTTP connection"), EventLogEntryType.Warning, 205, 1);
                }
            }
            return value;
        }
    }
}
