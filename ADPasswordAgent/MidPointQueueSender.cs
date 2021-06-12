using MidPointCommonTaskModels.Models;
using MidPointUpdatingService.Actions;
using MidPointUpdatingService.ClassExtensions;
using SecureDiskQueue;
using System.Collections.Generic;

namespace ADPasswordAgent
{
    public class MidPointQueueSender
    {
       
        private readonly string queuePath;


        private static void EnqueueMidTask(ActionCall task, PersistentSecureQueue queue)
        {
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                queueSession.Enqueue(Helpers.ObjectToByteArray(task));
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
            UpdatePasswordMidPointAction updatePasswordAction = new UpdatePasswordMidPointAction();
            ActionCall updatePasswordCall = new ActionCall("UpdatePassword", parameters);
            using (var queue = new PersistentSecureQueue(System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), queuePath)))
            {
                EnqueueMidTask(updatePasswordCall, queue);
            }
        }

    }
}
