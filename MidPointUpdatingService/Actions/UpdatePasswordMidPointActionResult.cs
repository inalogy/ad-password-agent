using MidPointUpdatingService.Models;
using System.Collections.Generic;

namespace MidPointUpdatingService.Actions
{
    public class UpdatePasswordMidPointActionResult : IActionResult
    {
        private readonly Dictionary<string, object> _resultDictionary = new Dictionary<string, object>();

        public UpdatePasswordMidPointActionResult(int error)
        {
            ErrorCode = error;
        }

        public Dictionary<string, object> resultDictionary 
        {
            get
            {                
                return _resultDictionary;
            }
        }

        public int ErrorCode { get; } = 0;
    }
}
