using MidPointUpdatingService.ClassExtensions;
using MidPointUpdatingService.Actions;
using MidPointUpdatingService.Models;
using SecureDiskQueue;
using System.Collections.Generic;

namespace ADPasswordAgent
{
    public class MidPointQueueSender
    {
       
        private int TTL = 50;
        private string queuePath;

        private static void EnqueueMidTask(MidPointTask task, PersistentSecureQueue queue)
        {
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                queueSession.Enqueue(Helpers.ObjectToByteArray(task));
                queueSession.Flush();
            }
        }

        public MidPointQueueSender(string qbasepath, int ttl)
        {
            queuePath = qbasepath;
            TTL = ttl;        
        }


        public void UpdateUserPasswordByName(string name, string password)
        {
            Dictionary<string, object> parameters = new Dictionary<string, object>() { { "userName", name }, { "password", password } };
            UpdatePasswordMidPointAction updatePasswordAction = new UpdatePasswordMidPointAction();
            MidPointTask updatePasswordTask = new MidPointTask(updatePasswordAction, TTL, parameters);
            using (var queue = new PersistentSecureQueue(queuePath))
            {
                EnqueueMidTask(updatePasswordTask, queue);
            }
        }

    }
}
