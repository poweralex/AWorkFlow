using AWorkFlow2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// peek fields from a list to a new list: [{"key":"a"},{"key":"b"}] => ["a","b"]
    /// </summary>
    public class PeekListProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "PeekList";

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<PeekListActionSetting>(actionSetting);

            var listJson = argument.Format(setting.Source);

            try
            {
                var list = JsonHelper.GetArray(listJson, setting.Source);
                var results = new List<JToken>();
                foreach (var listItem in list)
                {
                    var tmpArgument = new ArgumentProvider(argument.WorkingArguments.Copy());
                    tmpArgument.ClearKey("sourceItem");
                    tmpArgument.PutPrivate("sourceItem", listItem.ToString());
                    if (setting?.Where?.Any() != true || setting?.Where?.Any(x => x.Indicate(tmpArgument) ?? false) == true)
                    {
                        string targetValue = argument.Format(setting.Target);
                        if (setting.AsString)
                        {
                            results.Add(targetValue);
                        }
                        else
                        {
                            var valueObj = JsonHelper.TryGetObject(targetValue, setting.Target);
                            if (valueObj == null)
                            {
                                results.Add(targetValue);
                            }
                            else
                            {
                                targetValue = JsonConvert.SerializeObject(valueObj);
                                results.Add(valueObj);
                            }
                        }
                    }
                }

                var resultStr = JsonConvert.SerializeObject(results);
                return Task.FromResult(new ActionExecuteResult
                {
                    Success = true,
                    Output = new Dictionary<string, string> { { "result", resultStr } },
                    Data = resultStr
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ActionExecuteResult
                {
                    Fail = true,
                    Message = ex.Message,
                    Output = new Dictionary<string, string> { },
                    Data = string.Empty
                });
            }
        }
    }


    /// <summary>
    /// variable process action setting model
    /// </summary>
    public class PeekListActionSetting
    {
        /// <summary>
        /// source list expression
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// filter condition(s)
        /// </summary>
        public List<ResultIndicator> Where { get; set; }
        /// <summary>
        /// mapping rules
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// peek as string
        /// </summary>
        public bool AsString { get; set; }
    }

}
