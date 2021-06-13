using System.Collections.Generic;
using System.Xml;

namespace MidPointUpdatingService.Models
{
    public interface IActionDefinition
    {
        string ActionName { get; }
        string ActionDefinition { get; }
        string ActionInterfaceUrl { get; }
        bool ActionReturnsResult { get; }
        bool ValidateParamaters(Dictionary<string, object> parameters);
        IActionResult GetResult(XmlDocument xmldoc, MidPointError error);
    }
}
