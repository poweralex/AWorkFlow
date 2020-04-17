using AWorkFlow2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// aggregate list into one value(sum, etc..)
    /// </summary>
    public class AggregateListProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "Aggregate";

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<AggregateActionSetting>(actionSetting);

            var listJson = argument.Format(setting.Source);

            try
            {
                var list = JsonHelper.GetArray(listJson, setting.Source);
                if (string.Equals("sum", setting?.Action, StringComparison.CurrentCultureIgnoreCase))
                {
                    // sum
                    var sum = 0;
                    foreach (var listItem in list)
                    {
                        var tmpArgument = new ArgumentProvider(argument.WorkingArguments.Copy());
                        tmpArgument.ClearKey("sourceItem");
                        tmpArgument.PutPrivate("sourceItem", listItem.ToString());
                        if (setting?.Where?.Any() != true || setting?.Where?.Any(x => x.Indicate(tmpArgument) ?? false) == true)
                        {

                            var targetJson = tmpArgument.Format(setting.Target);
                            sum += targetJson.ToNullableInt() ?? 0;
                        }
                    }
                    return Task.FromResult(new ActionExecuteResult
                    {
                        Success = true,
                        Output = new Dictionary<string, string> { { "result", $"{sum}" } },
                        Data = $"{sum}"
                    });
                }
                else if (string.Equals("count", setting?.Action, StringComparison.CurrentCultureIgnoreCase))
                {
                    // count
                    var count = 0;
                    foreach (var listItem in list)
                    {
                        var tmpArgument = new ArgumentProvider(argument.WorkingArguments.Copy());
                        tmpArgument.ClearKey("sourceItem");
                        tmpArgument.PutPrivate("sourceItem", listItem.ToString());
                        if (setting?.Where?.Any() != true || setting?.Where?.Any(x => x.Indicate(tmpArgument) ?? false) == true)
                        {
                            count++;
                        }
                    }
                    return Task.FromResult(new ActionExecuteResult
                    {
                        Success = true,
                        Output = new Dictionary<string, string> { { "result", $"{count}" } },
                        Data = $"{count}"
                    });
                }

                return Task.FromResult(new ActionExecuteResult
                {
                    Success = false,
                    Output = new Dictionary<string, string> { { "result", "Unknow action" } },
                    Data = "Unknow action"
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
    public class AggregateActionSetting
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
        /// aggregate target
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// aggregate action
        /// </summary>
        public string Action { get; set; }
    }
}
