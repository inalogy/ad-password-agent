using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MidPointUpdatingService.Models
{
    [Serializable]
    public class MidPointTask
    {
        #region Private variables TTL and Parameters
        
        private int ttl = 0;

        private readonly Dictionary<string, object> paramaters = new Dictionary<string, object>();

        #endregion

        #region Public properties ActionDefinition, TTL and Parameters

        public Dictionary<string, object> Parameters { get { return paramaters; } }

        public IActionDefinition ActionDefinition { get; set; }

        public int TTL { get { return ttl; } }

        #endregion

        #region Default MidPoint task constructor

        public MidPointTask(IActionDefinition action,int ttl,Dictionary<string, object> par)
        {
            ActionDefinition = action;
            this.ttl = ttl;
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

                HttpResponseMessage response = client.PostAsync(relativeapiUrl, content).Result;
                ttl--;
                if (ActionDefinition.ActionReturnsResult && response.IsSuccessStatusCode)
                {
                    try
                    {
                        string xmlobj = response.Content.ReadAsStringAsync().Result;

                        // get oid from returned user object
                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.LoadXml(xmlobj);
                        MidPointError error = new MidPointError() { ErrorCode = 0, Recoverable = false, ErrorMessage = "OK" };
                        result = ActionDefinition.GetResult(xmldoc, error);
                        return (result.ErrorCode == 0);
                    }
                    catch 
                    {
                        result = null;
                        return false;
                    }
                }
                result = null;
                return response.IsSuccessStatusCode;
            }
            else 
            {
                //Inavalid paramaters
                ttl = 0;
                result = null;
                return false;
            }                 
        }

        #endregion
    }
}
