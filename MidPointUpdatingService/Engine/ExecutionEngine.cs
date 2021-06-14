using log4net;
using MidPointCommonTaskModels.Models;
using MidPointUpdatingService.ClassExtensions;
using MidPointUpdatingService.Models;
using MidPointUpdatingService.Operations;
using SecureDiskQueue;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace MidPointUpdatingService.Engine
{
    public static class ExecutionEngine
    {
        public static Dictionary<string, object> ExecuteMidpointAction(MidPointAction actionStep, HttpClient client, ILog log, out MidPointError error)
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

        public static void ProcessItem(HttpClient client, ILog log, int retryCount, IPersistentSecureQueue queue)
        {
            ActionCall call = ExecutionEngine.PeekMidTask(queue,log);
            if (call!=null)
            {
                if (!string.IsNullOrEmpty(call.ActionName))
                {
                    switch (call.ActionName)
                    {
                        case "UpdatePassword":
                            IMidPointOperation updatePasswordOperation = new UpdatePasswordOperation(retryCount);
                            updatePasswordOperation.ExecuteOperation(call.Parameters, client, log);
                            if (updatePasswordOperation.TTL == 0) { 
                                //Operation finished or non-recoverable error occured
                                ExecutionEngine.DequeueMidTask(queue,log);
                            }
                            break;
        
                        default:
                            // Unknown Action name
                            ExecutionEngine.DequeueMidTask(queue,log);
                            break;
                    }
                }
                else
                {
                    // Empty Action name
                    ExecutionEngine.DequeueMidTask(queue,log);
                }
            }
        }

        private static ActionCall PeekMidTask(IPersistentSecureQueue queue, ILog log)
        {
            ActionCall call = null;
            try
            {
                using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
                {
                    var queueItem = queueSession.Peek();
                    if (queueItem != null)
                    {
                        call = (ActionCall)Helpers.ByteArrayToObject((byte[])queueItem, typeof(ActionCall));
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error peeking the call queue", ex);
            }
            return call;
        }

        private static ActionCall DequeueMidTask(IPersistentSecureQueue queue, ILog log)
        {
            ActionCall call = null;
            try
            {
                using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
                {
                    var queueItem = queueSession.Dequeue();
                    if (queueItem != null)
                    {
                        call = (ActionCall)Helpers.ByteArrayToObject((byte[])queueItem, typeof(ActionCall));
                        queueSession.Flush();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error dequeuing the call queue", ex);
            }
            return call;
        }


    }
}
