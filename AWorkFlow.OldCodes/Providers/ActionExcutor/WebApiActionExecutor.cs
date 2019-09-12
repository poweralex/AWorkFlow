﻿using Mcs.SF.Common.ServiceProviders;
using Mcs.SF.Common.ServiceProviders.CommonModel;
using Mcs.SF.WorkFlow.Api.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Mcs.SF.WorkFlow.Api.Providers.ActionExcutor
{
    /// <summary>
    /// call web api type action executor
    /// </summary>
    public class WebApiActionExecutor : IActionExecutor
    {
        internal WebApiActionSetting Settings { get; set; }

        /// <summary>
        /// execute call
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public async Task<ActionExecuteResult> Execute(ArgumentProvider argument)
        {
            try
            {
                if (Settings == null)
                {
                    return new ActionExecuteResult { Success = false };
                }
                var call = HttpProvider.CallUrl(argument.Format(Settings.Url));
                if (Settings.Headers?.Any() == true)
                {
                    foreach (var kvp in Settings.Headers)
                    {
                        call.WithHeader(kvp.Key, argument.Format(kvp.Value));
                    }
                }
                call.AcceptStatusCodes(Settings.SuccessStatusCodes.Select(x => (HttpStatusCode)x).ToArray());
                if (!string.IsNullOrEmpty(Settings.Body))
                {
                    call.WithJsonBody(argument.Format(Settings.Body).ToJsonObject());
                }
                if (Settings.FormData?.Any() == true)
                {
                    foreach (var kvp in Settings.FormData)
                    {
                        call.WithHttpParameter(kvp.Key, kvp.Value);
                    }
                }
                OperationResult<string> result = null;
                if ("get".Equals(Settings.Method, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = await call.GetAsync();
                }
                else if ("post".Equals(Settings.Method, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = await call.PostAsync<string>();
                }
                else if ("put".Equals(Settings.Method, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = await call.PutAsync<string>();
                }
                else if ("delete".Equals(Settings.Method, StringComparison.CurrentCultureIgnoreCase))
                {
                    result = await call.DeleteAsync<string>();
                }

                if (result?.Success != true)
                {
                    return new ActionExecuteResult { Success = result?.Success ?? false, Message = result?.Message, Data = result?.Data };
                }

                var resultBody = result.Data;
                if (!string.IsNullOrEmpty(resultBody))
                {
                    argument.Put("result", resultBody);
                }
                ActionExecuteResult actionExecuteResult = new ActionExecuteResult { Data = resultBody };
                if (Settings.ResultIndicators?.Any() == true)
                {
                    foreach (var indicator in Settings.ResultIndicators)
                    {
                        var res = indicator.Indicate(argument);
                        if (res == null)
                        {
                            continue;
                        }
                        if (res == true)
                        {
                            actionExecuteResult.Success = true;
                            break;
                        }
                        if (res == false)
                        {
                            actionExecuteResult.Fail = true;
                            break;
                        }
                    }
                }
                else
                {
                    actionExecuteResult.Success = true;
                }

                //// process output
                //Dictionary<string, string> output = new Dictionary<string, string>();
                //if (Settings.Output?.Any() == true)
                //{
                //    foreach (var kvp in Settings.Output)
                //    {
                //        output[kvp.Key] = argument.Format(kvp.Value);
                //    }
                //}

                //actionExecuteResult.Output = output;
                return actionExecuteResult;
            }
            catch (Exception ex)
            {
                return new ActionExecuteResult { Success = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// initialize setting
        /// </summary>
        /// <param name="actionSetting"></param>
        public void InitializeSetting(string actionSetting)
        {
            Settings = JsonConvert.DeserializeObject<WebApiActionSetting>(actionSetting);
        }
    }

    /// <summary>
    /// web api setting
    /// </summary>
    public class WebApiActionSetting
    {
        /// <summary>
        /// http verb
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// url
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// headers
        /// </summary>
        public Dictionary<string, string> Headers { get; set; }
        /// <summary>
        /// form data
        /// </summary>
        public Dictionary<string,string> FormData { get; set; }
        /// <summary>
        /// body
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// list of status codes which will be accept as success
        /// </summary>
        public List<int> SuccessStatusCodes { get; set; } = new List<int> { 200 };
        /// <summary>
        /// result indicator(s)
        /// </summary>
        public List<ResultIndicator> ResultIndicators { get; set; }
    }

    /// <summary>
    /// result indicator setting
    /// </summary>
    public class ResultIndicator
    {
        /// <summary>
        /// actual value
        /// </summary>
        public string Actual { get; set; }
        /// <summary>
        /// expected value
        /// </summary>
        public List<string> Expected { get; set; }
        /// <summary>
        /// logical negation
        /// </summary>
        public bool Not { get; set; }
        /// <summary>
        /// treat as success while compare pass
        /// </summary>
        public bool IsSuccess { get; set; }
        /// <summary>
        /// treat as fail while compare pass
        /// </summary>
        public bool IsFail { get; set; }

        /// <summary>
        /// execute indicator
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public bool? Indicate(ArgumentProvider argument)
        {
            var actual = argument.Format(Actual);
            var expected = Expected.Select(x => argument.Format(x));

            var match = expected.Any(x => string.Equals(actual, x, StringComparison.CurrentCultureIgnoreCase));
            if (Not)
            {
                match = !match;
            }

            if (match)
            {
                if (IsSuccess)
                    return true;
                else if (IsFail)
                    return false;
                else
                    return null;
            }
            else
            {
                return null;
            }
        }
    }
}