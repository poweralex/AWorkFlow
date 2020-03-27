using AWorkFlow2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// expand list B in list A into a single level list
    /// </summary>
    public class ExpandProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "Expand";

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<ExpandActionSetting>(actionSetting);

            var listJson = argument.Format(setting.Source);

            try
            {
                var list = JArray.Parse(listJson);
                var result = new List<Dictionary<string, string>>();
                foreach (var listItem in list)
                {
                    var tmpArgument = new ArgumentProvider(argument.WorkingArguments.Copy());
                    tmpArgument.ClearKey("sourceItem");
                    tmpArgument.PutPrivate("sourceItem", listItem.ToString());
                    var target = setting.Target;
                    if (target.StartsWith("{{") && target.EndsWith("}}"))
                    {
                        target = target.Substring(2, target.Length - 4);
                    }

                    var targetJson = tmpArgument.Format($"{{{{sourceItem.{target}}}}}");
                    var targetList = JArray.Parse(targetJson);
                    foreach (var targetItem in targetList)
                    {
                        var tmpArgument2 = new ArgumentProvider(tmpArgument.WorkingArguments.Copy());
                        tmpArgument.ClearKey("targetItem");
                        tmpArgument2.PutPrivate("targetItem", targetItem.ToString());

                        Dictionary<string, string> results = new Dictionary<string, string>();
                        foreach (var rule in setting.Output)
                        {
                            results.Add(rule.Key, tmpArgument2.Format(rule.Value));
                        }
                        result.Add(results);
                    }
                }

                var resultStr = JsonConvert.SerializeObject(result);
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
    public class ExpandActionSetting
    {
        /// <summary>
        /// source list expression
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// target list to expand in source list
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// mapping rules
        /// </summary>
        public Dictionary<string, string> Output { get; set; }
    }

}
