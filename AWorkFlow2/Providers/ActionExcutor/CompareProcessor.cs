using AWorkFlow2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// provide processor for type=VariableProcess and method=Compare
    /// </summary>
    public class CompareProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "Compare";

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<CompareActionSetting>(actionSetting);
            try
            {
                var arg1 = argument.Format(setting.Arg1);
                var arg2 = argument.Format(setting.Arg2);
                if (setting.IgnoreCase)
                {
                    arg1 = arg1.ToLower();
                    arg2 = arg2.ToLower();
                }
                bool result = false;
                switch (setting.Comparer.ToLower())
                {
                    case "equal":
                    case "=":
                        result = arg1 == arg2;
                        break;
                    case "notequal":
                    case "<>":
                    case "!=":
                        result = arg1 != arg2;
                        break;
                    default:
                        return Task.FromResult(new ActionExecuteResult
                        {
                            Fail = true,
                            Message = $"Comparer ({setting.Comparer}) is not known"
                        });
                }
                return Task.FromResult(new ActionExecuteResult
                {
                    Success = result,
                    Output = new Dictionary<string, string> { { "result", result.ToString() } },
                    Data = result.ToString()
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
    public class CompareActionSetting
    {
        /// <summary>
        /// comparer
        /// </summary>
        public string Comparer { get; set; }
        /// <summary>
        /// arg1 expression
        /// </summary>
        public string Arg1 { get; set; }
        /// <summary>
        /// arg2 expression
        /// </summary>
        public string Arg2 { get; set; }
        /// <summary>
        /// if ignore case when compare
        /// </summary>
        public bool IgnoreCase { get; set; }
    }
}
