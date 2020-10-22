using AWorkFlow2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// boolean processor
    /// </summary>
    public class BooleanProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "Boolean";

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<BooleanProcessActionSetting>(actionSetting);
            try
            {
                bool arg = bool.Parse(argument.Format(setting.Arg));
                bool res = false;
                switch (setting.Action.ToLower())
                {
                    case "not":
                    case "!":
                        res = !arg;
                        break;
                    default:
                        return Task.FromResult(new ActionExecuteResult
                        {
                            Fail = true,
                            Message = $"Action ({setting.Action}) is not known"
                        });
                }
                return Task.FromResult(new ActionExecuteResult
                {
                    Success = true,
                    Output = new Dictionary<string, string> { { "result", res.ToString() } },
                    Data = res.ToString()
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new ActionExecuteResult
                {
                    Fail = true,
                    Message = ex.Message
                });
            }
        }
    }

    /// <summary>
    /// variable process action setting model
    /// </summary>
    public class BooleanProcessActionSetting
    {
        /// <summary>
        /// comparer
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// arg1 expression
        /// </summary>
        public string Arg { get; set; }
    }
}
