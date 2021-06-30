using System;
using System.Collections.Generic;
using MidPointUpdatingService.ClassExtensions;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MidPointUpdatingService.Actions;

namespace MidPointUpdatingService.Models
{
    [Serializable]
    public class MidPointAction
    {
        #region Private Parameters
        
        private readonly Dictionary<string, object> paramaters = new Dictionary<string, object>();

        #endregion

        #region Public properties ActionDefinition and Parameters

        public Dictionary<string, object> Parameters { get { return paramaters; } }

        public IActionDefinition ActionDefinition { get; set; }

        #endregion

        #region Default MidPoint task constructor

        public MidPointAction(IActionDefinition action, Dictionary<string, object> par)
        {
            ActionDefinition = action;
            if (par != null)
            {
                foreach (string k in par.Keys)
                {
                   paramaters.Add(k, par[k]);
                }
            }
        }

        #endregion

        #region Method Execute of the task

        public bool Execute(HttpClient client, out IActionResult result)
        {
            if (ActionDefinition.ValidateParamaters(Parameters))
            {
                string query = ActionDefinition.ActionDefinition.FormatDict(Parameters);
                string relativeapiUrl = ActionDefinition.ActionInterfaceUrl.FormatDict(Parameters);

                HttpContent content = new StringContent(query, Encoding.UTF8);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
                try
                {
                    HttpResponseMessage response = client.PostAsync(relativeapiUrl, content).Result;

                    if (ActionDefinition.ActionReturnsResult && response.IsSuccessStatusCode)
                    {
                        try
                        {
                            string xmlobj = response.Content.ReadAsStringAsync().Result;

                            // get oid from returned user object
                            XmlDocument xmldoc = new XmlDocument();
                            xmldoc.LoadXml(xmlobj);
                            MidPointError error = new MidPointError() { ErrorCode =  MidPointErrorEnum.OK, Recoverable = false, ErrorMessage = "OK" };
                            result = ActionDefinition.GetResult(xmldoc, error);
                            return (result.Error.ErrorCode == 0);
                        }
                        catch (Exception ex)
                        {
                            result = new NetworkCommunicationErrorResult(ex.Message, ex);
                            return false;
                        }
                    }
                    if (!ActionDefinition.ActionReturnsResult)
                    {
                        result = response.IsSuccessStatusCode ? new SuccessResult() : null;
                    }
                    else result = null;

                    return response.IsSuccessStatusCode;
                }
                catch (InvalidOperationException ioex)
                {
                    result = new InvalidBaseAddressResult(ioex);
                    return false;
                }
                catch (HttpRequestException hrex)
                {
                    result = new NetworkCommunicationErrorResult(hrex.Message, hrex);
                    return false;
                }
                catch (TaskCanceledException tcex)
                {
                    result = new NetworkCommunicationErrorResult(tcex.Message, tcex);
                    return false;
                }
                catch (Exception ex)
                {
                    result = new NetworkCommunicationErrorResult(ex.Message, ex);
                    return false;
                }
            }
            else 
            {
                //Inavalid paramaters
                result = new InvalidParametersResult(new Exception("Invalid operation parameters values"));
                return false;
            }      
            
        }

        #endregion
    }
}
