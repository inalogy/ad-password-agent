using MidPointUpdatingService.Models;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MidPointUpdatingService.Actions
{
    public class UpdatePasswordMidPointAction : IActionDefinition
    {
        public string ActionName { get { return "Update User Password"; } }

        public string ActionDefinition { get { return @"<objectModification
                    xmlns='http://midpoint.evolveum.com/xml/ns/public/common/api-types-3'
                    xmlns:c='http://midpoint.evolveum.com/xml/ns/public/common/common-3'
                    xmlns:t='http://prism.evolveum.com/xml/ns/public/types-3'>
                    <itemDelta>
                        <t:modificationType>replace</t:modificationType>
                        <t:path>c:credentials/c:password/c:value</t:path>
                        <t:value>
                            <clearValue>{password}</clearValue>
                        </t:value>
                    </itemDelta>
                </objectModification>"; } }

        public string ActionInterfaceUrl { get  { return @"ws/rest/users/{OID}"; } }

        public bool ActionReturnsResult { get { return false; } }

        public IActionResult GetResult(XmlDocument xmldoc, MidPointError error)
        {
            Exception ex = null;
            if ( error.ErrorCode>0)
                { ex = new Exception(error.ErrorMessage); }
                
            return new UpdatePasswordMidPointActionResult(error, ex);
        }

        public bool ValidateParamaters(Dictionary<string, object> parameters)
        {
            if (parameters.ContainsKey("OID") && parameters.ContainsKey("password"))
            {
                if (parameters["OID"] == null || parameters["password"] == null) { return false; }
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
