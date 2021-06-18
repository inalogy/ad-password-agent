using MidPointCommonTaskModels.Models;
using MidPointUpdatingService.ClassExtensions;
using SecureDiskQueue;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Common;

namespace ADPasswordAgent
{
    public class MidPointQueueSender
    {
       
        private readonly string queuePath;


        private static void EnqueueMidTask(ActionCall task, IPersistentSecureQueue queue)
        {
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                var queueItem = Helpers.ObjectToByteArray(task);
                queueSession.Enqueue(queueItem);
                queueSession.Flush();
            }
        }

        public MidPointQueueSender(string cqueuefld)
        {
            queuePath = cqueuefld;
        }


        public void UpdateUserPasswordByName(string name, string password)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "userName", name }, { "password", password } };
            ActionCall updatePasswordCall = new ActionCall("UpdatePassword", parameters);
            if (EnvironmentHelper.GetAgentLogging() > 1)
            {
                using (EventLog eventLog = new EventLog("Application"))
                {
                    eventLog.Source = "ADPasswordAgent";
                    eventLog.WriteEntry(String.Format(@"Information - queue location: {0}", System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), queuePath)), EventLogEntryType.Information, 107, 1);
                }
            }
            using (var queue = PersistentSecureQueue.WaitFor(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), queuePath),TimeSpan.FromSeconds(30)))
            {
                EnqueueMidTask(updatePasswordCall, queue);
            }
        }

    }
}
