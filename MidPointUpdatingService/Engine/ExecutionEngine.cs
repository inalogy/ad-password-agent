using ADPasswordSecureCache;
using MidPointCommonTaskModels.Models;
using MidPointUpdatingService.Actions;
using MidPointUpdatingService.ClassExtensions;
using MidPointUpdatingService.Models;
using MidPointUpdatingService.Operations;
using SecureDiskQueue;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MidPointUpdatingService.Engine
{
    public static class ExecutionEngine
    {
        public static Dictionary<string, object> ExecuteMidpointAction(MidPointAction actionStep, HttpClient client, out MidPointError error)
        {
            var results = new Dictionary<string, object>();
            actionStep.Execute(client, out IActionResult result);
            if (result != null)
            {                    
                results = result.ResultDictionary;
                error = result.Error;
            }
            else
            {

                error = new MidPointError() { ErrorCode = MidPointErrorEnum.NoActionResult, Recoverable = false, ErrorMessage = "Action returned no answer" };
            }            
            return results;
        }

        public static void ProcessItem(HttpClient client, PersistentSecureQueue queue)
        {
            ActionCall call = ExecutionEngine.PeekMidTask(queue);
            if (call!=null)
            {
                if (!string.IsNullOrEmpty(call.ActionName))
                {
                    switch (call.ActionName)
                    {
                        case "UpdatePassword":
                            IMidPointOperation updatePasswordOperation = new UpdatePasswordOperation();
                            updatePasswordOperation.ExecuteOperation(call.Parameters, client);
                            if (updatePasswordOperation.TTL == 0)
                            {
                                ExecutionEngine.DequeueMidTask(queue);
                            }
                            break;
        
                        default:
                            // Unknown Action name
                            ExecutionEngine.DequeueMidTask(queue);
                            break;
                    }
                }
                else
                {
                    // Empty Action name
                    ExecutionEngine.DequeueMidTask(queue);
                }
            }
        }

        /* For example only - service using only Peek and dequeue */
        /*
        private static void EnqueueMidTask(ActionCall call, PersistentSecureQueue queue)
        {
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                queueSession.Enqueue(Helpers.ObjectToByteArray(call));
                queueSession.Flush();
            }
        }
        */

        private static ActionCall PeekMidTask(PersistentSecureQueue queue)
        {
            ActionCall call = null;
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                var queueItem = queueSession.Peek();
                if (queueItem != null)
                {
                    call = (ActionCall)Helpers.ByteArrayToObject((byte[])queueItem);
                }
            }
            return call;
        }

        private static ActionCall DequeueMidTask(PersistentSecureQueue queue)
        {
            ActionCall call = null;
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                var queueItem = queueSession.Dequeue();
                if (queueItem != null)
                {
                    call = (ActionCall)Helpers.ByteArrayToObject((byte[])queueItem);
                    queueSession.Flush();
                }
            }
            return call;
        }


    }
}
