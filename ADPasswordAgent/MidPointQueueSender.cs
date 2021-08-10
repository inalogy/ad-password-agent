using MidPointCommonTaskModels.Models;
using MidPointUpdatingService.ClassExtensions;
using SecureDiskQueue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common;
using System.IO;
using System.Security.Cryptography;

namespace ADPasswordAgent
{
    public class MidPointQueueSender
    {
       
        private readonly string queuePath;
        private readonly long useHeapOnly;

        private static void EnqueueMidTask(ActionCall task, IPersistentSecureQueue queue)
        {
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                var queueItem = Helpers.ObjectToByteArray(task);
                queueSession.Enqueue(queueItem);
                queueSession.Flush();
            }
        }

        private static void HeapSendMidTask(ActionCall task, string heapPath)
        {
            DirectoryInfo di;
            try
            {
                if (!Directory.Exists(heapPath))
                {
                    di = Directory.CreateDirectory(heapPath);

                }
                else
                {
                    di = new DirectoryInfo(heapPath);
                }
                if (di == null) {
                    throw new Exception(String.Format("Directory {0} has not been created from unknown reason.", heapPath));
                        }
            }
            catch (Exception ex)
            {
                if (EnvironmentHelper.GetAgentLogging() < 5)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format("Error creating heap folder for AD Password agent: {0}", ex.Message), EventLogEntryType.Error, 305, 1);
                    }
                }
                return;
            }
            var timestamp = DateTime.Now.ToFileTime();
            string heapfilename = String.Format("{0}.itq", timestamp);
            string fullHeapFileName = Path.Combine(di.FullName, heapfilename);
            int ctr = 0;
            try
            {
                var heapItem = Helpers.ObjectToByteArray(task);
                var protectedHeapItem = ProtectedData.Protect(heapItem, null, DataProtectionScope.LocalMachine);
                bool retryIOError = false;
                do
                {
                    while (File.Exists(fullHeapFileName))
                    {
                        heapfilename = String.Format("{0}_{1}.itq", timestamp, ctr++);
                        fullHeapFileName = Path.Combine(di.FullName, heapfilename);
                    }
                    try
                    {
                        retryIOError = false;
                        File.WriteAllBytes(fullHeapFileName, protectedHeapItem);
                    }
                    catch (IOException ex)
                    {
                        if (EnvironmentHelper.GetAgentLogging() < 5)
                        {
                            using (EventLog eventLog = new EventLog("Application"))
                            {
                                eventLog.Source = "ADPasswordAgent";
                                eventLog.WriteEntry(String.Format("Error heap file conflict {1} for AD Password agent: {0}", ex.Message, fullHeapFileName), EventLogEntryType.Warning, 209, 1);
                            }
                        }
                        retryIOError = true;
                    }
                } while (retryIOError);
            }
            catch (Exception ex)
            {
                if (EnvironmentHelper.GetAgentLogging() < 5)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format("Error creating heap file {1} for AD Password agent: {0}", ex.Message, fullHeapFileName), EventLogEntryType.Error, 306, 1);
                    }
                }
                return;
            }
        }



        public MidPointQueueSender(string cqueuefld, long cUseHeapOnly)
        {
            queuePath = cqueuefld;
            useHeapOnly = cUseHeapOnly;
        }


        public void UpdateUserPasswordByName(string name, string password)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "userName", name }, { "password", password } };
            ActionCall updatePasswordCall = new ActionCall("UpdatePassword", parameters);
            if (EnvironmentHelper.GetAgentLogging() < 1)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Information - heap queue location: {0} - user {1}", System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), queuePath), name), EventLogEntryType.Information, 107, 1);
                }
            }

            try
            {
                if (useHeapOnly == 0)
                {
                    using (var queue = PersistentSecureQueue.WaitFor(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), queuePath), TimeSpan.FromSeconds(EnvironmentHelper.GetQueueWaitSeconds())))
                    {
                        try
                        {
                            EnqueueMidTask(updatePasswordCall, queue);
                        }
                        catch
                        {
                            HeapSendMidTask(updatePasswordCall, System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), queuePath + ".Heap"));
                        }
                    }
                }
                else
                {
                    HeapSendMidTask(updatePasswordCall, System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), queuePath + ".Heap"));
                }
            }
            catch (TimeoutException)
            {
                HeapSendMidTask(updatePasswordCall, System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), queuePath + ".Heap"));
            }
            catch (Exception e)
            {
                if (EnvironmentHelper.GetAgentLogging() < 1)
                {
                    using (EventLog eventLog = new EventLog("Application"))
                    {
                        eventLog.Source = "ADPasswordAgent";
                        eventLog.WriteEntry(String.Format(@"Error - unable to access queue location: {0} - {1}", System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), queuePath + ".Heap"), e.Message), EventLogEntryType.Error, 409, 1);
                    }
                }

            }
        }
    }
}
