﻿using System;
using System.Collections.Generic;
using MidPointUpdatingService.ClassExtensions;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using MidPointUpdatingService.Actions;
using System.IO;
using Common;

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
                    var postTask = client.PostAsync(relativeapiUrl, content);
                    postTask.Wait();
                    HttpResponseMessage response = postTask.Result;

                    if (ActionDefinition.ActionReturnsResult && response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var readResponseTask = response.Content.ReadAsStringAsync();
                            readResponseTask.Wait();
                            string xmlobj = readResponseTask.Result;

                            if (!String.IsNullOrEmpty(xmlobj))
                            {
                                // get results from returned object
                                XmlDocument xmldoc = XmlDocumentParsingHelper.CreateXMLDocumentFromString(xmlobj, false);
                                MidPointError error = new MidPointError() { ErrorCode = MidPointErrorEnum.OK, Recoverable = false, ErrorMessage = "OK" };
                                result = ActionDefinition.GetResult(xmldoc, ref error);
                                return (result.Error.ErrorCode == MidPointErrorEnum.OK);
                            }
                            else
                            {
                                result = new SuccessResult();
                                return true;
                            }
                        }
                        catch (XmlException ex)
                        {
                            result = new InvalidReturnedResult(String.Format("MidPoint returned invalid XML in response - {0}{1}", ex.Message , "- response debug loging disabled"), ex);
                            return false;
                        }
                        catch (Exception ex)
                        {
                            result = new InvalidReturnedResult(ex.Message, ex);
                            return false;
                        }
                    }
                    if (!response.IsSuccessStatusCode)
                    {
                        try
                        {
                            var readResponseTask = response.Content.ReadAsStringAsync();
                            readResponseTask.Wait();
                            string xmlobj = readResponseTask.Result;

                            if (!String.IsNullOrEmpty(xmlobj))
                            {
                                // get detailed error from returned operation result object
                                XmlDocument xmldoc = XmlDocumentParsingHelper.CreateXMLDocumentFromString(xmlobj, false);
                                MidPointError error = new MidPointError() { ErrorCode = MidPointErrorEnum.InvalidResult, Recoverable = false, ErrorMessage = response.ReasonPhrase };
                                result = ActionDefinition.GetResult(xmldoc, ref error);                                
                                return (result.Error.ErrorCode == MidPointErrorEnum.OK);
                            }
                            else
                            {
                                MidPointError error = new MidPointError() { ErrorCode = MidPointErrorEnum.InvalidResult, Recoverable = false, ErrorMessage = response.ReasonPhrase };
                                result = new InvalidReturnedResult(error.ErrorMessage, new InvalidOperationException(error.ErrorMessage));
                                return false;
                            }
                        }
                        catch (XmlException ex)
                        {
                            result = new InvalidReturnedResult(String.Format("MidPoint returned invalid XML in response - {0}{1}", ex.Message, ""), ex);
                            return false;
                        }
                        catch (Exception ex)
                        {
                            result = new InvalidReturnedResult(ex.Message, ex);
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
