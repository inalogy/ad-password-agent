using MidPointUpdatingService.Models;
using System.Collections.Generic;

namespace MidPointUpdatingService.Actions
{
    public class NetworkCommunicationErrorResult : IActionResult
    {
        private readonly Dictionary<string, object> _resultDictionary = new Dictionary<string, object>();

        public NetworkCommunicationErrorResult(string message)
        {
            Error = new MidPointError() { ErrorCode = MidPointErrorEnum.NetworkCommunicationError, Recoverable = true, ErrorMessage = string.Format("Network communication error: {0}", message )};
        }

        public Dictionary<string, object> ResultDictionary
        {
            get
            {
                return _resultDictionary;
            }
        }

        public MidPointError Error { get; }
    }
}


