using MidPointCommonTaskModels.Models;
using MidPointUpdatingService.ClassExtensions;
using MidPointUpdatingService.Models;
using MidPointUpdatingService.Operations;
using SecureDiskQueue;
using System.Collections.Generic;
using System.Net.Http;

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

        public static void ProcessItem(HttpClient client, IPersistentSecureQueue queue)
        {
            ActionCall call = ExecutionEngine.PeekMidTask(queue);
            if (call!=null)
            {
                System.Diagnostics.Debugger.Launch();
                if (!string.IsNullOrEmpty(call.ActionName))
                {
                    switch (call.ActionName)
                    {
                        case "UpdatePassword":
                            IMidPointOperation updatePasswordOperation = new UpdatePasswordOperation() { TTL=50 };
                            updatePasswordOperation.ExecuteOperation(call.Parameters, client);
                            if (updatePasswordOperation.TTL == 0) { 
                                //Operation finished or non-recoverable error occured
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

        private static ActionCall PeekMidTask(IPersistentSecureQueue queue)
        {
            ActionCall call = null;
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                var queueItem = queueSession.Peek();
                if (queueItem != null)
                {
                    call = (ActionCall)Helpers.ByteArrayToObject((byte[])queueItem, typeof(ActionCall));
                }
            }
            return call;
        }

        private static ActionCall DequeueMidTask(IPersistentSecureQueue queue)
        {
            ActionCall call = null;
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                var queueItem = queueSession.Dequeue();
                if (queueItem != null)
                {
                    call = (ActionCall)Helpers.ByteArrayToObject((byte[])queueItem, typeof(ActionCall));
                    queueSession.Flush();
                }
            }
            return call;
        }


    }
}
