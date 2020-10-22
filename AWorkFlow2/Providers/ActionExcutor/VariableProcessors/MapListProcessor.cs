using AWorkFlow2.Helps;
using AWorkFlow2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
                var list = JsonHelper.GetArray(listJson, setting.Source);
                var results = new List<Dictionary<string, string>>();
                foreach (var listItem in list)
                {
                    var tmpArgument = new ArgumentProvider(argument.WorkingArguments.Copy());
                    tmpArgument.ClearKey("mapItem");
                    tmpArgument.PutPrivate("mapItem", listItem.ToString());
                    if (setting?.Where?.Any() != true || setting?.Where?.Any(x => x.Indicate(tmpArgument) ?? false) == true)
                    {
                        Dictionary<string, string> result = new Dictionary<string, string>();
                        foreach (var rule in setting.Output)
                        {
                            result.Add(rule.Key, tmpArgument.Format(rule.Value, true));
                        }
                        results.Add(result);
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
    public class MapListActionSetting
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
        public Dictionary<string, string> Output { get; set; }
    }

}
