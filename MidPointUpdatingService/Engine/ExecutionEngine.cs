using log4net;
using MidPointCommonTaskModels.Models;
using MidPointUpdatingService.ClassExtensions;
using MidPointUpdatingService.Models;
using MidPointUpdatingService.Operations;
using SecureDiskQueue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;

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

        public static void ProcessItem(HttpClient client, ILog log, int retryCount, string queueName, int queueWait, CancellationToken token)
        {
            ActionCall call = ExecutionEngine.PeekMidTask(queueName, queueWait, log);
            if (call!=null)
            {
                if (!string.IsNullOrEmpty(call.ActionName))
                {
                    switch (call.ActionName)
                    {
                        case "UpdatePassword":
                            IMidPointOperation updatePasswordOperation = new UpdatePasswordOperation(retryCount);
                            updatePasswordOperation.ExecuteOperation(call.Parameters, client, log, token);
                            if (updatePasswordOperation.TTL == 0) { 
                                //Operation finished or non-recoverable error occured
                                ExecutionEngine.DequeueMidTask(queueName, queueWait, log);
                            }
                            break;
        
                        default:
                            // Unknown Action name
                            ExecutionEngine.DequeueMidTask(queueName, queueWait, log);
                            break;
                    }
                }
                else
                {
                    // Empty Action name
                    ExecutionEngine.DequeueMidTask(queueName, queueWait, log);
                }
            }
        }

        private static ActionCall PeekMidTask(string queueName,int queueWait,  ILog log)
        {
            ActionCall call = null;
            try
            {
                using (IPersistentSecureQueue queue = PersistentSecureQueue.WaitFor(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), queueName), TimeSpan.FromSeconds(queueWait)))
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
            }
            catch (Exception ex)
            {
                log.Error("Error peeking the call queue", ex);
            }
            return call;
        }

        private static ActionCall DequeueMidTask(string queueName,int queueWait, ILog log)
        {
            ActionCall call = null;
            try
            {
                using (IPersistentSecureQueue queue = PersistentSecureQueue.WaitFor(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), queueName), TimeSpan.FromSeconds(queueWait)))
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
            }
            catch (Exception ex)
            {
                log.Error("Error dequeuing the call queue", ex);
            }
            return call;
        }


    }
}
