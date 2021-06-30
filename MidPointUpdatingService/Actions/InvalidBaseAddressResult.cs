using MidPointUpdatingService.Models;
using System;
using System.Collections.Generic;

namespace MidPointUpdatingService.Actions
{
    public class InvalidBaseAddressResult : IActionResult
    {
        private readonly Dictionary<string, object> _resultDictionary = new Dictionary<string, object>();

        public InvalidBaseAddressResult(Exception ex)
        {
            Error = new MidPointError() { ErrorCode = MidPointErrorEnum.BaseAddressError, Recoverable = false, ErrorMessage = "Invalid base address" };
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
