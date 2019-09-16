using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AWorkFlow.ConsoleAction
{
    public class ConsoleActionExecutor : IExecutor
    {
        public Task<ExecutionResultDto> Execute(IExpressionProvider expressionProvider, ActionSettingDto action)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string output = string.Empty;
            if (action.Settings != null)
            {
                var Settings = JsonConvert.DeserializeObject<ConsoleActionSetting>(JsonConvert.SerializeObject(action.Settings));
                output = expressionProvider.Format(Settings.OutputExp).Result.ResultJson;
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {output}");
            }
            bool? indicateResult = null;
            if (action.Indicators?.Any() == true)
            {
                foreach (var indicator in action.Indicators)
                {
                    var res = indicator.Indicate(expressionProvider);
                    if (res.HasValue)
                    {
                        indicateResult = res;
                        break;
                    }
                }
            }
            sw.Stop();
            return Task.FromResult(new ExecutionResultDto
            {
                Completed = indicateResult.HasValue,
                Success = indicateResult == true,
                Fail = indicateResult == false,
                ExecuteResult = output,
                ExecutionTime = sw.Elapsed
            });
        }
    }

    public class ConsoleActionSetting
    {
        public string OutputExp { get; set; }
    }
}
