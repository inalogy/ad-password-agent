using log4net;
using MidPointUpdatingService.Actions;
using MidPointUpdatingService.ClassExtensions;
using MidPointUpdatingService.Engine;
using MidPointUpdatingService.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;

namespace MidPointUpdatingService.Operations
{
    public class UpdatePasswordOperation : IMidPointOperation
    {
        Dictionary<string, object> output = null;

        public int TTL { get; set; }

        private int originalTTL; 

        public string OperationName { get { return "UpdatePassword"; } }

        public UpdatePasswordOperation(int ttl)
        {
            TTL = ttl;
            originalTTL = ttl;
        }

        public void ExecuteOperation(Dictionary<string, object> parameters, HttpClient client, ILog log)
        {
            while (TTL > 0)
            {

                GetOIDMidPointAction getOIDMidPointAction = new GetOIDMidPointAction();
                MidPointAction getOIDAction = new MidPointAction(getOIDMidPointAction, parameters);
                output = ExecutionEngine.ExecuteMidpointAction(getOIDAction, client, log, out MidPointError error);
                if (error.ErrorCode == MidPointErrorEnum.OK)
                {
                    parameters.Combine(output);
                    UpdatePasswordMidPointAction updatePasswordMidPointAction = new UpdatePasswordMidPointAction();
                    MidPointAction updatePasswordAction = new MidPointAction(updatePasswordMidPointAction, parameters);
                    output = ExecutionEngine.ExecuteMidpointAction(updatePasswordAction, client, log, out error);
                    if (error.ErrorCode != MidPointErrorEnum.OK)
                    {
                        if (error.Recoverable)
                        {
                            log.Warn(string.Format("{0}:{1}:{2} failed with {4}:{5} - {3} retry attempts left", this.OperationName, updatePasswordAction.ActionDefinition.ActionName, parameters["userName"].ToString(), this.TTL, error.ErrorCode, error.ErrorMessage));
                            TTL--;
                        }
                        else
                        {
                            log.Error(string.Format("{0}:{1}:{2} failed with {3}:{4}", this.OperationName, updatePasswordAction.ActionDefinition.ActionName, parameters["userName"].ToString(), error.ErrorCode, error.ErrorMessage));
                            TTL = 0;
                        }
                        // Propagate MidPointError
                    }
                    else TTL = 0;
                }
                else
                {
                    if (error.Recoverable)
                    {
                        log.Warn(string.Format("{0}:{1}:{2} failed with {4}:{5} - {3} retry attempts left", this.OperationName, getOIDAction.ActionDefinition.ActionName, parameters["userName"].ToString(), this.TTL, error.ErrorCode, error.ErrorMessage));
                        TTL--;
                    }
                    else
                    {
                        log.Error(string.Format("{0}:{1}:{2} failed with {3}:{4}", this.OperationName, getOIDAction.ActionDefinition.ActionName, parameters["userName"].ToString(), error.ErrorCode, error.ErrorMessage));
                        TTL = 0;
                    }
                    // Propagate MidPointError
                }
                ExponentialDelay();
            }
        }

        private void ExponentialDelay()
        {
            // Delay by ttl
            int delaytimems = (2 ^((originalTTL - TTL)/(originalTTL/24)));
            Thread.Sleep(delaytimems);
        }
    }
}
