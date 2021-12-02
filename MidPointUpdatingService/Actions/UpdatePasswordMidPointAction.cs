using MidPointUpdatingService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
                            <clearValue><![CDATA[{password}]]></clearValue>
                        </t:value>
                    </itemDelta>
                </objectModification>"; } }

        public string ActionInterfaceUrl { get  { return @"ws/rest/users/{OID}"; } }

        public bool ActionReturnsResult { get { return true; } }

        public IActionResult GetResult(XmlDocument xmldoc, ref MidPointError error)
        {

            Exception ex;
            if (error.ErrorCode == MidPointErrorEnum.OK)
            {
                try
                {
                    error = new MidPointError() { ErrorCode = MidPointErrorEnum.OK, ErrorMessage = "OK", Recoverable = true };
                    return new UpdatePasswordMidPointActionResult(error, null);

                }
                catch (Exception exc)
                {
                    error = new MidPointError() { ErrorCode = MidPointErrorEnum.OK, ErrorMessage = "OK", Recoverable = true };
                    ex = exc;
                    return new UpdatePasswordMidPointActionResult(error, ex);
                }
            }
            else if (error.ErrorCode == MidPointErrorEnum.InvalidResult)
            {
                try
                {
                    XmlNodeList ml,dl;
                    if (xmldoc.DocumentElement.Attributes["xmlns"] != null)
                    {
                        string xmlns = xmldoc.DocumentElement.Attributes["xmlns"].Value;
                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmldoc.NameTable);
                        nsmgr.AddNamespace("b", xmlns);
                        ml = xmldoc.FirstChild.SelectNodes("./b:message", nsmgr);
                        dl = xmldoc.FirstChild.SelectNodes("./b:details", nsmgr);
                    }
                    else
                    {
                        ml = xmldoc.FirstChild.SelectNodes("./message");
                        dl = xmldoc.FirstChild.SelectNodes("./details");
                    }

                    //Get all messages
                    StringBuilder errroMessageSb = new StringBuilder();
                    foreach (XmlNode messageChild in ml)
                    {
                        errroMessageSb.Append("M:");
                        errroMessageSb.AppendLine(messageChild.InnerText);
                    }
                    string errroMessage = errroMessageSb.ToString();                    
                    
                    //Get all details
                    StringBuilder errroDetailsSb = new StringBuilder();
                    foreach (XmlNode detailChild in dl)
                    {
                        errroDetailsSb.Append("D:");
                        errroDetailsSb.AppendLine(detailChild.InnerText);
                    }
                    string errroDetail = errroDetailsSb.ToString();

                    InvalidOperationException ioex = new InvalidOperationException(errroDetail);

                    error.ErrorMessage =  String.IsNullOrEmpty(errroMessage)?"Detailed message has not been found":errroMessage;

                    return new UpdatePasswordMidPointActionResult(error, ioex);

                }
                catch (Exception exc)
                {                    
                    error.ErrorMessage = "Error parsing Midpoint OperationResult for detailed error message";
                    return new UpdatePasswordMidPointActionResult(error, exc);
                }
            }
            else
            {
                string fileStorePath = Path.GetTempFileName();
                fileStorePath = Path.ChangeExtension(fileStorePath, "xml");
                xmldoc.Save(fileStorePath);
                error.ErrorMessage = error.ErrorMessage + " - " + fileStorePath;
                ex = new Exception(error.ErrorMessage);
            }
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
