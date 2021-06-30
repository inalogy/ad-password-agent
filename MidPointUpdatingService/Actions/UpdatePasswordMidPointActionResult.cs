using MidPointUpdatingService.Models;
using System;
using System.Collections.Generic;

namespace MidPointUpdatingService.Actions
{
    public class UpdatePasswordMidPointActionResult : IActionResult
    {
        private readonly Dictionary<string, object> _resultDictionary = new Dictionary<string, object>();

        public UpdatePasswordMidPointActionResult(MidPointError error, Exception ex)
        {
            Error = error;
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
