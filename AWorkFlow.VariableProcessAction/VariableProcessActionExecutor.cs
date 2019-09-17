using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using AWorkFlow.VariableProcessAction.Processor;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow.VariableProcessAction
{
    /// <summary>
    /// variable process type action executor
    /// </summary>
    public class VariableProcessActionExecutor : IExecutor
    {
        static VariableProcessActionExecutor()
        {
            IocManager.Initialize();
        }
        public async Task<ExecutionResultDto> Execute(IExpressionProvider expressionProvider, ActionSettingDto action)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            ExecutionResultDto res = null;
            if (action.Settings != null)
            {
                var Settings = JsonConvert.DeserializeObject<VariableProcessActionSetting>(JsonConvert.SerializeObject(action.Settings));
                var executor = IocManager.Get<IVariableProcessor>(Settings.Method?.ToUpper());
                res = await executor.Execute(Settings, expressionProvider);
            }
            if (res == null)
            {
                res = new ExecutionResultDto();
            }

            if (action?.Indicators?.Any() == true)
            {
                foreach (var indicator in action.Indicators)
                {
                    var indicateRes = indicator.Indicate(expressionProvider);
                    if (indicateRes.HasValue)
                    {
                        res.Completed = true;
                        res.Success = indicateRes == true;
                        res.Fail = indicateRes == false;
                        break;
                    }
                }
            }
            else
            {
                res.Completed = true;
                res.Success = true;
            }

            sw.Stop();
            res.ExecutionTime = sw.Elapsed;
            return res;
        }
    }

    /// <summary>
    /// variable process action setting model
    /// </summary>
    public class VariableProcessActionSetting
    {
        /// <summary>
        /// method
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// output setting
        /// </summary>
        public Dictionary<string, string> Output { get; set; }
    }
}
