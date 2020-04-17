using AWorkFlow2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// groups a list to a new list of [{"groupKey":"","groupItems":[]}]
    /// </summary>
    public class GroupListProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "GroupList";

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<GroupListActionSetting>(actionSetting);

            var listJson = argument.Format(setting.Source);

            try
            {
                var list = JsonHelper.GetArray(listJson, setting.Source);
                var result = new List<Dictionary<string, string>>();
                Dictionary<string, JArray> groupData = new Dictionary<string, JArray>();
                foreach (var listItem in list)
                {
                    var tmpArgument = new ArgumentProvider(argument.WorkingArguments.Copy());
                    tmpArgument.ClearKey("loopItem");
                    tmpArgument.PutPrivate("loopItem", listItem.ToString());
                    var key = tmpArgument.Format(setting.Key);
                    if (groupData.ContainsKey(key))
                    {
                        groupData[key].Add(listItem);
                    }
                    else
                    {
                        groupData[key] = new JArray { listItem };
                    }
                }
                foreach (var kvp in groupData)
                {
                    var tmpArgument = new ArgumentProvider(argument.WorkingArguments.Copy());
                    tmpArgument.ClearKey("groupKey");
                    tmpArgument.ClearKey("groupItems");
                    tmpArgument.PutPrivate("groupKey", kvp.Key.ToString());
                    tmpArgument.PutPrivate("groupItems", kvp.Value.ToString());
                    Dictionary<string, string> results = new Dictionary<string, string>();
                    foreach (var rule in setting.Output)
                    {
                        results.Add(rule.Key, tmpArgument.Format(rule.Value));
                    }
                    result.Add(results);
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
    public class GroupListActionSetting
    {
        /// <summary>
        /// source list expression
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// key expression
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// mapping rules
        /// </summary>
        public Dictionary<string, string> Output { get; set; }
    }

}
