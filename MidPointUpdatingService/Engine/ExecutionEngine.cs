using log4net;
using MidPointCommonTaskModels.Models;
using MidPointUpdatingService.ClassExtensions;
using MidPointUpdatingService.Models;
using MidPointUpdatingService.Operations;
using SecureDiskQueue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;

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
                error.CurrentException = result.CurrentException;
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
                using (IPersistentSecureQueue queue = PersistentSecureQueue.WaitFor(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), queueName), TimeSpan.FromSeconds(queueWait)))
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
            catch (TimeoutException tex)
            {
                log.Warn("Timeout dequeuing the call queue", tex);
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
                using (IPersistentSecureQueue queue = PersistentSecureQueue.WaitFor(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), queueName), TimeSpan.FromSeconds(queueWait)))
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
            catch (TimeoutException tex)
            {
                log.Warn("Timeout dequeuing the call queue", tex);
            }
            catch (Exception ex)
            {
                log.Error("Error dequeuing the call queue", ex);
            }
            return call;
        }

        public static bool EnqueueHeapItem(ILog log, string cqueuefld, int queuewait, CancellationToken token)
        {
            bool result = true;
            string heapPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), cqueuefld + ".Heap");
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
                if (di == null)
                {
                    throw new Exception(String.Format("Directory {0} has not been created from unknown reason.", heapPath));
                }

                string firstFileName =
                    di.GetFiles()
                    .Where(f => f.Extension == ".itq")
                    .OrderBy(f => f.Name)
                    .Select(fi => fi.FullName)
                    .FirstOrDefault();

                if (!String.IsNullOrEmpty(firstFileName))
                {
                    TakeFile(log, firstFileName, cqueuefld, queuewait);
                }

            }
            catch (Exception ex)
            {
                log.Error(String.Format("Error creating heap folder for AD Password agent: {0}", ex.Message), ex);
                result = false;
            }
            return result;
        }

        private static void EnqueueMidTask(ActionCall task, IPersistentSecureQueue queue)
        {
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                var queueItem = Helpers.ObjectToByteArray(task);
                queueSession.Enqueue(queueItem);
                queueSession.Flush();
            }
        }

        private static void TakeFile(ILog log,string fileName, string cqueuefld, int queuewait)
        {
            try
            {
                var protectedHeapItem = File.ReadAllBytes(fileName);
                var heapItem = ProtectedData.Unprotect(protectedHeapItem, null, DataProtectionScope.LocalMachine);
                ActionCall call = (ActionCall)Helpers.ByteArrayToObject(heapItem, typeof(ActionCall));

                using (IPersistentSecureQueue queue = PersistentSecureQueue.WaitFor(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), cqueuefld), TimeSpan.FromSeconds(queuewait)))
                {
                    EnqueueMidTask(call, queue);
                }
                File.Delete(fileName);
            }
            catch (Exception ex)
            {
                log.Error(String.Format("Unable to process heap file {0} with error {1}", fileName, ex.Message), ex);
            }
        }
    }
}
