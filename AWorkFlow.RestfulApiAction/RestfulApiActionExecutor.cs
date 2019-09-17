using AWorkFlow.Core.Extensions;
using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AWorkFlow.RestfulApiAction
{
    /// <summary>
    /// call web api type action executor
    /// </summary>
    public class RestfulApiActionExecutor : IExecutor
    {
        /// <summary>
        /// execute call
        /// </summary>
        /// <param name="argument"></param>
        /// <returns></returns>
        public async Task<ExecutionResultDto> Execute(IExpressionProvider expressionProvider, ActionSettingDto action)
        {
            try
            {
                var Settings = JsonConvert.DeserializeObject<RestfulApiActionSetting>(JsonConvert.SerializeObject(action.Settings));
                if (Settings == null)
                {
                    return new ExecutionResultDto();
                }
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Dictionary<string, string> arguments = new Dictionary<string, string>();
                var url = expressionProvider.Format(Settings.Url).Result;
                var call = HttpProvider.CallUrl(url);
                arguments.Add("url", url);
                if (Settings.Headers?.Any() == true)
                {
                    foreach (var kvp in Settings.Headers)
                    {
                        var header = expressionProvider.Format(kvp.Value).Result;
                        call.WithHeader(kvp.Key, header);
                        arguments.Add($"header_{kvp.Key}", header);
                    }
                }
                var acceptCodes = Settings.SuccessStatusCodes.Select(x => (HttpStatusCode)x).Distinct().ToArray();
                call.AcceptStatusCodes(acceptCodes);
                arguments.Add($"acceptStatusCodes", acceptCodes.ToJson());
                if (!string.IsNullOrEmpty(Settings.Body))
                {
                    var body = expressionProvider.Format(Settings.Body).Result;
                    call.WithJsonBody(body.ToObject<object>());
                    arguments.Add($"body", body);
                }
                if (Settings.FormData?.Any() == true)
                {
                    foreach (var kvp in Settings.FormData)
                    {
                        call.WithHttpParameter(kvp.Key, kvp.Value);
                        arguments.Add($"form_{kvp.Key}", kvp.Value);
                    }
                }
                OperationResult<string> result = null;
                arguments.Add($"http_method", Settings.Method);
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
                    sw.Stop();
                    return new ExecutionResultDto
                    {
                        Completed = false,
                        Success = false,
                        Fail = false,
                        ExecuteResult = result?.Data,
                        ExecutionTime = sw.Elapsed,
                        ExecuteArguments = arguments
                    };
                }

                var resultBody = result.Data;
                if (!string.IsNullOrEmpty(resultBody))
                {
                    expressionProvider.Arguments.PutPrivate("result", resultBody);
                }
                var executionResult = new ExecutionResultDto
                {
                    ExecuteResult = result?.Data,
                    ExecuteArguments = arguments
                };
                executionResult.Completed = true;
                if (Settings.ResultIndicators?.Any() == true)
                {
                    foreach (var indicator in Settings.ResultIndicators)
                    {
                        var res = indicator.Indicate(expressionProvider);
                        if (res == null)
                        {
                            continue;
                        }
                        if (res == true)
                        {
                            executionResult.Success = true;
                            break;
                        }
                        if (res == false)
                        {
                            executionResult.Fail = true;
                            break;
                        }
                    }
                }
                else
                {
                    executionResult.Success = true;
                }

                sw.Stop();
                executionResult.ExecutionTime = sw.Elapsed;
                return executionResult;
            }
            catch (Exception)
            {
                return new ExecutionResultDto();
            }
        }
    }

    /// <summary>
    /// web api setting
    /// </summary>
    public class RestfulApiActionSetting
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
}
