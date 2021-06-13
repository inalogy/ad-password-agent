using MidPointUpdatingService.Actions;
using MidPointUpdatingService.ClassExtensions;
using MidPointUpdatingService.Engine;
using MidPointUpdatingService.Models;
using System.Collections.Generic;
using System.Net.Http;

namespace MidPointUpdatingService.Operations
{
    public class UpdatePasswordOperation : IMidPointOperation
    {
        Dictionary<string, object> output = null;

        public int TTL { get; set; }

        public string OperationName { get { return "UpdatePassword"; } }

        public void ExecuteOperation(Dictionary<string, object> parameters, HttpClient client)
        {
            GetOIDMidPointAction getOIDMidPointAction = new GetOIDMidPointAction();
            MidPointAction getOIDAction = new MidPointAction(getOIDMidPointAction, parameters);
            output = ExecutionEngine.ExecuteMidpointAction(getOIDAction, client, out MidPointError error);
            if (error.ErrorCode == MidPointErrorEnum.OK)
            {
                parameters.Combine(output);
                UpdatePasswordMidPointAction updatePasswordMidPointAction = new UpdatePasswordMidPointAction();
                MidPointAction updatePasswordAction = new MidPointAction(updatePasswordMidPointAction, parameters);
                output = ExecutionEngine.ExecuteMidpointAction(updatePasswordAction, client, out error);
                if (error.ErrorCode != MidPointErrorEnum.OK)
                {
                    if (error.Recoverable) TTL--;
                    else TTL = 0;
                    // Propagate MidPointError
                }
                else TTL = 0;
            }
            else
            {
                if (error.Recoverable) TTL--;
                else TTL = 0;
                // Propagate MidPointError
            }
        }
    }
}
