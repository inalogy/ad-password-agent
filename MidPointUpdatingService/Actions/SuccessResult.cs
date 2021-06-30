using MidPointUpdatingService.Models;
using System;
using System.Collections.Generic;

namespace MidPointUpdatingService.Actions
{
    public class SuccessResult : IActionResult
    {
        private readonly Dictionary<string, object> _resultDictionary = new Dictionary<string, object>();

        public SuccessResult()
        {
            Error = new MidPointError() { ErrorCode = MidPointErrorEnum.OK, Recoverable = false, ErrorMessage = "OK" };
        }

        public Dictionary<string, object> ResultDictionary
        {
            get
            {
                return _resultDictionary;
            }
        }

        public MidPointError Error { get; }

        public Exception CurrentException { get { return null; } }
    }
}
