using MidPointUpdatingService.Models;
using System.Collections.Generic;

namespace MidPointUpdatingService.Actions
{
    public class InvalidBaseAddressResult : IActionResult
    {
        private readonly Dictionary<string, object> _resultDictionary = new Dictionary<string, object>();

        public InvalidBaseAddressResult()
        {
            Error = new MidPointError() { ErrorCode = MidPointErrorEnum.BaseAddressError, Recoverable = false, ErrorMessage = "Invalid base address" };
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
