using MidPointUpdatingService.Models;
using System.Collections.Generic;

namespace MidPointUpdatingService.Actions
{
    public class GetOIDMidPointActionResult : IActionResult
    {
        private readonly Dictionary<string, object> _resultDictionary = new Dictionary<string, object>();

        public GetOIDMidPointActionResult(string oid, int error)
        {
            _resultDictionary.Add("OID", oid);
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
