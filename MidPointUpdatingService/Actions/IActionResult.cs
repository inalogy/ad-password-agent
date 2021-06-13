using System.Collections.Generic;

namespace MidPointUpdatingService.Models
{
    public interface IActionResult
    {
        MidPointError Error { get; }
        Dictionary<string, object> ResultDictionary { get; }
    }
}
