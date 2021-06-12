using ADPasswordSecureCache;
using MidPointUpdatingService.ClassExtensions;
using MidPointUpdatingService.Models;
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
        public static Dictionary<string, object> ExecuteMidpointTask(MidPointTask task, HttpClient client, out MidPointError error)
        {
            var results = new Dictionary<string, object>();
            if (task.TTL > 0)
            {
                task.Execute(client, out IActionResult result);
                if (result != null)
                {                    
                    error = new MidPointError() { ErrorCode = 0, Recoverable = false, ErrorMessage = "OK" };
                    results = result.resultDictionary;
                }
                else
                {

                    error = new MidPointError() { ErrorCode = 0, Recoverable = false, ErrorMessage = "OK" };
                }

            }
            else
            {
                error = new MidPointError() { ErrorCode = 100, Recoverable = false, ErrorMessage = "TTL Expired" };
            }

            return results;
        }

        public static async Task<Dictionary<string, object>> ProcessItem(HttpClient client, DiskCache<string> diskCache, PersistentSecureQueue queue)
        {
            Dictionary<string, object> output = null;
            MidPointTask task = ExecutionEngine.PeekMidTask(queue);
            if (task!=null)
            {
                var binaryFormatter = new BinaryFormatter();
                string key = ExecutionEngine.CombineCacheKey(task.ActionDefinition.ActionName, task.Parameters);
                if (diskCache.ContainsKey(key))
                {
                    Stream outStream = diskCache.GetValueAsync(key).Result;
                    output = (Dictionary<string,object>)binaryFormatter.Deserialize(outStream);
                }
                {
                    output = ExecutionEngine.ExecuteMidpointTask(task, client, out MidPointError error);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        binaryFormatter.Serialize(ms, output);
                        await diskCache.SetValueAsync(key, ms);
                    }

                }
            }
            return output;                            
        }

        private static string CombineCacheKey(string actionName, Dictionary<string, object> parameters)
        {
            StringBuilder cacheKeyBuilder = new StringBuilder();
            cacheKeyBuilder.Append(actionName);
            foreach (KeyValuePair<string,object> kvp in parameters)
            {
                cacheKeyBuilder.AppendFormat(";{0};{1}", kvp.Key, kvp.Value.ToString());
            }
            return cacheKeyBuilder.ToString();
        }

        private static void EnqueueMidTask(MidPointTask task, PersistentSecureQueue queue)
        {
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                queueSession.Enqueue(Helpers.ObjectToByteArray(task));
                queueSession.Flush();
            }
        }

        private static MidPointTask PeekMidTask(PersistentSecureQueue queue)
        {
            MidPointTask task = null;
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                task = (MidPointTask)Helpers.ByteArrayToObject(queueSession.Peek());
            }
            return task;
        }

        private static MidPointTask DequeueMidTask(PersistentSecureQueue queue)
        {
            MidPointTask task = null;
            using (IPersistentSecureQueueSession queueSession = queue.OpenSession())
            {
                task = (MidPointTask)Helpers.ByteArrayToObject(queueSession.Dequeue());
                queueSession.Flush();
            }
            return task;
        }


    }
}
