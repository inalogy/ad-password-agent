using MidPointUpdatingService.Models;
using System;
using System.Collections.Generic;

namespace MidPointUpdatingService.Actions
{
    public class InvalidReturnedResult : IActionResult
    {
        private readonly Dictionary<string, object> _resultDictionary = new Dictionary<string, object>();

        public InvalidReturnedResult(string message, Exception ex)
        {
            Error = new MidPointError() { ErrorCode = MidPointErrorEnum.InvalidResult, Recoverable = false, ErrorMessage = string.Format("Invalid returned result: {0}", message) };
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
