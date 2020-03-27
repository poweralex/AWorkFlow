using AWorkFlow2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// provide processor for type=VariableProcess and method=CountList
    /// </summary>
    public class CountListProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "CountList";

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<CountListActionSetting>(actionSetting);

            var listJson = argument.Format(setting.List);

            try
            {
                var list = JArray.Parse(listJson);

                return Task.FromResult(new ActionExecuteResult
                {
                    Success = true,
                    Output = new Dictionary<string, string> { { "count", $"{ list.Count}" } },
                    Data = $"{list.Count}"
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ActionExecuteResult
                {
                    Fail = true,
                    Message = ex.Message,
                    Output = new Dictionary<string, string> { { "count", "0" } },
                    Data = "0"
                });
            }
        }
    }

    /// <summary>
    /// variable process action setting model
    /// </summary>
    public class CountListActionSetting
    {
        /// <summary>
        /// list expression
        /// </summary>
        public string List { get; set; }
    }
}
