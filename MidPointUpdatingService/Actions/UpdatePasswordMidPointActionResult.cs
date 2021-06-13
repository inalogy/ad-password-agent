using MidPointUpdatingService.Models;
using System.Collections.Generic;

namespace MidPointUpdatingService.Actions
{
    public class UpdatePasswordMidPointActionResult : IActionResult
    {
        private readonly Dictionary<string, object> _resultDictionary = new Dictionary<string, object>();

        public UpdatePasswordMidPointActionResult(MidPointError error)
        {
            Error = error;
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
