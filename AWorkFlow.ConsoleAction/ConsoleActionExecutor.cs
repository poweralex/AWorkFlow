using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers.Interfaces;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AWorkFlow.ConsoleAction
{
    public class ConsoleActionExecutor : IExecutor
    {
        public Task<ExecutionResultDto> Execute(IExpressionProvider expressionProvider, ActionSettingDto action)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var Settings = JsonConvert.DeserializeObject<ConsoleActionSetting>(JsonConvert.SerializeObject(action.Settings));
            var output = expressionProvider.Format(Settings.OutputExp);
            Console.WriteLine($"{DateTime.Now.ToLongTimeString()}: {output}");
            sw.Stop();
            return Task.FromResult(new ExecutionResultDto
            {
                Success = true,
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
