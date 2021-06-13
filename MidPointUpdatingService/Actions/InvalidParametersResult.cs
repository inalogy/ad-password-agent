using MidPointUpdatingService.Models;
using System.Collections.Generic;

namespace MidPointUpdatingService.Actions
{
    public class InvalidParametersResult : IActionResult
    {
        private readonly Dictionary<string, object> _resultDictionary = new Dictionary<string, object>();

        public InvalidParametersResult()
        {
            Error = new MidPointError() { ErrorCode=MidPointErrorEnum.ParametersError, Recoverable=false, ErrorMessage="Invaled parameters" } ;
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
