using AWorkFlow2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
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

                var result = await CallHttp(argument);
                if (result?.Success != true)
                {
                    return new ActionExecuteResult { Success = result?.Success ?? false, Message = result?.Message, Data = result?.Message };
                }

                var resultBody = result.Data;
                if (!string.IsNullOrEmpty(resultBody))
                {
                    argument.PutPrivate("result", resultBody);
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
                        else
                        {
                            actionExecuteResult.Fail = true;
                            break;
                        }
                    }
                }
                else
                {
                    actionExecuteResult.Success = true;
                    actionExecuteResult.Fail = false;
                }

                return actionExecuteResult;
            }
            catch (Exception ex)
            {
                return new ActionExecuteResult { Success = false, Message = ex.Message };
            }
        }

        private async Task<OperationResult<string>> CallHttp(ArgumentProvider argument)
        {
            var url = argument.Format(Settings.Url);
            var call = HttpProvider.CallUrl(url);
            var headers = new List<KeyValuePair<string, string>>();
            if (Settings.Headers?.Any() == true)
            {
                foreach (var kvp in Settings.Headers)
                {
                    var headerValue = argument.Format(kvp.Value);
                    headers.Add(new KeyValuePair<string, string>(kvp.Key, headerValue));
                    call.WithHeader(kvp.Key, headerValue);
                }
            }
            call.AcceptStatusCodes(Settings.SuccessStatusCodes.Select(x => (HttpStatusCode)x).ToArray());
            string body = string.Empty;
            if (!string.IsNullOrEmpty(Settings.Body))
            {
                body = argument.Format(Settings.Body);
                call.WithJsonBody(body.ToJsonObject());
            }
            if (Settings.FormData?.Any() == true)
            {
                var formData = new List<KeyValuePair<string, string>>();
                foreach (var kvp in Settings.FormData)
                {
                    var formValue = argument.Format(kvp.Value);
                    formData.Add(new KeyValuePair<string, string>(kvp.Key, formValue));
                    call.WithHttpParameter(kvp.Key, formValue);
                }
                body = JsonConvert.SerializeObject(formData);
            }
            var executionData = new
            {
                Settings.Method,
                url,
                headers,
                body
            };
            argument.PutPrivate(nameof(executionData), JsonConvert.SerializeObject(executionData));

            if ("get".Equals(Settings.Method, StringComparison.CurrentCultureIgnoreCase))
            {
                return await call.GetAsync();
            }
            else if ("post".Equals(Settings.Method, StringComparison.CurrentCultureIgnoreCase))
            {
                return await call.PostAsync<string>();
            }
            else if ("put".Equals(Settings.Method, StringComparison.CurrentCultureIgnoreCase))
            {
                return await call.PutAsync<string>();
            }
            else if ("delete".Equals(Settings.Method, StringComparison.CurrentCultureIgnoreCase))
            {
                return await call.DeleteAsync<string>();
            }

            return new OperationResult<string>
            {
                Success = false,
                Code = Messages.UnsupportedHttpMethod.Code,
                Message = Messages.UnsupportedHttpMethod.Message
            };
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
        public Dictionary<string, string> FormData { get; set; }
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
