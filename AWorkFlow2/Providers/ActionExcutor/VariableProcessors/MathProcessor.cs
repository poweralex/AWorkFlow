using AWorkFlow2.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AWorkFlow2.Providers.ActionExcutor
{
    /// <summary>
    /// math processor
    /// </summary>
    public class MathProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "Math";

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<MathProcessActionSetting>(actionSetting);
            try
            {
                decimal arg1 = decimal.Parse(argument.Format(setting.Arg1));
                decimal arg2 = decimal.Parse(argument.Format(setting.Arg2));
                decimal res = 0;
                switch (setting.Action.ToLower())
                {
                    case "+":
                        res = arg1 + arg2;
                        break;
                    case "-":
                        res = arg1 - arg2;
                        break;
                    case "*":
                        res = arg1 * arg2;
                        break;
                    case "/":
                        if (arg2 == 0)
                        {
                            return Task.FromResult(new ActionExecuteResult
                            {
                                Fail = true,
                                Message = $"You cannot divide by zero!"
                            });
                        }
                        res = arg1 / arg2;
                        break;
                    case "%":
                        if (arg2 == 0)
                        {
                            return Task.FromResult(new ActionExecuteResult
                            {
                                Fail = true,
                                Message = $"You cannot divide by zero!"
                            });
                        }
                        res = arg1 % arg2;
                        break;
                    case "abs":
                        res = Math.Abs(arg1);
                        break;
                    case "ceiling":
                        res = Math.Ceiling(arg1);
                        break;
                    case "floor":
                        res = Math.Floor(arg1);
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
    public class MathProcessActionSetting
    {
        /// <summary>
        /// action
        /// </summary>
        public string Action { get; set; }
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
