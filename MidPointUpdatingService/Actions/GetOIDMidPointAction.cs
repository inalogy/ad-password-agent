using MidPointUpdatingService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

            Exception ex;
            if (error.ErrorCode == MidPointErrorEnum.OK)
            {
                try
                {
                    string OID = string.Empty;
                    if (xmldoc != null && xmldoc.FirstChild != null && xmldoc.FirstChild.FirstChild != null)
                    { 
                        OID = xmldoc.FirstChild.FirstChild.Attributes.GetNamedItem("oid").Value;
                        error = new MidPointError() { ErrorCode = MidPointErrorEnum.OK, ErrorMessage = "OK", Recoverable = true };
                    }
                    else
                    {
                        error = new MidPointError() { ErrorCode = MidPointErrorEnum.NotFoundError, ErrorMessage = String.Format("Username not found"), Recoverable = false };
                        
                    }

                    return new GetOIDMidPointActionResult(OID, error, null);

                }
                catch (Exception exc)
                {
                    ex = exc;
                    error = new MidPointError() { ErrorCode = MidPointErrorEnum.ErrorDecodingResultFromXml, ErrorMessage = ex.Message, Recoverable = false };
                    return new GetOIDMidPointActionResult(string.Empty,error, ex);
                }
            }
            else if (error.ErrorCode == MidPointErrorEnum.InvalidResult)
            {
                try
                {
                    XmlNodeList ml, dl;
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

                    error.ErrorMessage = String.IsNullOrEmpty(errroMessage) ? "Detailed message has not been found" : errroMessage;

                    return new GetOIDMidPointActionResult(string.Empty, error, ioex);

                }
                catch (Exception exc)
                {
                    error.ErrorMessage = "Error parsing Midpoint OperationResult for detailed error message";
                    return new GetOIDMidPointActionResult(string.Empty, error, exc);
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
            return new GetOIDMidPointActionResult(string.Empty, error,ex);
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
