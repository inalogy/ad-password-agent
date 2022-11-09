using log4net;
using MidPointCommonTaskModels.Models;
using MidPointUpdatingService.ClassExtensions;
using MidPointUpdatingService.Models;
using MidPointUpdatingService.Operations;
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
        // lock
        private static Object lockObj = new Object();

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

        public static bool ProcessHeapItem(ActionCall call, HttpClient client, ILog log, int retryCount, CancellationToken token)
        {
            if (call != null)
            {
                if (!string.IsNullOrEmpty(call.ActionName))
                {
                    switch (call.ActionName)
                    {
                        case "UpdatePassword":
                            IMidPointOperation updatePasswordOperation = new UpdatePasswordOperation(retryCount);
                            updatePasswordOperation.ExecuteOperation(call.Parameters, client, log, token);
                            return (updatePasswordOperation.TTL == 0);

                        default:
                            // Unknown Action name
                            break;
                    }
                }
            }
            return false;
        }

        public static DirectoryInfo EnsureHeapDirectory(string heapPath)
        {
            lock(lockObj)
            {
                if (!Directory.Exists(heapPath))
                {
                    return Directory.CreateDirectory(heapPath);

                }
                else
                {
                    return new DirectoryInfo(heapPath);
                }
            }
        }
        
        public static bool EnqueueHeapItem(HttpClient client, ILog log, int retryCount, string cqueuefld, CancellationToken token)
        {
            bool result = true;
            string heapPath = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.CommonApplicationData), cqueuefld + ".Heap");
            try
            {
                DirectoryInfo di = ExecutionEngine.EnsureHeapDirectory(heapPath);
                if (di == null)
                {
                    throw new Exception(String.Format("Directory {0} has not been created from unknown reason.", heapPath));
                }

                string firstFileName =
                    di.GetFiles()
                    .Where(f => f.Extension == ".itq")
                    .OrderBy(f => f.CreationTime.ToString("yyyyMMddHHmmssfff"))
                    .Select(fi => fi.FullName)
                    .FirstOrDefault();
                if (!String.IsNullOrEmpty(firstFileName))
                {
                    ProcessChangeInMidpoint(client, log, retryCount, firstFileName, token);
                }

            }
            catch (Exception ex)
            {
                log.Error(String.Format("Error creating heap folder for AD Password agent: {0}", ex.Message), ex);
                result = false;
            }
            return result;
        }

        private static void ProcessChangeInMidpoint(HttpClient client, ILog log, int retryCount, string fileName, CancellationToken token)
        {
            try
            {
                var protectedHeapItem = File.ReadAllBytes(fileName);
                var heapItem = ProtectedData.Unprotect(protectedHeapItem, null, DataProtectionScope.LocalMachine);
                ActionCall call = (ActionCall)Helpers.ByteArrayToObject(heapItem, typeof(ActionCall));
                if (ProcessHeapItem(call, client, log, retryCount, token))
                {
                    File.Delete(fileName);
                } else
                {
                    // rename file 
                    string errFileName = $"{fileName}.err";
                    File.Move(fileName, errFileName);
                }
                
            }
            catch (Exception ex)
            {
                log.Error(String.Format("Unable to process heap file {0} in midpoint with error {1}", fileName, ex.Message), ex);
                // rename file 
                string errFileName = $"{fileName}.err";
                try
                {
                    File.Move(fileName, errFileName);
                } catch (Exception moveEx)
                {
                    log.Error(String.Format("Unable to process heap file {0} move to errors failed: {1}", fileName, moveEx.Message), moveEx);
                }
                
            }
        }
    }
}
