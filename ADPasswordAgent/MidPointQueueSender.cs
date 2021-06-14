using MidPointCommonTaskModels.Models;
using MidPointUpdatingService.ClassExtensions;
using SecureDiskQueue;
using System;
using System.Collections.Generic;

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
            using (var queue = PersistentSecureQueue.WaitFor(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), queuePath),TimeSpan.FromSeconds(30)))
            {
                EnqueueMidTask(updatePasswordCall, queue);
            }
        }

    }
}
