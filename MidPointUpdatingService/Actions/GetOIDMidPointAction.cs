using MidPointUpdatingService.Models;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MidPointUpdatingService.Actions
{
    public class GetOIDMidPointAction : IActionDefinition
    {
        public string ActionName { get { return "Get User OID"; } }

        public string ActionDefinition { get { return @"<query>
                    <filter>
                        <equal>
                            <path>name</path>
                            <value>{userName}</value>
                        </equal>
                    </filter>
                </query>"; } }

        public string ActionInterfaceUrl { get  { return @"ws/rest/users/search"; } }

        public bool ActionReturnsResult { get { return true; } }

        public IActionResult GetResult(XmlDocument xmldoc, ref MidPointError error)
        {
            if (error.ErrorCode == 0)
            {
                try
                {
                    string OID = xmldoc.FirstChild.FirstChild.Attributes.GetNamedItem("oid").Value;

                    return new GetOIDMidPointActionResult(OID, new MidPointError() { ErrorCode = MidPointErrorEnum.OK, ErrorMessage = "OK", Recoverable = true }, null);

                }
                catch (Exception ex)
                {
                    return new GetOIDMidPointActionResult(string.Empty, new MidPointError() { ErrorCode= MidPointErrorEnum.ErrorDecodingResultFromXml, ErrorMessage = ex.Message, Recoverable = false }, ex);
                }
            }
            else
            {
                return new GetOIDMidPointActionResult(string.Empty, error, null);
            }
        }

        public bool ValidateParamaters(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("userName"))
            {
                if (parameters["userName"] == null ) { return false; }
            }
            else
            {
                //not all neccessary parameters are in dictionary contained
                return false;
            }
            return true;
        }
    }
}
