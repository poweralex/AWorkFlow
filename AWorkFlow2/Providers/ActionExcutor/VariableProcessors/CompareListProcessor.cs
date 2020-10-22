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
    /// compare list to match compare action
    /// </summary>
    public class CompareListProcessor : IVariableProcessor
    {
        /// <summary>
        /// method
        /// </summary>
        public static readonly string Method = "CompareList";

        /// <summary>
        /// execute
        /// </summary>
        /// <param name="actionSetting"></param>
        /// <param name="argument"></param>
        /// <returns></returns>
        public Task<ActionExecuteResult> Execute(string actionSetting, ArgumentProvider argument)
        {
            var setting = JsonConvert.DeserializeObject<CompareListActionSetting>(actionSetting);
            try
            {
                var listStr = argument.Format(setting.Source);
                var list = JsonHelper.GetArray(listStr, setting.Source);
                string methodSetting = JsonConvert.SerializeObject(setting.ActionSetting);

                bool result = false;
                switch (setting.Comparer)
                {
                    case "any":
                        result = list.Any(x =>
                        {
                            var tmpArgument = new ArgumentProvider(argument.WorkingArguments.Copy());
                            tmpArgument.ClearKey("sourceItem");
                            tmpArgument.PutPrivate("sourceItem", x.ToString());

                            return ExecuteAction(setting.Action, methodSetting, tmpArgument).Result?.Success == true;
                        });
                        break;
                    case "all":
                        result = list.All(x =>
                        {
                            var tmpArgument = new ArgumentProvider(argument.WorkingArguments.Copy());
                            tmpArgument.ClearKey("sourceItem");
                            tmpArgument.PutPrivate("sourceItem", x.ToString());

                            return ExecuteAction(setting.Action, methodSetting, tmpArgument).Result?.Success == true;
                        });
                        break;
                    case "notany":
                        result = !list.Any(x =>
                        {
                            var tmpArgument = new ArgumentProvider(argument.WorkingArguments.Copy());
                            tmpArgument.ClearKey("sourceItem");
                            tmpArgument.PutPrivate("sourceItem", x.ToString());

                            return ExecuteAction(setting.Action, methodSetting, tmpArgument).Result?.Success == true;
                        });
                        break;
                    case "notall":
                        result = !list.All(x =>
                        {
                            var tmpArgument = new ArgumentProvider(argument.WorkingArguments.Copy());
                            tmpArgument.ClearKey("sourceItem");
                            tmpArgument.PutPrivate("sourceItem", x.ToString());

                            return ExecuteAction(setting.Action, methodSetting, tmpArgument).Result?.Success == true;
                        });
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
                    Success = true,
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

        private Task<ActionExecuteResult> ExecuteAction(string action, string actionSetting, ArgumentProvider argument)
        {
            IVariableProcessor comparer = null;
            if (string.Equals(action, CompareProcessor.Method, StringComparison.CurrentCultureIgnoreCase))
            {
                comparer = new CompareProcessor();
            }
            else if (string.Equals(action, CompareNumberProcessor.Method, StringComparison.CurrentCultureIgnoreCase))
            {
                comparer = new CompareNumberProcessor();
            }

            if (comparer == null)
            {
                throw new ArgumentOutOfRangeException(nameof(action), $"Only {CompareProcessor.Method} and {CompareNumberProcessor.Method} are supported for now.");
            }
            return comparer.Execute(actionSetting, argument);
        }
    }

    /// <summary>
    /// variable process action setting model
    /// </summary>
    public class CompareListActionSetting
    {
        /// <summary>
        /// comparer(any,all,notany,notall)
        /// </summary>
        public string Comparer { get; set; }
        /// <summary>
        /// list expression
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// compare action(Compare, CompareNumber)
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// compare action setting
        /// </summary>
        public object ActionSetting { get; set; }
    }
}
