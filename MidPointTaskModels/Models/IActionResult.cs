using System.Collections.Generic;

namespace MidPointUpdatingService.Models
{
    public interface IActionResult
    {
        int ErrorCode { get; }
        Dictionary<string, object> resultDictionary { get; }
    }
}
