using System;
using System.Collections.Generic;

namespace MidPointCommonTaskModels.Models
{
    [Serializable]
    public class ActionCall
    {
        public ActionCall(string actionName, Dictionary<string, object> parameters)
        {
            ActionName = actionName;
            Parameters = parameters;
        }

        public string ActionName { get; }
        public Dictionary<string, object> Parameters { get; }
    }
}
