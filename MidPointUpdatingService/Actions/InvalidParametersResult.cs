using MidPointUpdatingService.Models;
using System;
using System.Collections.Generic;

namespace MidPointUpdatingService.Actions
{
    public class InvalidParametersResult : IActionResult
    {
        private readonly Dictionary<string, object> _resultDictionary = new Dictionary<string, object>();

        public InvalidParametersResult(Exception ex)
        {
            Error = new MidPointError() { ErrorCode=MidPointErrorEnum.ParametersError, Recoverable=false, ErrorMessage="Invaled parameters" } ;
            CurrentException = ex;
        }

        public Dictionary<string, object> ResultDictionary
        {
            get
            {
                return _resultDictionary;
            }
        }

        public MidPointError Error { get; }

        public Exception CurrentException { get; }
    }
}
