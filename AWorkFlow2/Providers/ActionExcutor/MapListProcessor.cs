using AWorkFlow2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// Maps a list to a new list
    /// </summary>
    public class MapListProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "MapList";

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<MapListActionSetting>(actionSetting);

            var listJson = argument.Format(setting.Source);

            try
            {
                var list = JArray.Parse(listJson);
                var result = new List<Dictionary<string, string>>();
                foreach (var listItem in list)
                {
                    var tmpArgument = new ArgumentProvider(argument.WorkingArguments.Copy());
                    tmpArgument.ClearKey("mapItem");
                    tmpArgument.PutPrivate("mapItem", listItem.ToString());
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
    public class MapListActionSetting
    {
        /// <summary>
        /// source list expression
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// mapping rules
        /// </summary>
        public Dictionary<string, string> Output { get; set; }
    }

}
