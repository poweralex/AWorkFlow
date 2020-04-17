using AWorkFlow2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// provide processor for type=VariableProcess and method=CompareNumber
    /// </summary>
    public class CompareNumberProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "CompareNumber";

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<CompareNumberActionSetting>(actionSetting);
            try
            {
                var arg1 = argument.Format(setting.Arg1);
                var arg2 = argument.Format(setting.Arg2);
                if (decimal.TryParse(arg1, out decimal d1)
                    && decimal.TryParse(arg2, out decimal d2))
                {
                    bool result = false;
                    switch (setting.Comparer.ToLower())
                    {
                        case "greater":
                        case ">":
                            result = d1 > d2;
                            break;
                        case "greaterorequal":
                        case ">=":
                            result = d1 >= d2;
                            break;
                        case "equal":
                        case "=":
                            result = d1 == d2;
                            break;
                        case "less":
                        case "<":
                            result = d1 < d2;
                            break;
                        case "lessorequal":
                        case "<=":
                            result = d1 <= d2;
                            break;
                        case "notequal":
                        case "<>":
                        case "!=":
                            result = d1 != d2;
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
                else
                {
                    return Task.FromResult(new ActionExecuteResult
                    {
                        Fail = true,
                        Message = $"Cannot compare {arg1} and {arg2}"
                    });
                }
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
    public class CompareNumberActionSetting
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
    }
}
