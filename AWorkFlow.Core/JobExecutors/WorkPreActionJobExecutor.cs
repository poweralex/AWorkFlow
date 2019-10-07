using System.Diagnostics;
using System.Threading.Tasks;
using Autofac;
using AWorkFlow.Core.ActionExecutors;
using AWorkFlow.Core.Models;
using AWorkFlow.Core.Providers;

namespace AWorkFlow.Core.JobExecutors
{
    public class WorkPreActionJobExecutor : IJobExecutor
    {
        private readonly IContainer _actionContainer;
        public WorkPreActionJobExecutor(IContainer actionContainer)
        {
            _actionContainer = actionContainer;
        }

        public async Task<JobExecutionResultDto> Execute(JobDto job)
        {
            System.Console.WriteLine($"going to execute WorkPreActionJob");
            Stopwatch sw = new Stopwatch();
            var result = new JobExecutionResultDto { Completed = true, Success = true };
            var actionConfigs = job.Work.WorkFlow.PreActions;
            var expressionProvider = new ExpressionProvider(job.Work.InputData);
            sw.Start();
            foreach (var actionConfig in actionConfigs)
            {
                if (!_actionContainer.IsRegisteredWithName<IActionExecutor>(actionConfig?.ActionType))
                {
                    result.ActionResults.Add(new ActionExecutionResultDto { Completed = true, Fail = true, Message = $"Action type {actionConfig?.ActionType} not registered." });
                    result.Fail = true;
                    result.Completed = false;
                    break;
                }
                var actionExecutor = _actionContainer.ResolveNamed<IActionExecutor>(actionConfig.ActionType);
                var actionResult = await actionExecutor.Execute(expressionProvider, actionConfig);
                result.ActionResults.Add(actionResult);
                if (actionResult.Fail)
                {
                    result.Fail = true;
                    result.Completed = false;
                    break;
                }
            }
            sw.Stop();
            result.ExecutionTime = sw.Elapsed;
            return result;
        }
    }
}
